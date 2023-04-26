using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;


// public class DSPScheduler : MonoBehaviour
// {
//     static DSPScheduler _instance;
//     static AudioSource _audioSource;
//     static int _sampleRate;

//     // private static List<ScheduledEvent> _schedulables = new List<ScheduledEvent>();
//     private static Dictionary<Guid, List<ScheduledEvent>> _schedulablesById = new Dictionary<Guid, List<ScheduledEvent>>();

//     [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
//     private static void Initialize()
//     {
//         if(_instance == null) {
//             var go = new GameObject("DSPScheduler");
//             _instance = go.AddComponent<DSPScheduler>();
//             _audioSource = go.AddComponent<AudioSource>();
//             _audioSource.Play();
//             _sampleRate = AudioSettings.outputSampleRate;
//             DontDestroyOnLoad(_instance.gameObject);
//         }
//     }

//     public static void Schedule(ScheduledEvent scheduable, Guid ownerId)
//     {
//         if (!_schedulablesById.ContainsKey(ownerId)) {
//             _schedulablesById.Add(ownerId, new List<ScheduledEvent>());
//         }
//         lock(_schedulablesById[ownerId]) {
//             _schedulablesById[ownerId].Add(scheduable);
//         }
//     }

//     public void Unschedule(ScheduledEvent scheduable, Guid ownerId)
//     {
//         if (!_schedulablesById.ContainsKey(ownerId)) {
//             throw new ArgumentException("Scheduable with id " + ownerId + " not found");
//         }

//         if (!_schedulablesById[ownerId].Contains(scheduable)) {
//             throw new ArgumentException("Scheduable not found");
//         }
//         lock(_schedulablesById[ownerId]) {
//             _schedulablesById[ownerId].Remove(scheduable);
//         }
//     }


//     public void UnscheduleByOwner(Guid ownerId) {
//         if (!_schedulablesById.ContainsKey(ownerId)) {
//             throw new ArgumentException("Scheduable with id " + ownerId + " not found");
//         }
//         lock(_schedulablesById[ownerId]) {
//             _schedulablesById[ownerId].Clear();
//         }
//     }

//     private List<int> _indicesToRemove = new List<int>();
//     private void OnAudioFilterRead(float[] data, int channels) {

//         if (_schedulablesById.Count == 0) return;

//         // schedule any events at least 1 block ahead of time
//         double bufferStartTime = AudioSettings.dspTime;
//         double bufferEndTime = bufferStartTime + (((float)data.Length / channels) / (double)_sampleRate);

//         // if any of the Scheduables are within the next block
//         // we can schedule them to be played in the next block

//         foreach(var ownerId in _schedulablesById.Keys) {
//             lock(_schedulablesById[ownerId]) {
//                 _indicesToRemove.Clear();
//                 var scheduablesToDispatch = _schedulablesById[ownerId].Where(s => s.OccursBefore(bufferEndTime));
//                 // var scheduablesToDispatch = _schedulables.Where(s => s.OccursBetween(bufferStartTime, bufferEndTime));
                
//                 if (scheduablesToDispatch.Count() == 0) return;

//                 foreach(var scheduable in scheduablesToDispatch) {
//                     // dispatcher.Dispatch(scheduable);
//                     Dispatcher.RunOnMainThread(() => scheduable.Invoke());
//                     _indicesToRemove.Add(_schedulablesById[ownerId].IndexOf(scheduable));
//                 }
//                 // _schedulables.Remove(scheduable);

//                 // remove the scheduables that have been dispatched
//                 foreach(var index in _indicesToRemove) {
//                     _schedulablesById[ownerId].RemoveAt(index);
//                 }
//             }    
//         }
//     }

// }