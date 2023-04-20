using UnityEngine;
using System;

[Serializable]
public class SequenceItem {
    public ISequenceable sequenceable;
    // public Action onPlayStart;
    // public Action onPlayEnd;
    public float gain;
    public bool isMuted;
    public double scheduleTime;

    public void SetScheduleTime(double scheduleTime) {
        this.scheduleTime = scheduleTime;
    }

    // public double endTime;
    // public void SetSequenceTime(double startTime, double endTime) {
    //     this.scheduleTime = startTime;
    //     this.endTime = endTime;
    // }
}