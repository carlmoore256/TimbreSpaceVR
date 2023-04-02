using UnityEngine;
using System;

[Serializable]
public class SequenceItem {
    public ISequenceable sequenceable;
    public float gain;
    public bool isMuted;
    public double startTime;
    public double endTime;

    public void SetSequenceTime(double startTime, double endTime) {
        this.startTime = startTime;
        this.endTime = endTime;
    }
}