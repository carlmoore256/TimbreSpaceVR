using UnityEngine;
using System;


[Serializable]
public class SequenceItem {
    public ISequenceable Sequenceable { get; set; }

    public SequenceableParameters Parameters { get; set; } = new SequenceableParameters();

    public double RelativePlayTime { get; set; } = -1d; // sequenceEditor/scheduler will compute these

    public BeatIndex BeatIndex { get; set; } // if null, rescheduling bpm will ignore this sequenceable
}