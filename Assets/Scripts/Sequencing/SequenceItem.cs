using UnityEngine;
using System;


[Serializable]
public class SequenceItem
{
    public ISequenceable Sequenceable { get; set; }

    public SequenceableParameters Parameters { get; set; } = new SequenceableParameters();

    public double RelativePlayTime { get; set; } = -1d; // sequenceEditor/scheduler will compute these

    public BeatIndex BeatIndex { get; set; } // if null, rescheduling bpm will ignore this sequenceable

    public ScheduledEvent ScheduledPlayEvent { get; set; }

    // this gives the ability for any other object to specifically 
    // track this sequence item, when it occurs
    public void AddListener(Action action) {
        if (ScheduledPlayEvent == null) {
            throw new System.Exception("SequenceItem.ScheduledEvent has not yet been set");
        }
        ScheduledPlayEvent.OnSchedule += () => action();
    }

    public void RemoveListener(Action action) {
        if (ScheduledPlayEvent == null) {
            throw new System.Exception("SequenceItem.ScheduledEvent has not yet been set");
        }
        ScheduledPlayEvent.OnSchedule -= () => action();
    }

    // public void Reschedule(BeatIndex beatIndex, Sequence sequence) {
    //     if (ScheduledPlayEvent == null) {
    //         throw new System.Exception("SequenceItem.ScheduledEvent has not yet been set");
    //     }
    //     ScheduledPlayEvent.Cancel();
    //     ScheduledPlayEvent = Sequenceable.Schedule(beatIndex, Parameters);
    // }
}