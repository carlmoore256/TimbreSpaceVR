using System;
using System.Collections.Generic;
using NWaves.Signals;
using NWaves.Audio;
using NWaves.FeatureExtractors.Multi;
using NWaves.FeatureExtractors.Options;
using NWaves.FeatureExtractors;
using NWaves.FeatureExtractors.Base;
using NWaves.Windows;
using NWaves.Filters.Base;

using System.Linq;
using UnityEngine;

public enum FeatureExtractorType {
    Spectral,
    Temporal,
    MFCC
}

public struct FeatureKey {
    public string alias;
    public AudioFeature feature;
    public FeatureExtractorType extractorType;
    public FeatureKey(string alias, FeatureExtractorType extractorType, AudioFeature feature) {
        this.alias = alias;
        this.extractorType = extractorType;
        this.feature = feature;
    }
}

public class FeatureValues {
    public float[] values;
    public float min;
    public float max;
    public FeatureKey key;
    public int Length { get { return values.Length; }}
    public FeatureValues(float[] values, FeatureKey key) {
        this.values = values;
        this.key = key;
        this.min = values.Min();
        this.max = values.Max();
    }

    public float GetNormalized(int index, float rangeLow = 0, float rangeHigh = 1) {
        return (((values[index] - min) / (max - min)) * (rangeHigh - rangeLow)) + rangeLow;
    }

    public float this[int index] {
        get { return values[index]; }
        set { values[index] = value; }
    }
}

public struct WindowTime {
    public double startTime;
    public double endTime;
    public double duration;
    public WindowTime(double startTime, double endTime) {
        this.startTime = startTime;
        this.endTime = endTime;
        this.duration = endTime - startTime;
    }
}

// instead of having a lot of little GrainAudioFeatures,
// maybe each grain should have an index, which can lookup
// an index across multiple feature vectors

public class AudioFeatureExtractor
{    
    private int windowSize;
    private int hopSize;
    private static FeatureKey[] featureKeys;

    // store whether or not each feature has been computed
    public Dictionary<AudioFeature, bool> computedFeatures;

    // using a dictionary because operations may possibly happen async,
    // and insertions might happen out of order
    // public Dictionary<int, GrainAudioFeatures> grainAudioFeatures;

    // public Dictionary<AudioFeature, float[]> FeatureValues { get; protected set; }
    public Dictionary<AudioFeature, FeatureValues> FeatureValues { get; protected set; }

    public List<WindowTime> WindowTimes { get; protected set; }

    public AudioFeatureExtractor(int windowSize, int hopSize)
    {
        this.windowSize = windowSize;
        this.hopSize = hopSize;

        // make a list of all feature keys
        featureKeys = new FeatureKey[] {
            // Spectral
            new FeatureKey("centroid", FeatureExtractorType.Spectral, AudioFeature.Centroid),
            new FeatureKey("spread", FeatureExtractorType.Spectral, AudioFeature.Spread),
            new FeatureKey("flatness", FeatureExtractorType.Spectral, AudioFeature.Flatness),
            new FeatureKey("noiseness", FeatureExtractorType.Spectral, AudioFeature.Noiseness),
            new FeatureKey("rolloff", FeatureExtractorType.Spectral, AudioFeature.Rolloff),
            new FeatureKey("crest", FeatureExtractorType.Spectral, AudioFeature.Crest),
            new FeatureKey("entropy", FeatureExtractorType.Spectral, AudioFeature.Entropy),
            new FeatureKey("decrease", FeatureExtractorType.Spectral, AudioFeature.Decrease),
            new FeatureKey("c1", FeatureExtractorType.Spectral, AudioFeature.Contrast_0),
            new FeatureKey("c2", FeatureExtractorType.Spectral, AudioFeature.Contrast_1),
            new FeatureKey("c3", FeatureExtractorType.Spectral, AudioFeature.Contrast_2),
            new FeatureKey("c4", FeatureExtractorType.Spectral, AudioFeature.Contrast_3),
            new FeatureKey("c5", FeatureExtractorType.Spectral, AudioFeature.Contrast_4),
            new FeatureKey("c6", FeatureExtractorType.Spectral, AudioFeature.Contrast_5),
            // Temporal
            new FeatureKey("energy", FeatureExtractorType.Temporal, AudioFeature.Energy),
            new FeatureKey("rms", FeatureExtractorType.Temporal, AudioFeature.RMS),
            new FeatureKey("zcr", FeatureExtractorType.Temporal, AudioFeature.ZCR),
            new FeatureKey("time_entropy", FeatureExtractorType.Temporal, AudioFeature.TimeEntropy),
            // MFCC
            new FeatureKey("mfcc0", FeatureExtractorType.MFCC, AudioFeature.MFCC_0),
            new FeatureKey("mfcc1", FeatureExtractorType.MFCC, AudioFeature.MFCC_1),
            new FeatureKey("mfcc2", FeatureExtractorType.MFCC, AudioFeature.MFCC_2),
            new FeatureKey("mfcc3", FeatureExtractorType.MFCC, AudioFeature.MFCC_3),
            new FeatureKey("mfcc4", FeatureExtractorType.MFCC, AudioFeature.MFCC_4),
            new FeatureKey("mfcc5", FeatureExtractorType.MFCC, AudioFeature.MFCC_5),
            new FeatureKey("mfcc6", FeatureExtractorType.MFCC, AudioFeature.MFCC_6),
        };

        // iterate over feature keys to fill computed features entries with null arrays
        computedFeatures = new Dictionary<AudioFeature, bool>();
        foreach (FeatureKey key in featureKeys) {
            computedFeatures.Add(key.feature, false);
        }

        FeatureValues = new Dictionary<AudioFeature, FeatureValues>();
        foreach (FeatureKey key in featureKeys) {
            FeatureValues.Add(key.feature, null);
        }

    }

    private string FeatureKeysToNWavesOptions(List<FeatureKey> featureKeys) {
        var options = featureKeys.Select(k => k.alias);
        return string.Join(",", options);
    }


    // ultimately, WE ARE ONLY CHANGING THE FEATURE VALUES
    // that is why I decided to make the featureValues into a dict,
    // and prioritize those values when saving them as an enumerable

    ///<summary>
    /// Compute all multi-feature types (MFCC, Spectral, Temporal)
    ///</summary>
    private void ComputeFeatures(DiscreteSignal signal, List<FeatureKey> keys, FeatureExtractorType type) {
        // find keys of specified type
        keys = keys.FindAll(k => k.extractorType == type);
        if (keys.Count == 0)
            return;        
        // mfcc is a special type and is handled separately
        if (type == FeatureExtractorType.MFCC) { 
            ComputeMfccFeatures(signal, keys);
            return;
        }
        // remove any existing computed features
        List<FeatureKey> keysToRemove = new List<FeatureKey>();
        foreach(FeatureKey key in keys) {
            if (computedFeatures[key.feature])
                keysToRemove.Add(key);
        }
        foreach(FeatureKey key in keysToRemove)
            keys.Remove(key);
        if (keys.Count == 0)
            return;
        Debug.Log($"Processing Feature List: {FeatureKeysToNWavesOptions(keys)} | {type}");
        var multiOpts = new MultiFeatureOptions {
            SamplingRate = signal.SamplingRate,
            FrameDuration = (double)windowSize / (double)signal.SamplingRate,
            HopDuration = (double)hopSize / (double)signal.SamplingRate,
            FftSize = windowSize,
            FeatureList = FeatureKeysToNWavesOptions(keys)
        };
        FeatureExtractor extractor;
        switch (type) {
            case FeatureExtractorType.Spectral:
                extractor = new SpectralFeaturesExtractor(multiOpts);
                break;
            case FeatureExtractorType.Temporal:
                extractor = new TimeDomainFeaturesExtractor(multiOpts);
                break;
            default:
                throw new Exception("Invalid feature extractor type");
        }
        List<float[]> vectors = extractor.ParallelComputeFrom(signal);
        FeaturePostProcessing.NormalizeMean(vectors);

        if (WindowTimes == null) {
            WindowTimes = new List<WindowTime>();
            extractor.TimeMarkers(vectors.Count).ForEach((double start) => {
                WindowTimes.Add(new WindowTime(start, start + multiOpts.FrameDuration));
            });
        }

        var names = extractor.FeatureDescriptions;
        for(int featureIdx = 0; featureIdx < names.Count; featureIdx++) {
            var fk = Array.Find(featureKeys, k => k.alias == names[featureIdx]);
            // transpose values
            float[] _featureValues = new float[vectors.Count];
            for (int j = 0; j < vectors.Count; j++)
                _featureValues[j] = vectors[j][featureIdx];
    
            // float _min = _featureValues.Min();
            // float _max = _featureValues.Max();
            // // normalize FeatureValues to -1 to 1
            // for(int i = 0; i < _featureValues.Length; i++)
            //     _featureValues[i] = ((_featureValues[i] - _min) / (_max - _min)) * 2 - 1;

            FeatureValues[fk.feature] = new FeatureValues(_featureValues, fk);
        }
    }

    private void ComputeMfccFeatures(DiscreteSignal signal, List<FeatureKey> keys) {
        // make sure feature isn't already computed
        List<FeatureKey> keysToRemove = new List<FeatureKey>();
        foreach(FeatureKey key in keys) {
            if (computedFeatures[key.feature])
                keysToRemove.Add(key);
        }
        foreach(FeatureKey key in keysToRemove)
            keys.Remove(key);
        // only run mfcc extraction once per instance of AudioFeatures
        var mfccOpts = new MfccOptions {
            SamplingRate = signal.SamplingRate,
            FrameDuration = (double)windowSize / (double)signal.SamplingRate,
            HopDuration = (double)hopSize / (double)signal.SamplingRate,
            FftSize = windowSize,
            FilterBankSize = 26,
            FeatureCount = 8
        };
        var mfccExtractor = new MfccExtractor(mfccOpts);
        var vectors = mfccExtractor.ParallelComputeFrom(signal);
        FeaturePostProcessing.NormalizeMean(vectors);
        var names = mfccExtractor.FeatureDescriptions;


        // FeatureKey[] _featureKeys = new FeatureKey[names.Count]; // We have to do this to ensure order is correct
        
        if (WindowTimes == null) { // REMOVE THIS!
            WindowTimes = new List<WindowTime>();
            mfccExtractor.TimeMarkers(vectors.Count).ForEach((double start) => {
                WindowTimes.Add(new WindowTime(start, start + mfccOpts.FrameDuration));
            });
        }

        for(int featureIdx = 0; featureIdx < names.Count; featureIdx++) {
    
            var fk = Array.Find(featureKeys, k => k.alias == names[featureIdx]);

            // transpose values
            float[] _featureValues = new float[vectors.Count];
            for (int j = 0; j < vectors.Count; j++)
                _featureValues[j] = vectors[j][featureIdx];

            // float _min = _featureValues.Min();
            // float _max = _featureValues.Max();
            // // normalize FeatureValues to -1 to 1
            // for(int i = 0; i < _featureValues.Length; i++)
            //     _featureValues[i] = ((_featureValues[i] - _min) / (_max - _min)) * 2 - 1;
            FeatureValues[fk.feature] = new FeatureValues(_featureValues, fk);;
        }
    }


    public void ExtractFeatures(DiscreteSignal signal, AudioFeature[] features, Action<GrainAudioFeatures> callback) {
        // get FeatureKeys for requested features
        List<FeatureKey> requestedFeatures = new List<FeatureKey>();

        foreach (AudioFeature feature in features) {
            FeatureKey featureKey = Array.Find(featureKeys, key => key.feature == feature);
            requestedFeatures.Add(featureKey);
        }

        // ==== run extractors, put results into computedFeatures dict ===========
        ComputeFeatures(signal, requestedFeatures, FeatureExtractorType.Spectral);
        ComputeFeatures(signal, requestedFeatures, FeatureExtractorType.Temporal);
        ComputeFeatures(signal, requestedFeatures, FeatureExtractorType.MFCC);

        // ==== run callback once processing has ended ===========================
        for(int i = 0; i < FeatureValues[features[0]].Length; i++) {
            GrainAudioFeatures gf = new GrainAudioFeatures(this, i);
            callback.Invoke(gf);
        }
    }
}
























// private void ComputeSpectralFeatures(DiscreteSignal signal, List<FeatureKey> keys) {
//     // make sure feature isn't already computed
//     foreach(FeatureKey key in keys) {
//         if (computedFeatures[key.feature].values != null)
//             keys.Remove(key);
//     }
//     if (keys.Count == 0)
//         return;
//     var multiOpts = new MultiFeatureOptions {
//         SamplingRate = signal.SamplingRate,
//         FrameDuration = (double)windowSize / (double)signal.SamplingRate,
//         HopDuration = (double)hopSize / (double)signal.SamplingRate,
//         FftSize = windowSize
//     };
//     multiOpts.FeatureList = FeatureKeysToNWavesOptions(keys);
//     var specExtractor = new SpectralFeaturesExtractor(multiOpts);
//     var vectors = specExtractor.ParallelComputeFrom(signal);
//     FeaturePostProcessing.NormalizeMean(vectors);
//     var names = specExtractor.FeatureDescriptions;
//     FeatureKey[] _featureKeys = new FeatureKey[names.Count];
//     for(int i = 0; i < names.Count; i++) {
//         _featureKeys[i] = Array.Find(featureKeys, k => k.alias == names[i]);
//     }

//     for (int i = 0; i < vectors.Count; i++) {
//         GrainAudioFeatures gf = null;
//         grainAudioFeatures.TryGetValue(i, out gf);
//         if (gf == null) {
//             gf = new GrainAudioFeatures(vectors[i], _featureKeys);
//             grainAudioFeatures.Add(i, gf);
//         }
//         // computedFeatures[key.feature] = new FeatureValues(vectors[i], key);
//     }
// }

// private void ComputeTemporalFeatures(DiscreteSignal signal, List<FeatureKey> keys) {
//     foreach(FeatureKey key in keys) {
//         if (computedFeatures[key.feature].values != null)
//             keys.Remove(key);
//     }
//     if (keys.Count == 0)
//         return;
//     var multiOpts = new MultiFeatureOptions {
//         SamplingRate = signal.SamplingRate,
//         FrameDuration = (double)windowSize / (double)signal.SamplingRate,
//         HopDuration = (double)hopSize / (double)signal.SamplingRate,
//         FftSize = windowSize
//     };
//     multiOpts.FeatureList = FeatureKeysToNWavesOptions(keys);
//     var timeExtractor = new TimeDomainFeaturesExtractor(multiOpts);
//     var vectors = timeExtractor.ParallelComputeFrom(signal);
//     FeaturePostProcessing.NormalizeMean(vectors);
//     var names = timeExtractor.FeatureDescriptions;
//     for(int i = 0; i < names.Count; i++) {
//         var key = Array.Find(featureKeys, k => k.alias == names[i]);
//         computedFeatures[key.feature] = new FeatureValues(vectors[i], key);
//     }
// }

// var spectralKeys = requestedFeatures.FindAll(key => key.extractorType == FeatureExtractorType.Spectral);
// var temporalKeys = requestedFeatures.FindAll(key => key.extractorType == FeatureExtractorType.Temporal);
// var mfccKeys = requestedFeatures.FindAll(key => key.extractorType == FeatureExtractorType.MFCC);

// // ==== run extractors ==================================================

// if (spectralKeys.Count > 0) {
// ComputeSpectralFeatures(signal, spectralKeys);
// }

// if (temporalKeys.Count > 0) {
// ComputeTemporalFeatures(signal, temporalKeys);
// }

// if (mfccKeys.Count > 0) {
// ComputeMfccFeatures(signal, mfccKeys);
// }

// create extractors
// var spectralKeys = requestedFeatures.FindAll(key => key.extractorType == FeatureExtractorType.Spectral);
// var temporalKeys = requestedFeatures.FindAll(key => key.extractorType == FeatureExtractorType.Temporal);
// var mfccKeys = requestedFeatures.FindAll(key => key.extractorType == FeatureExtractorType.MFCC);

// // List<FeatureExtractor> extractors = new List<FeatureExtractor>();
// var multiOpts = new MultiFeatureOptions {
//     SamplingRate = signal.SamplingRate,
//     FrameDuration = (double)windowSize / (double)signal.SamplingRate,
//     HopDuration = (double)hopSize / (double)signal.SamplingRate,
//     FftSize = windowSize
// };

// // ==== run extractors ==================================================

// if (spectralKeys.Count > 0) {
//     // make sure feature isn't already computed
//     foreach(FeatureKey key in spectralKeys) {
//         if (computedFeatures[key.feature].values != null)
//             spectralKeys.Remove(key);
//     }

//     multiOpts.FeatureList = FeatureKeysToNWavesOptions(spectralKeys);
//     var specExtractor = new SpectralFeaturesExtractor(multiOpts);
//     var vectors = specExtractor.ParallelComputeFrom(signal);
//     FeaturePostProcessing.NormalizeMean(vectors);
//     var names = specExtractor.FeatureDescriptions;
//     for(int i = 0; i < names.Count; i++) {
//         var key = Array.Find(featureKeys, k => k.alias == names[i]);
//         computedFeatures[key.feature] = new FeatureValues(vectors[i], key);
//     }
// }

// if (temporalKeys.Count > 0) {
//     // make sure feature isn't already computed
//     foreach(FeatureKey key in temporalKeys) {
//         if (computedFeatures[key.feature].values != null)
//             temporalKeys.Remove(key);
//     }
//     multiOpts.FeatureList = FeatureKeysToNWavesOptions(temporalKeys);
//     var timeExtractor = new TimeDomainFeaturesExtractor(multiOpts);
//     var vectors = timeExtractor.ParallelComputeFrom(signal);
//     FeaturePostProcessing.NormalizeMean(vectors);
//     var names = timeExtractor.FeatureDescriptions;
//     for(int i = 0; i < names.Count; i++) {
//         var key = Array.Find(featureKeys, k => k.alias == names[i]);
//         computedFeatures[key.feature] = new FeatureValues(vectors[i], key);
//     }
// }

// if (mfccKeys.Count > 0) {
//     // make sure feature isn't already computed
//     bool hasComputed = false;
//     foreach(FeatureKey key in mfccKeys) {
//         if (computedFeatures[key.feature].values != null)
//             hasComputed = true;
//     }
//     if (!hasComputed) { // only run mfcc extraction once per instance of AudioFeatures
//         var mfccOpts = new MfccOptions {
//             SamplingRate = signal.SamplingRate,
//             FrameDuration = (double)windowSize / (double)signal.SamplingRate,
//             HopDuration = (double)hopSize / (double)signal.SamplingRate,
//             FftSize = windowSize,
//             FilterBankSize = 26,
//         };
//         var mfccExtractor = new MfccExtractor(mfccOpts);
//     }
// }


// featureOptions = featureOptions.Substring(0, featureOptions.Length - 1);        

// private MultiFeatureOptions optionsMultiFeature;
// private MfccOptions optionsMFCC;

// private static Dictionary<AudioFeature, FeatureKey> featureKeys;
// make a dictionary linking AudioFeature to FeatureKey
// featureKeys = new Dictionary<AudioFeature, FeatureKey>() {
//     // Spectral
//     { AudioFeature.Centroid, new FeatureKey("centroid", FeatureExtractorType.Spectral, AudioFeature.Centroid) },
//     { AudioFeature.Spread, new FeatureKey("spread", FeatureExtractorType.Spectral, AudioFeature.Spread) },
//     { AudioFeature.Flatness, new FeatureKey("flatness", FeatureExtractorType.Spectral, AudioFeature.Flatness) },
//     { AudioFeature.Noiseness, new FeatureKey("noiseness", FeatureExtractorType.Spectral, AudioFeature.Noiseness) },
//     { AudioFeature.Rolloff, new FeatureKey("rolloff", FeatureExtractorType.Spectral, AudioFeature.Rolloff) },
//     { AudioFeature.Crest, new FeatureKey("crest", FeatureExtractorType.Spectral, AudioFeature.Crest) },
//     { AudioFeature.Entropy, new FeatureKey("entropy", FeatureExtractorType.Spectral, AudioFeature.Entropy) },
//     { AudioFeature.Decrease, new FeatureKey("decrease", FeatureExtractorType.Spectral, AudioFeature.Decrease) },
//     { AudioFeature.Contrast_0, new FeatureKey("c1", FeatureExtractorType.Spectral, AudioFeature.Contrast_0) },
//     { AudioFeature.Contrast_1, new FeatureKey("c2", FeatureExtractorType.Spectral, AudioFeature.Contrast_1) },
//     { AudioFeature.Contrast_2, new FeatureKey("c3", FeatureExtractorType.Spectral, AudioFeature.Contrast_2) },
//     { AudioFeature.Contrast_3, new FeatureKey("c4", FeatureExtractorType.Spectral, AudioFeature.Contrast_3) },
//     { AudioFeature.Contrast_4, new FeatureKey("c5", FeatureExtractorType.Spectral, AudioFeature.Contrast_4) },
//     { AudioFeature.Contrast_5, new FeatureKey("c6", FeatureExtractorType.Spectral, AudioFeature.Contrast_5) },
//     // Temporal
//     { AudioFeature.Energy, new FeatureKey("energy", FeatureExtractorType.Temporal, AudioFeature.Energy) },
//     { AudioFeature.RMS, new FeatureKey("rms", FeatureExtractorType.Temporal, AudioFeature.RMS) },
//     { AudioFeature.ZCR, new FeatureKey("zcr", FeatureExtractorType.Temporal, AudioFeature.ZCR) },
//     { AudioFeature.TimeEntropy, new FeatureKey("time_entropy", FeatureExtractorType.Temporal, AudioFeature.TimeEntropy)},
//     // MFCC
//     { AudioFeature.MFCC_0, new FeatureKey("mfcc1", FeatureExtractorType.MFCC, AudioFeature.MFCC_0) },
//     { AudioFeature.MFCC_1, new FeatureKey("mfcc2", FeatureExtractorType.MFCC, AudioFeature.MFCC_1) },
//     { AudioFeature.MFCC_2, new FeatureKey("mfcc3", FeatureExtractorType.MFCC, AudioFeature.MFCC_2) },
//     { AudioFeature.MFCC_3, new FeatureKey("mfcc4", FeatureExtractorType.MFCC, AudioFeature.MFCC_3) },
//     { AudioFeature.MFCC_4, new FeatureKey("mfcc5", FeatureExtractorType.MFCC, AudioFeature.MFCC_4) },
//     { AudioFeature.MFCC_5, new FeatureKey("mfcc6", FeatureExtractorType.MFCC, AudioFeature.MFCC_5) },
//     { AudioFeature.MFCC_6, new FeatureKey("mfcc7", FeatureExtractorType.MFCC, AudioFeature.MFCC_6) },
//     { AudioFeature.MFCC_7, new FeatureKey("mfcc8", FeatureExtractorType.MFCC, AudioFeature.MFCC_7) },
// };