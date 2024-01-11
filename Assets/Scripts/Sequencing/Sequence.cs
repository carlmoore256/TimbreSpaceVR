using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;

// public interface ISequenceObserver
// {
//     void OnSequenceUpdated(IEnumerable<SequenceItem> sequenceItems);

//     void OnSequenceAdvance(SequenceItem sequenceItem);
// }

// public interface ISequenceSubject
// {
//     void AddObserver(ISequenceObserver observer);
//     void RemoveObserver(ISequenceObserver observer);
//     void NotifyObservers();
// }



public class Sequence : ISequenceable, IEnumerable<SequenceItem>
{
    public List<SequenceItem> SequenceItems { get; private set; } = new List<SequenceItem>(); // make sure 
    // that this will return an ordered list based on the item's RelativePlayTime
    public int Count => SequenceItems.Count;
    public SequenceItem this[int index] => SequenceItems[index];


    public event Action<SequenceItem> OnSequenceAdvance;
    
    public event Action OnSequenceAdded;
    public event Action OnSequenceRemoved;
    public event Action OnSequenceCleared;

    // public event EventHandler OnSequenceUpdated;
    
    
    public RhythmClock Clock { get; private set; } = new RhythmClock();

    public bool IsPlaying { get; private set; } = false;

    private List<ScheduleCancellationToken> _cancellationTokens = new List<ScheduleCancellationToken>();
    private SequenceableParameters _lastPlayParameters;

    public Sequence()
    {
        Clock.OnTempoChanged += () => {
            if (IsPlaying) {
                Stop(); Resume();
            }
        };
        Clock.OnTimeSignatureChanged += CalculatePlayTimes;
    }



    private void CalculatePlayTimes()
    {
        UnityEngine.Debug.Log("Recalculating Play Times!");
        foreach(var item in SequenceItems)
        {
            if (item.BeatIndex == null) continue;
            item.RelativePlayTime = Clock.TimeFromBeatIndex(item.BeatIndex);
        }
    }

    # region Add/Remove Sequence Items

    public void AddSequenceable(ISequenceable sequenceable) {
        SequenceItems.Add(new SequenceItem { Sequenceable = sequenceable });
        OnSequenceAdded?.Invoke();
    }

    public void AddSequenceableRange(IEnumerable<ISequenceable> sequenceables) {
        foreach (var sequenceable in sequenceables) {
            SequenceItems.Add(new SequenceItem { Sequenceable = sequenceable });
            OnSequenceAdded?.Invoke();
        }
    }

    public void AddSequenceItem(SequenceItem sequenceItem) {
        SequenceItems.Add(sequenceItem);
        OnSequenceAdded?.Invoke();
    }

    public void AddSequenceableAtTime(ISequenceable sequenceable, double time, float gain=1.0f)
    {
        SequenceItem sequenceItem = new SequenceItem
        {
            Sequenceable = sequenceable,
            RelativePlayTime = time
        };
        SequenceItems.Add(sequenceItem);
        OnSequenceAdded?.Invoke();
    }

    public void AddSequenceableAtBeat(ISequenceable sequenceable, float beat, float gain=1.0f)
    {
        AddSequenceableAtTime(sequenceable, Clock.BeatPositionToTime(beat), gain);
    }

    public void AddSequenceableAtNoteValue(ISequenceable sequenceable, int bar, NoteValue noteValue, int noteValuePosition, float gain=1.0f)
    {
        double time = Clock.TimeFromNotePosition(bar, noteValue, noteValuePosition);
        AddSequenceableAtTime(sequenceable, time, gain);
    }

    public void AddSequenceableAtBeatIndex(ISequenceable sequenceable, BeatIndex beat, SequenceableParameters parameters=null)
    {
        // UnityEngine.Debug.Log("Adding sequenceable at beat index " + beat.ToString() + " with time " + Clock.TimeFromBeatIndex(beat).ToString("F3") + " seconds");
        SequenceItem sequenceItem = new SequenceItem
        {
            Sequenceable = sequenceable,
            BeatIndex = beat,
            RelativePlayTime = Clock.TimeFromBeatIndex(beat)
        };
        if (parameters != null) {
            sequenceItem.Parameters = parameters;
        }
        SequenceItems.Add(sequenceItem);
        OnSequenceAdded?.Invoke();
    }



    public void Remove(ISequenceable sequenceable) {
        SequenceItems.RemoveAll(item => item.Sequenceable == sequenceable);
        OnSequenceRemoved?.Invoke();
    }

    public void Clear() {
        SequenceItems.Clear();
        OnSequenceCleared?.Invoke();
    }

    public IEnumerable<SequenceItem> SequenceItemsBetween(BeatIndex start, BeatIndex end) {
        return SequenceItems.Where(item => item.BeatIndex >= start && item.BeatIndex < end);
    }

    # endregion

    /// <summary>
    /// Cancel all scheduled events by this sequence
    /// </summary>
    public void Stop()
    {
        IsPlaying = false;
        foreach(var token in _cancellationTokens) {
            token.Cancel();
        }
        _cancellationTokens.Clear();
    }

    public void Resume()
    {
        // basically just start playing again from where clock is
        double scheduleTime = AudioSettings.dspTime - Clock.CurrentBeatTime;
        Schedule(scheduleTime, _lastPlayParameters);
    }

    public void Play(float gain=1f)
    {
        Schedule(AudioSettings.dspTime, new SequenceableParameters { Gain = gain });
    }

    public void Play(SequenceableParameters parameters)
    {
        Schedule(AudioSettings.dspTime, parameters);
    }

    public void Restart()
    {
        Stop();
        Play();
    }
    
    public void RunUpdate()
    {
        Stop();
        Resume();        
    }

    /// Reschedules a single item
    public void RescheduleItem(SequenceItem item, BeatIndex newBeatIndex)
    {
        item.ScheduledPlayEvent.Cancel();
        item.RelativePlayTime = Clock.TimeFromBeatIndex(newBeatIndex);
        item.BeatIndex = newBeatIndex;
        var absolutePlayTime = AudioSettings.dspTime + item.RelativePlayTime - Clock.CurrentBeatTime;
        var token = item.Sequenceable.Schedule(
            absolutePlayTime, 
            item.Parameters
        );
        item.ScheduledPlayEvent = new ScheduledEvent(
            absolutePlayTime,
            () => AdvanceTo(item),
            token
        );
        DSPSchedulerSingleton.Schedule(item.ScheduledPlayEvent);
        _cancellationTokens.Add(token);
    }


    # region ISequenceable

    public Guid Id { get; protected set; } = Guid.NewGuid();

    public ScheduleCancellationToken Schedule(double scheduleTime, SequenceableParameters parameters) 
    {
        UnityEngine.Debug.Log("Scheduling sequence at time " + scheduleTime.ToString("F3") + " seconds");
        List<ScheduleCancellationToken> cancellationTokens = new List<ScheduleCancellationToken>();
        
        _lastPlayParameters = parameters;
        CalculatePlayTimes();

        double currentTime = AudioSettings.dspTime;
        
        // order sequence items by play time and filter out any where relativePlayTime is less than 0
        var validSequenceItems = SequenceItems.OrderBy(item => item.RelativePlayTime)
                                              .Where(item => item.RelativePlayTime >= 0)
                                              .Where(item => scheduleTime + item.RelativePlayTime >= currentTime); // make sure not
                                                                                                                    // to schedule
                                                                                                                    // anything in the past
        if (validSequenceItems.Count() == 0) {
            UnityEngine.Debug.Log("[!] Error: No valid sequence items to schedule for sequence " + Id.ToString());
            return null;
        }

        var firstScheduledItem = validSequenceItems.FirstOrDefault();
        var lastScheduledItem = validSequenceItems.LastOrDefault();
  
        
        // here calculate if there are any SequenceItems with a BeatIndex, and if so,
        // take the last one and make sure that this Sequence's OnSequenceablePlayEnd fires on the final bar
        var startTime = scheduleTime + firstScheduledItem.RelativePlayTime;
        var lastBar = validSequenceItems.Where(item => item.BeatIndex != null)
                                        .LastOrDefault()?.BeatIndex.Bar ?? -1;

        double test = Clock.TimeFromBars(lastBar + 1);

        double endTime = lastBar == -1 ? scheduleTime + lastScheduledItem.RelativePlayTime : scheduleTime + Clock.TimeFromBars(lastBar + 1);
        

        var startToken = new ScheduleCancellationToken(() => UnityEngine.Debug.Log("Cancelling start token"));
        var endToken = new ScheduleCancellationToken(() => UnityEngine.Debug.Log("Cancelling end token"));

        cancellationTokens.Add(startToken);
        cancellationTokens.Add(endToken);

        ScheduledEvent.CreateAndSchedule(startTime, SequenceablePlayStart, startToken);
        ScheduledEvent.CreateAndSchedule(endTime, SequenceablePlayEnd, endToken);
    
        // iteare through the sequence items, and schedule them relative to the 
        // scheduled playtime of this sequence that has been requested
        foreach(var item in validSequenceItems) {

            var absolutePlayTime = scheduleTime + item.RelativePlayTime;

            // noob issue of having item go out of scope,
            // so we need to make sure we capture it in a local variable
            // remember this for later because it was hard to debug
            // BE CAREFUL WHEN ASSIGNING LAMBDAS WITH THE CONTEXT OF LOCAL VARS
            var currentItem = item;

            SequenceableParameters mergedParameters = currentItem.Parameters.Merge(parameters);

            var token = currentItem.Sequenceable.Schedule(
                absolutePlayTime, 
                mergedParameters
            );

            currentItem.Parameters.Color = mergedParameters.Color;

            // what if instead of relying on the audiobuffer to register the OnSequenceablePlayStart and End, we separately 
            // schedule these alongside it. That way, sequence is always in control of those events. 

            // make new scheduled event, set the cancellation token as the same
            // so cancelling one cancels both. This reduces the issues with having a duality
            // of this event, and the one that goes to the audio system. If they're both canceled,
            // that's that
            currentItem.ScheduledPlayEvent = new ScheduledEvent(
                absolutePlayTime,
                () => AdvanceTo(currentItem),
                token
            );

            // now schedule the event. It will happen within the frame around the same audio block
            DSPSchedulerSingleton.Schedule(currentItem.ScheduledPlayEvent);
            cancellationTokens.Add(token);
        }

        // wrap these tokens up in a new token
        var sequenceableToken = new ScheduleCancellationToken(() => {
            UnityEngine.Debug.Log("Cancelling sequenceable token");
            foreach(var token in cancellationTokens) {
                token.Cancel();
            }
        });

        _cancellationTokens.Add(sequenceableToken);
        OnSchedule?.Invoke(this, (scheduleTime, parameters, sequenceableToken));
        return sequenceableToken;
    }

    private void AdvanceTo(SequenceItem currentItem)
    {
        // Trigger the sequence advance here
        currentItem.Sequenceable.SequenceablePlayStart();
        OnSequenceAdvance?.Invoke(currentItem);
        Clock.CurrentBeatTime = currentItem.RelativePlayTime;
    }

    public event EventHandler<(double, SequenceableParameters, ScheduleCancellationToken)> OnSchedule;

    // **** MAY NO LONGER NEED THESE 
    public event Action OnSequenceablePlayStart;
    public event Action OnSequenceablePlayEnd;

    // ******************************

    public void SequenceablePlayStart() {
        UnityEngine.Debug.Log("CALLING SEQUENCE SEQUENCEABLE PLAY ~START~ |>");
        OnSequenceablePlayStart?.Invoke();
        IsPlaying = true;
    }

    public void SequenceablePlayEnd() {
        UnityEngine.Debug.Log("CALLING SEQUENCE SEQUENCEABLE PLAY ~END~");
        OnSequenceablePlayEnd?.Invoke();
        IsPlaying = false;
    }


    # endregion

    private Action _activeLoop = null;
    

    public void Loop() 
    {

        Action loopAction = null;
        loopAction = () => {
            Play();
        };
        OnSequenceablePlayEnd += loopAction;
    }

    public void Loop(int loopCount) 
    {
        int currentLoopCount = 0;
        Action loopAction = null;
        loopAction = () => {
            currentLoopCount++;
            if (currentLoopCount < loopCount) {
                Play();
            } else {
                OnSequenceablePlayEnd -= loopAction;
            }
        };
        OnSequenceablePlayEnd += loopAction;
    }

    public void Loop(float loopTime) 
    {
        double loopEndTime = AudioSettings.dspTime + loopTime;
        Action loopAction = null;
        loopAction = () => {
            if (AudioSettings.dspTime < loopEndTime) {
                Play();
            } else {
                OnSequenceablePlayEnd -= loopAction;
            }
        };
        OnSequenceablePlayEnd += loopAction;
    }

    public void CancelLoop() {
        OnSequenceablePlayEnd -= _activeLoop;
    }


    
    public IEnumerator<SequenceItem> GetEnumerator() {
        return SequenceItems.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

