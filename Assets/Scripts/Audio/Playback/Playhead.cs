using System.Threading;
using System;
using System.Collections.Generic;
using UnityEngine;
using NWaves.Signals;

public class Playhead {
    public int StartSample { get; set; }
    public int EndSample { get; set; }
    public int NumSamples { get; set; }
    public int Position { get; set; }
    public bool HasAdvanced { get; set; } = false;

    
    public Playhead(int startSample, int endSample) {
        StartSample = startSample;
        EndSample = endSample;
        NumSamples = endSample - startSample;
        Position = startSample;
        HasAdvanced = false;
    }

    public Playhead() {
        StartSample = 0;
        EndSample = 0;
        NumSamples = 0;
        Position = 0;
        HasAdvanced = false;
    }

    public void Set(WindowTime windowTime, DiscreteSignal signal) {
        // StartSample = (int)Mathf.Floor((float)(windowTime.startTime * signal.SamplingRate));
        // EndSample = (int)Mathf.Floor((float)(windowTime.endTime * signal.SamplingRate));
        StartSample = (int)Mathf.Floor((float)(windowTime.startTime * signal.SamplingRate));
        EndSample = (int)Mathf.Floor((float)(windowTime.endTime * signal.SamplingRate));
        EndSample = Mathf.Min(EndSample, signal.Length);
        Position = StartSample;
        NumSamples = EndSample - StartSample;
        HasAdvanced = false;

        // var sampleRange = windowTime.GetSampleRange(signal.SamplingRate);
        // StartSample = sampleRange.start;
        // NumSamples = sampleRange.count;
        // EndSample = Mathf.Min(sampleRange.end, signal.Length);
        // Position = StartSample;
        // NumSamples = EndSample - StartSample;
        // HasAdvanced = false;
    }

    public float Score() {
        float score = EndSample - Position;
        if (score == 0) return 0f;
        score = score / (float)NumSamples;
        return score;
    }

    public int WindowIndex() {
        return Position - StartSample;
    }

    public int SamplesRemaining() {
        return EndSample - Position;
    }

    public bool IsFinished() {
        return Position >= EndSample;
    }

    public override string ToString() {
        return $"PLAYHEAD: {Position} | {StartSample} -> {EndSample} | Length: {NumSamples} | Current Index: {WindowIndex()}";
    }
}