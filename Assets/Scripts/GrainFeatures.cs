using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrainFeatures
{
    // spectral features
    public float[] audioSamples;
    public float[] mfccs;
    public float[] contrast;
    public float centroid;
    public float spread;
    public float flatness;
    public float noiseness;
    public float rolloff;
    public float crest;
    public float entropy;
    public float decrease;

    // time-domain features
    public float energy;
    public float rms;
    public float zcr;
    public float timeEntropy;

    // data features
    public float grainIndex;
    public int sampleRate;

    public Dictionary<string, float> featureDict;

    public GrainFeatures(
        float[] _audioSamples,
        float[] mfccs,
        float[] contrast,
        float centroid,
        float spread,
        float flatness,
        float noiseness,
        float rolloff,
        float crest,
        float entropy,
        float decrease,
        float energy,
        float rms,
        float zcr,
        float timeEntropy,
        float grainIndex,
        int sampleRate)
    {
        this.audioSamples = _audioSamples;
        this.mfccs = mfccs;
        this.contrast = contrast;
        this.centroid = centroid;
        this.spread = spread;
        this.flatness = flatness;
        this.noiseness = noiseness;
        this.rolloff = rolloff;
        this.crest = crest;
        this.entropy = entropy;
        this.decrease = decrease;

        this.energy = energy;
        this.rms = rms;
        this.zcr = zcr;
        this.timeEntropy = timeEntropy;

        this.grainIndex = grainIndex;
        this.sampleRate = sampleRate;

        featureDict = new Dictionary<string, float>
        {
            { "centriod", centroid },
            { "spread", spread},
            { "flatness", flatness },
            { "noiseness", noiseness},
            { "rolloff", rolloff},
            { "crest", crest},
            { "entropy", entropy},
            { "decrease", decrease},
            { "energy", energy},
            { "rms", rms},
            { "zcr", zcr},
            { "timeEntropy", timeEntropy},
            { "grainIndex", grainIndex}
        };

        for (int i = 0; i < mfccs.Length; i++)
            featureDict.Add($"mfcc_{i}", mfccs[i]);

        for (int i = 0; i < contrast.Length; i++)
            featureDict.Add($"contrast_{i}", contrast[i]);
    }
}