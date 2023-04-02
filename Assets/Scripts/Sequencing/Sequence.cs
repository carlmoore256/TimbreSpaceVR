using UnityEngine;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;


public interface ISequenceObserver
{
    void OnSequenceUpdated(IEnumerable<SequenceItem> sequenceItems);
}

public class Sequence : ISequenceable {

    public int ID { get => 0; }
    private List<SequenceItem> sequenceItems;
    private List<ISequenceObserver> observers;

    private CancellationTokenSource cancellationTokenSource;
    private Task playbackTask;
    private readonly object syncObj = new object();
    
    public Sequence() {
        sequenceItems = new List<SequenceItem>();
        observers = new List<ISequenceObserver>();
    }

    public void Add(ISequenceable sequenceable, float gain = 1.0f, bool isMuted = false) {
        sequenceItems.Add(new SequenceItem { sequenceable = sequenceable, gain = gain, isMuted = isMuted });
        NotifyObservers();
    }

    public void Add(IEnumerable<ISequenceable> sequenceables, float gain = 1.0f, bool isMuted = false) {
        foreach (var sequenceable in sequenceables) {
            sequenceItems.Add(new SequenceItem { sequenceable = sequenceable, gain = gain, isMuted = isMuted });
        }
        NotifyObservers();
    }

    public void Remove(ISequenceable sequenceable) {
        sequenceItems.RemoveAll(item => item.sequenceable == sequenceable);
        NotifyObservers();
    }

    public void Play(float gain) {
        cancellationTokenSource = new CancellationTokenSource();
        playbackTask = PlaySequence(gain, cancellationTokenSource.Token);
    }
    
    public void Stop()
    {
        cancellationTokenSource?.Cancel();
    }

    private async Task PlaySequence(float gain, CancellationToken cancellationToken) {
        List<SequenceItem> sortedSequenceItems = new List<SequenceItem>(sequenceItems);
        sortedSequenceItems.Sort((a, b) => a.startTime.CompareTo(b.startTime));

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        
        int currentItemIndex = 0;

        while (currentItemIndex < sortedSequenceItems.Count)
        {
            if (cancellationToken.IsCancellationRequested) break;

            SequenceItem item;
            lock (syncObj)
            {
                item = sortedSequenceItems[currentItemIndex];
            }

            // SequenceItem item = sortedSequenceItems[currentItemIndex];
            // UnityEngine.Debug.Log("Current time: " + stopwatch.Elapsed.TotalSeconds + ", next item starts at " + item.startTime + ", difference: " + (item.startTime - stopwatch.Elapsed.TotalSeconds) + ", index: " + currentItemIndex);

            // There is a big problem here, because when we change BPM, the stopwatch needs to be reset
            if (stopwatch.Elapsed.TotalSeconds >= item.startTime)
            {
                if (!item.isMuted)
                {
                    // UnityEngine.Debug.Log("Playing " + item.sequenceable.ID + " at " + item.startTime + " with gain " + gain * item.gain);
                    Dispatcher.RunOnMainThread(() => item.sequenceable.Play(gain * item.gain));
                }
                currentItemIndex++;
            }
            else
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1));
            }
        }
        
        stopwatch.Stop();
    }

    public void SetBPM(float bpm) {
        double beatsPerSecond = (double)bpm / 60d;
        double timePerBeat = 1d / beatsPerSecond;
        double currentTime = 0d;
        
        lock (syncObj)
        {
            for (int i = 0; i < sequenceItems.Count; i++) {
                sequenceItems[i].SetSequenceTime(currentTime, currentTime + timePerBeat);
                currentTime += timePerBeat;
            }
        }
    }
    
    public void AddObserver(ISequenceObserver observer)
    {
        observers.Add(observer);
        UnityEngine.Debug.Log("ADDED OBSERVER " + observer + " new length of observers: " + observers.Count);
    }

    public void RemoveObserver(ISequenceObserver observer)
    {
        observers.Remove(observer);
    }

    // Call this method whenever the sequence is updated
    private void NotifyObservers()
    {
        UnityEngine.Debug.Log("Notifying " + observers.Count + " observers for sequence");
        foreach (var observer in observers)
        {
            UnityEngine.Debug.Log("Notifying observer " + observer);
            observer.OnSequenceUpdated(sequenceItems);
        }
    }
}













// foreach (SequenceItem item in sequenceItems) {
//     if (!item.isMuted) {
//         double delay = item.startTime - stopwatch.Elapsed.TotalSeconds;
//         if (delay > 0) {
//             await Task.Delay(TimeSpan.FromSeconds(delay));
//         }
//         Dispatcher.RunOnMainThread(() => item.sequenceable.Play(gain * item.gain));
//     }
// }
