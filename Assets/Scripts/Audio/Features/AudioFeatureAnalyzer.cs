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

public class FeatureVector {

    // THIS SHOULD HAVE AN AudioFeature
    public float[] values;
    public float min;
    public float max;
    public FeatureKey key;
    public int Length { get { return values.Length; }}
    public FeatureVector(float[] values, FeatureKey key) {
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

    public (int start, int end, int count) GetSampleRange(int sampleRate) {
        var startSample = (int)(startTime * sampleRate);
        var endSample = (int)(endTime * sampleRate);
        var numSamples = endSample - startSample;
        return (startSample, endSample, numSamples);
    }
}

// instead of having a lot of little GrainAudioFeatures,
// maybe each grain should have an index, which can lookup
// an index across multiple feature vectors

public class AudioFeatureAnalyzer
{    
    public DiscreteSignal Signal { get; protected set; }
    private int windowSize;
    private int hopSize;
    private static FeatureKey[] featureKeys;
    public Dictionary<AudioFeature, FeatureVector> FeatureVectors { get; protected set; }
    public WindowTime[] WindowTimes { get; protected set; }

    public AudioFeatureAnalyzer(int windowSize, int hopSize) {
        this.windowSize = windowSize;
        this.hopSize = hopSize;
        this.Signal = null;
        Initialize();
    }

    public AudioFeatureAnalyzer(int windowSize, int hopSize, DiscreteSignal signal)
    {
        this.windowSize = windowSize;
        this.hopSize = hopSize;
        this.Signal = signal;
        CalculateWindowTimes();
        Initialize();
    }

    private void Initialize() {
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
        FeatureVectors = new Dictionary<AudioFeature, FeatureVector>();
        foreach (FeatureKey key in featureKeys) {
            FeatureVectors.Add(key.feature, null);
        }
    }
  

    public FeatureVector GetFeatureVector(AudioFeature feature, bool recompute = false) {
        if (!recompute && FeatureVectors.ContainsKey(feature)) {
            return FeatureVectors[feature];
        }
        FeatureKey featureKey = Array.Find(featureKeys, key => key.feature == feature);
        return ComputeFeature(featureKey);
    }

    private FeatureVector ComputeFeature(FeatureKey featureKey) {
        
        FeatureExtractor extractor = GetFeatureExtractor(featureKey);
        
        List<float[]> vectors = extractor.ParallelComputeFrom(Signal);
        
        FeaturePostProcessing.NormalizeMean(vectors);

        var names = extractor.FeatureDescriptions;

        float[] _featureVector = new float[vectors.Count];

        // FOR MFCCS THIS WILL NEED TO BE DIFFERENT

        for (int i = 0; i < vectors.Count; i++) {
            _featureVector[i] = vectors[i][0];
        }

        FeatureVector featureVector = new FeatureVector(_featureVector, featureKey);

        FeatureVectors[featureKey.feature] = featureVector;

        // new DiscreteSignal()

        return featureVector;
    }

    private FeatureExtractor GetFeatureExtractor(FeatureKey featureKey) {
        FeatureExtractor extractor;
        double frameDuration = (double)windowSize / (double)Signal.SamplingRate;
        double hopDuration = (double)hopSize / (double)Signal.SamplingRate;
        switch (featureKey.extractorType) {
            case FeatureExtractorType.MFCC:
                return new MfccExtractor(new MfccOptions {
                    SamplingRate = Signal.SamplingRate,
                    FrameDuration = frameDuration,
                    HopDuration = hopDuration,
                    FftSize = windowSize,
                    FilterBankSize = 26,
                    FeatureCount = 8
                });

            case FeatureExtractorType.Spectral:
                extractor = new SpectralFeaturesExtractor(new MultiFeatureOptions {
                    SamplingRate = Signal.SamplingRate,
                    FrameDuration = frameDuration,
                    HopDuration = hopDuration,
                    FftSize = windowSize,
                    FeatureList = featureKey.alias // <= THIS MAY CAUSE AN ISSUE...
                });
                break;

            case FeatureExtractorType.Temporal:
                extractor = new TimeDomainFeaturesExtractor(new MultiFeatureOptions {
                    SamplingRate = Signal.SamplingRate,
                    FrameDuration = frameDuration,
                    HopDuration = hopDuration,
                    FftSize = windowSize,
                    FeatureList = featureKey.alias // <= THIS MAY CAUSE AN ISSUE...
                });
                break;

            default:
                throw new Exception("Invalid feature extractor type");
        }
        return null;
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
        var keysToRemove = keys.FindAll(k => FeatureVectors[k.feature] != null);
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
            CalculateWindowTimes();
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

            FeatureVectors[fk.feature] = new FeatureVector(_featureValues, fk);
        }
    }

    private void ComputeMfccFeatures(DiscreteSignal signal, List<FeatureKey> keys) {
        // make sure feature isn't already computed
        var keysToRemove = keys.FindAll(k => FeatureVectors[k.feature] != null);
        foreach(FeatureKey key in keysToRemove)
            keys.Remove(key);
        if (keys.Count == 0)
            return;
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
            CalculateWindowTimes();
            // var _windowTimes = new List<WindowTime>();
            // mfccExtractor.TimeMarkers(vectors.Count).ForEach((double start) => {
            //     _windowTimes.Add(new WindowTime(start, start + mfccOpts.FrameDuration));
            // });
            // WindowTimes = _windowTimes.ToArray();
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
            FeatureVectors[fk.feature] = new FeatureVector(_featureValues, fk);;
        }
    }
      
    private void CalculateWindowTimes() {
        var _windowTimes = new List<WindowTime>();
        int nFrames = (Signal.Length - windowSize) / hopSize + 1;
        for (int i = 0; i < nFrames; i++) {
            double start = i * hopSize / Signal.SamplingRate;
            double end = start + windowSize / Signal.SamplingRate;
            _windowTimes.Add(new WindowTime(start, end));
        }
        WindowTimes = _windowTimes.ToArray();
    }

    public void BatchComputeFeatures(DiscreteSignal signal, AudioFeature[] features, Action onComplete) {
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
        onComplete?.Invoke();
    }
}
