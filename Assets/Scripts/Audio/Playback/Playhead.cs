using System.Threading;
using System;
using System.Collections.Generic;
using UnityEngine;
using NWaves.Signals;

public class Playhead {
    public int startSample;
    public int endSample;
    public int windowSamples;
    public int position;
    public bool hasAdvanced;
    // public int startPad;
    
    public Playhead(int startSample, int endSample) {
        this.startSample = startSample;
        this.endSample = endSample;
        this.windowSamples = endSample - startSample;
        this.position = startSample;
        this.hasAdvanced = false;
        // this.startPad = 0;
    }

    public Playhead() {
        this.startSample = 0;
        this.endSample = 0;
        this.windowSamples = 0;
        this.position = 0;
        this.hasAdvanced = false;
        // this.startPad = 0;
    }

    public void Set(WindowTime windowTime, DiscreteSignal signal) {
        this.startSample = (int)Mathf.Floor((float)(windowTime.startTime * signal.SamplingRate));
        this.endSample = (int)Mathf.Floor((float)(windowTime.endTime * signal.SamplingRate));
        this.endSample = Mathf.Min(endSample, signal.Length);
        this.position = startSample;
        this.windowSamples = endSample - startSample;
        this.hasAdvanced = false;
    }

    public float Score() {
        float score = endSample - position;
        if (score == 0) return 0f;
        score = score / (float)windowSamples;
        // score *= Mathf.Abs(playbackEvent.rms);
        return score;
    }

    public int WindowIndex() {
        return position - startSample;
    }

    public int SamplesRemaining() {
        return endSample - position;
    }

    public bool IsFinished() {
        return position >= endSample;
    }

    public void DebugMessage() {
        Debug.Log($"PLAYHEAD: {position} | {startSample} -> {endSample} | Length: {windowSamples} | Current Index: {WindowIndex()}");
    }
}