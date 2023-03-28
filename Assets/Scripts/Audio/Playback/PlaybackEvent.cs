using UnityEngine;
using System;


public class PlaybackEvent {
    public int submitterID;
    public float score;
    public float gain;
    public float rms;
    public DateTime createdAt;
    public WindowTime windowTime;
    public SequencedPlayback sequencedPlayback;
    public Action onComplete;

    public PlaybackEvent() {}

    public PlaybackEvent(WindowTime windowTime) {
        this.windowTime = windowTime;
        this.createdAt = DateTime.Now;
        // this.createdAt = AudioSettings.dspTime;
        this.score = Mathf.Infinity;
    }

    public PlaybackEvent(float gain, WindowTime windowTime, float rms=0f, int submitterID=0, SequencedPlayback sequencedPlayback=null) {
        this.gain = gain;
        this.windowTime = windowTime;
        this.rms = rms;
        this.submitterID = submitterID;
        this.sequencedPlayback = sequencedPlayback;
        this.createdAt = DateTime.Now;
        // this.createdAt = AudioSettings.dspTime;
        this.score = Mathf.Infinity; // goal is to minimize score
    }

    public void Set(float gain, WindowTime windowTime, float rms=0f, int submitterID=0, SequencedPlayback sequencedPlayback=null) {
        this.gain = gain;
        this.windowTime = windowTime;
        this.rms = rms;
        this.submitterID = submitterID;
        this.sequencedPlayback = sequencedPlayback;
        this.createdAt = DateTime.Now;
        // this.createdAt = AudioSettings.dspTime;
        this.score = Mathf.Infinity; // goal is to minimize score
    }

    public void Reset() {
        this.score = Mathf.Infinity;
        this.gain = 0f;
        this.rms = 0f;
        this.sequencedPlayback = null;
        this.createdAt = DateTime.Now;
    }
}



public class SequencedPlayback {
    public double playTime;
    public double endTime;
    public SequencedPlayback(double scheduledPlayTime, double scheduledEndTime) {
        this.playTime = scheduledPlayTime;
        this.endTime = scheduledEndTime;
    }
}