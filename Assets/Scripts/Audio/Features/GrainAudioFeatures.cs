using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Wrapper class for a grain's audio features
/// </summary>
public class GrainAudioFeatures {
    private AudioFeatureAnalyzer featureExtractor;
    public WindowTime WindowTime { get; protected set; }
    public int GrainIndex { get; protected set; }

    public GrainAudioFeatures(AudioFeatureAnalyzer featureExtractor, int grainIndex) {
        this.featureExtractor = featureExtractor;
        WindowTime = featureExtractor.WindowTimes[grainIndex];
        GrainIndex = grainIndex;
    }

    public float Get(AudioFeature feature, bool normalize = true, bool positive = false) {
        if (featureExtractor.FeatureVectors.ContainsKey(feature)) {
            if (normalize) {
                if (positive) {
                    return (featureExtractor.FeatureVectors[feature].GetNormalized(GrainIndex, 0f, 1f));
                } else {
                    return (featureExtractor.FeatureVectors[feature].GetNormalized(GrainIndex, -1f, 1f));
                }
            } else {
                if (positive) {
                    float min = featureExtractor.FeatureVectors[feature].min;
                    return featureExtractor.FeatureVectors[feature][GrainIndex] + min;
                } else {
                    return featureExtractor.FeatureVectors[feature][GrainIndex];
                }
            } 
        } else {
            return 0;
        }
    }
}





























// grain features needs to be changed - it shouldnt contain audio samples,
// only start and end positions, and the features
// or even a reference to the audio features float[]

public class GrainFeatures
{
    public float[] AudioSamples { get; protected set; }
    public int SampleRate { get; protected set; }
    public float[] mfccs;
    Dictionary<AudioFeature, float> features;

    public GrainFeatures(
        float[] audioSamples,
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
        this.AudioSamples = audioSamples;
        this.SampleRate = sampleRate;
        
        features = new Dictionary<AudioFeature, float> {
            { AudioFeature.Centroid, centroid },
            { AudioFeature.Spread, spread},
            { AudioFeature.Flatness, flatness },
            { AudioFeature.Noiseness, noiseness},
            { AudioFeature.Rolloff, rolloff},
            { AudioFeature.Crest, crest},
            { AudioFeature.Entropy, entropy},
            { AudioFeature.Decrease, decrease},
            { AudioFeature.Energy, energy},
            { AudioFeature.RMS, rms},
            { AudioFeature.ZCR, zcr},
            // { AudioFeature.TimeEntropy, timeEntropy},
            { AudioFeature.MFCC_0, mfccs[0]},
            { AudioFeature.MFCC_1, mfccs[1]},
            { AudioFeature.MFCC_2, mfccs[2]},
            { AudioFeature.MFCC_3, mfccs[3]},
            { AudioFeature.MFCC_4, mfccs[4]},
            { AudioFeature.MFCC_5, mfccs[5]},
            { AudioFeature.MFCC_6, mfccs[6]},
            { AudioFeature.MFCC_7, mfccs[7]},
            { AudioFeature.Contrast_0, contrast[0]},
            { AudioFeature.Contrast_1, contrast[1]},
            { AudioFeature.Contrast_2, contrast[2]},
            { AudioFeature.Contrast_3, contrast[3]},
            // { AudioFeature.GrainIndex, grainIndex}
        };
    }

    public float Get(AudioFeature feature) {
        return features[feature];
    }
}

























// public struct FeatureKeyVectorPair
// {
//     public FeatureKey[] Keys;
//     public float[] Vectors;
// }

// this can be a wrapper around the vectors returned by the feature extractor
// public class GrainAudioFeatures
// {
//     public List<float[]> Vectors { get; protected set; }

//     // pretty wonky, but allows for audioFeatureExtractor to manage the memory, so this avoids copying
//     public List<FeatureKeyVectorPair> FeatureKeyVectorPairs { get; protected set; }
//     public double StartTime { get; protected set; }
//     public double EndTime { get; protected set; }

//     // add start and end points
//     public GrainAudioFeatures(float[] vectors, FeatureKey[] featureKeys, double startTime, double endTime) {
//         FeatureKeyVectorPairs = new List<FeatureKeyVectorPair>();
//         FeatureKeyVectorPairs.Add(new FeatureKeyVectorPair { Keys = featureKeys, Vectors = vectors });
//         StartTime = startTime;
//         EndTime = endTime;
//     }

//     public void Add(float[] vectors, FeatureKey[] featureKeys) {
//         FeatureKeyVectorPairs.Add(new FeatureKeyVectorPair { Keys = featureKeys, Vectors = vectors });
//     }

//     public float Get(AudioFeature feature) {
//         foreach (var pair in FeatureKeyVectorPairs) {
//             for (int i = 0; i < pair.Keys.Length; i++) {
//                 if (pair.Keys[i].feature == feature) {
//                     return pair.Vectors[i];
//                 }
//             }
//         }
//         return 0;
//     }
// }


// this.mfccs = mfccs;
// this.contrast = contrast;
// this.centroid = centroid;
// this.spread = spread;
// this.flatness = flatness;
// this.noiseness = noiseness;
// this.rolloff = rolloff;
// this.crest = crest;
// this.entropy = entropy;
// this.decrease = decrease;
// this.energy = energy;
// this.rms = rms;
// this.zcr = zcr;
// this.timeEntropy = timeEntropy;
// this.grainIndex = grainIndex;
// for (int i = 0; i < mfccs.Length; i++)
//     featureDict.Add($"mfcc_{i}", mfccs[i]);

// for (int i = 0; i < contrast.Length; i++)
//     featureDict.Add($"contrast_{i}", contrast[i]);
    // public float[] contrast;
    // public float centroid;
    // public float spread;
    // public float flatness;
    // public float noiseness;
    // public float rolloff;
    // public float crest;
    // public float entropy;
    // public float decrease;

    // time-domain features
    // public float energy;
    // public float rms;
    // public float zcr;
    // public float timeEntropy;

    // data features
    // public float grainIndex;