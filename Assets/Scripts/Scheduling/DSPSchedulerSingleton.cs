using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class DSPSchedulerSingleton : MonoBehaviour
{
    private static AudioSource _audioSource;
    private static int _sampleRate;
    private static List<ScheduledEvent> _scheduledEvents = new List<ScheduledEvent>();
    private List<int> _indicesToRemove = new List<int>();
    private static DSPSchedulerSingleton _instance;


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if(_instance == null) {
            var go = new GameObject("DSPScheduler");
            _instance = go.AddComponent<DSPSchedulerSingleton>();
            _audioSource = go.AddComponent<AudioSource>();
            _audioSource.Play();
            _sampleRate = AudioSettings.outputSampleRate;
            DontDestroyOnLoad(_instance.gameObject);
        }
    }

    public static void Schedule(ScheduledEvent scheduledEvent)
    {
        lock(_scheduledEvents) {
            // Debug.Log("Adding schedulable with id " + scheduledEvent.Id + " to the scheduler");
            _scheduledEvents.Add(scheduledEvent);
        }
    }

    public static void Unschedule(ScheduledEvent scheduable)
    {
        if (!_scheduledEvents.Contains(scheduable)) {
            throw new ArgumentException("Scheduable not found");
        }
        lock(_scheduledEvents) {
            _scheduledEvents.Remove(scheduable);
        }

    }

    private void OnAudioFilterRead(float[] data, int channels) {

        if (_scheduledEvents.Count == 0) return;

        // schedule any events at least 1 block ahead of time
        double bufferStartTime = AudioSettings.dspTime;
        double bufferEndTime = bufferStartTime + (((float)data.Length / channels) / (double)_sampleRate);

        // if any of the Scheduables are within the next block
        // we can schedule them to be played in the next block
        lock(_scheduledEvents) {
            _indicesToRemove.Clear();

            // _scheduledEvents = _scheduledEvents.RemoveAll(s => s.IsCancelled);

            var scheduablesToDispatch = _scheduledEvents.Where(s => s.OccursBefore(bufferEndTime));

            // Debug.Log("Number of scheduled events: " + _schedulables.Count());
            
            if (scheduablesToDispatch.Count() == 0) return;

            foreach(var scheduledEvent in scheduablesToDispatch) {
                if (!scheduledEvent.IsCancelled) {
                    Dispatcher.RunOnMainThread(() => scheduledEvent.Invoke());
                }
                _indicesToRemove.Add(_scheduledEvents.IndexOf(scheduledEvent));
            }

            // _scheduledEvents.RemoveAll(s => s.OccursBefore(bufferEndTime));
            
            // remove the scheduables that have been dispatched
            foreach(var index in _indicesToRemove.OrderByDescending(v => v)) {
                // Debug.Log("Removing scheduable at index " + index + " length of schedulables is " + _schedulables.Count);
                _scheduledEvents.RemoveAt(index);
            }
        }
    }
}