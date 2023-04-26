using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;


public class DSPScheduler : MonoBehaviour
{
    private static AudioSource _audioSource;
    private static int _sampleRate = AudioSettings.outputSampleRate;
    private static List<ScheduledEvent> _schedulables = new List<ScheduledEvent>();
    private List<int> _indicesToRemove = new List<int>();

    private static DSPScheduler _instance;

    // public static DSPScheduler CreateScheduler(Guid ownerId)
    // {

    // }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if(_instance == null) {
            var go = new GameObject("DSPScheduler");
            _instance = go.AddComponent<DSPScheduler>();
            _audioSource = go.AddComponent<AudioSource>();
            _audioSource.Play();
            _sampleRate = AudioSettings.outputSampleRate;
            DontDestroyOnLoad(_instance.gameObject);
        }
    }

    int scheduleIdx = 0;
    public static void Schedule(ScheduledEvent scheduable)
    {
        lock(_schedulables) {
            // Debug.Log("Adding schedulable " + scheduable.ScheduleTime);
            _schedulables.Add(scheduable);
        }
    }

    public static void Unschedule(ScheduledEvent scheduable)
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