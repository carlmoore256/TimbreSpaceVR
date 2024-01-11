using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class DSPScheduler : MonoBehaviour
{
    public Guid OwnerId { get; private set; }
    private AudioSource _audioSource;
    private int _sampleRate;
    private List<ScheduledEvent> _schedulables = new List<ScheduledEvent>();
    private List<int> _indicesToRemove = new List<int>();

    public void Initialize(Guid ownerId)
    {
        OwnerId = ownerId;
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.Play();
        _sampleRate = AudioSettings.outputSampleRate;
    }

    public static DSPScheduler CreateScheduler(Guid ownerId)
    {
        var go = new GameObject("DSPScheduler");
        var scheduler = go.AddComponent<DSPScheduler>();
        scheduler.Initialize(ownerId);
        DontDestroyOnLoad(go);
        return scheduler;
    }

    int scheduleIdx = 0;
    public void Schedule(ScheduledEvent scheduable)
    {
        lock(_schedulables) {
            // Debug.Log("Adding schedulable " + scheduable.ScheduleTime);
            _schedulables.Add(scheduable);
        }
    }

    public void Unschedule(ScheduledEvent scheduable)
    {
        if (!_schedulables.Contains(scheduable)) {
            throw new ArgumentException("Scheduable not found");
        }
        lock(_schedulables) {
            _schedulables.Remove(scheduable);
        }

    }

    private void OnAudioFilterRead(float[] data, int channels) {

        if (_schedulables.Count == 0) return;

        // schedule any events at least 1 block ahead of time
        double bufferStartTime = AudioSettings.dspTime;
        double bufferEndTime = bufferStartTime + (((float)data.Length / channels) / (double)_sampleRate);

        // if any of the Scheduables are within the next block
        // we can schedule them to be played in the next block
        lock(_schedulables) {
            _indicesToRemove.Clear();
            var scheduablesToDispatch = _schedulables.Where(s => s.OccursBefore(bufferEndTime));
            // var scheduablesToDispatch = _schedulables.Where(s => s.OccursBetween(bufferStartTime, bufferEndTime));
            
            if (scheduablesToDispatch.Count() == 0) return;

            foreach(var scheduable in scheduablesToDispatch) {
                // dispatcher.Dispatch(scheduable);
                Dispatcher.RunOnMainThread(() => scheduable.Invoke());
                _indicesToRemove.Add(_schedulables.IndexOf(scheduable));
            }

            // remove the scheduables that have been dispatched
            foreach(var index in _indicesToRemove.OrderByDescending(v => v)) {
                // Debug.Log("Removing scheduable at index " + index + " length of schedulables is " + _schedulables.Count);
                _schedulables.RemoveAt(index);
            }
        }
    }

}
