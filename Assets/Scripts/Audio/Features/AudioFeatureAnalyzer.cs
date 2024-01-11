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


/// <summary>
/// Handles audio feature analysis on a given DiscreteSignal, given WindowSize (samples) and HopSize (samples).
/// Provides APIs to retrieve a FeatureVector for a given AudioFeature, and computations are cached to to avoid recomputing.
/// </summary>
public class AudioFeatureAnalyzer
{
    public DiscreteSignal Signal { get; protected set; }
    private int windowSize;
    private int hopSize;
    public Dictionary<AudioFeature, FeatureVector> FeatureVectors { get; protected set; }
    public WindowTime[] WindowTimes { get; protected set; }

    public enum FeatureExtractorType
    {
        Spectral,
        Temporal,
        MFCC
    }

    public AudioFeatureAnalyzer(int windowSize, int hopSize)
    {
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

    private static readonly Dictionary<AudioFeature, (string alias, FeatureExtractorType extractorType)> featureExtractors =
    new Dictionary<AudioFeature, (string alias, FeatureExtractorType extractorType)>() {
        { AudioFeature.Centroid, ("centroid", FeatureExtractorType.Spectral) },
        { AudioFeature.Spread, ("spread", FeatureExtractorType.Spectral) },
        { AudioFeature.Flatness, ("flatness", FeatureExtractorType.Spectral) },
        { AudioFeature.Noiseness, ("noiseness", FeatureExtractorType.Spectral) },
        { AudioFeature.Rolloff, ("rolloff", FeatureExtractorType.Spectral) },
        { AudioFeature.Crest, ("crest", FeatureExtractorType.Spectral) },
        { AudioFeature.Entropy, ("entropy", FeatureExtractorType.Spectral) },
        { AudioFeature.Decrease, ("decrease", FeatureExtractorType.Spectral) },
        { AudioFeature.Contrast_0, ("c1", FeatureExtractorType.Spectral) },
        { AudioFeature.Contrast_1, ("c2", FeatureExtractorType.Spectral) },
        { AudioFeature.Contrast_2, ("c3", FeatureExtractorType.Spectral) },
        { AudioFeature.Contrast_3, ("c4", FeatureExtractorType.Spectral) },
        { AudioFeature.Contrast_4, ("c5", FeatureExtractorType.Spectral) },
        { AudioFeature.Contrast_5, ("c6", FeatureExtractorType.Spectral) },
        { AudioFeature.Energy, ("energy", FeatureExtractorType.Temporal) },
        { AudioFeature.RMS, ("rms", FeatureExtractorType.Temporal) },
        { AudioFeature.ZCR, ("zcr", FeatureExtractorType.Temporal) },
        { AudioFeature.MFCC_0, ("mfcc0", FeatureExtractorType.MFCC) },
        { AudioFeature.MFCC_1, ("mfcc1", FeatureExtractorType.MFCC) },
        { AudioFeature.MFCC_2, ("mfcc2", FeatureExtractorType.MFCC) },
        { AudioFeature.MFCC_3, ("mfcc3", FeatureExtractorType.MFCC) },
        { AudioFeature.MFCC_4, ("mfcc4", FeatureExtractorType.MFCC) },
        { AudioFeature.MFCC_5, ("mfcc5", FeatureExtractorType.MFCC) },
        { AudioFeature.MFCC_6, ("mfcc6", FeatureExtractorType.MFCC) },
        { AudioFeature.MFCC_7, ("mfcc7", FeatureExtractorType.MFCC) },
        { AudioFeature.MFCC_8, ("mfcc8", FeatureExtractorType.MFCC) },
        { AudioFeature.MFCC_9, ("mfcc9", FeatureExtractorType.MFCC) },
        { AudioFeature.MFCC_10, ("mfcc10", FeatureExtractorType.MFCC) },
        { AudioFeature.MFCC_11, ("mfcc11", FeatureExtractorType.MFCC) },
        { AudioFeature.MFCC_12, ("mfcc12", FeatureExtractorType.MFCC) }
    };

    private void Initialize()
    {
        FeatureVectors = new Dictionary<AudioFeature, FeatureVector>();
    }

    public FeatureVector GetFeatureVector(AudioFeature feature, bool recompute = false)
    {
        if (!recompute && FeatureVectors.ContainsKey(feature) && FeatureVectors[feature] != null)
        {
            return FeatureVectors[feature];
        }
        return ComputeFeature(feature);
    }

    private FeatureVector ComputeFeature(AudioFeature feature)
    {
        Debug.Log("Computing feature: " + feature);
        FeatureExtractor extractor = GetFeatureExtractor(feature);
        List<float[]> vectors = extractor.ParallelComputeFrom(Signal);
        FeaturePostProcessing.NormalizeMean(vectors);
        var names = extractor.FeatureDescriptions;
        float[] _featureVector = new float[vectors.Count];
        for (int i = 0; i < vectors.Count; i++)
        {
            _featureVector[i] = float.IsNaN(vectors[i][0]) ? 0 : vectors[i][0];
        }
        FeatureVector featureVector = new FeatureVector(_featureVector, feature);
        FeatureVectors[feature] = featureVector;
        return featureVector;
    }

    private FeatureExtractor GetFeatureExtractor(AudioFeature feature)
    {
        // Debug.Log("Getting feature extractor for: " + featureKey.alias + " " + featureKey.feature);
        double frameDuration = (double)windowSize / (double)Signal.SamplingRate;
        double hopDuration = (double)hopSize / (double)Signal.SamplingRate;

        var featureNWavesAlias = featureExtractors[feature].alias;
        var extractorType = featureExtractors[feature].extractorType;
        switch (extractorType)
        {
            case FeatureExtractorType.MFCC:
                return new MfccExtractor(new MfccOptions
                {
                    SamplingRate = Signal.SamplingRate,
                    FrameDuration = frameDuration,
                    HopDuration = hopDuration,
                    FftSize = windowSize,
                    FilterBankSize = 26,
                    FeatureCount = 8
                });

            case FeatureExtractorType.Spectral:
                return new SpectralFeaturesExtractor(new MultiFeatureOptions
                {
                    SamplingRate = Signal.SamplingRate,
                    FrameDuration = frameDuration,
                    HopDuration = hopDuration,
                    FftSize = windowSize,
                    FeatureList = featureNWavesAlias // <= THIS MAY CAUSE AN ISSUE...
                });

            case FeatureExtractorType.Temporal:
                return new TimeDomainFeaturesExtractor(new MultiFeatureOptions
                {
                    SamplingRate = Signal.SamplingRate,
                    FrameDuration = frameDuration,
                    HopDuration = hopDuration,
                    FftSize = windowSize,
                    FeatureList = featureNWavesAlias // <= THIS MAY CAUSE AN ISSUE...
                });

            default:
                throw new Exception("Invalid feature extractor type");
        }
    }

    private string AudioFeaturesToNWaves(List<AudioFeature> audioFeatures)
    {
        var featureKeys = audioFeatures.Select(f => featureExtractors[f]);
        IEnumerable<string> options = featureKeys.Select(k => k.alias);
        return string.Join(",", options);
    }


    public void BatchComputeFeatures(IEnumerable<AudioFeature> features, Action onComplete = null, bool recompute = false)
    {
        var spectralFeatures = new List<AudioFeature>();
        var temporalFeatures = new List<AudioFeature>();
        var mfccFeatures = new List<AudioFeature>();

        // Organize features by extractor type
        foreach (var feature in features)
        {
            if (!recompute && FeatureVectors.ContainsKey(feature))
                continue;

            var extractorType = featureExtractors[feature].extractorType;
            switch (extractorType)
            {
                case FeatureExtractorType.Spectral:
                    spectralFeatures.Add(feature);
                    break;
                case FeatureExtractorType.Temporal:
                    temporalFeatures.Add(feature);
                    break;
                case FeatureExtractorType.MFCC:
                    mfccFeatures.Add(feature);
                    break;
            }
        }

        // Compute features by extractor type
        if (spectralFeatures.Count > 0)
            ComputeFeaturesByType(spectralFeatures, FeatureExtractorType.Spectral);
        if (temporalFeatures.Count > 0)
            ComputeFeaturesByType(temporalFeatures, FeatureExtractorType.Temporal);
        if (mfccFeatures.Count > 0)
            ComputeMfccFeatures(mfccFeatures);

        onComplete?.Invoke();
    }

    private void ComputeFeaturesByType(List<AudioFeature> features, FeatureExtractorType type)
    {
        Debug.Log($"Processing Feature List: {AudioFeaturesToNWaves(features)} | {type}");
        var multiOpts = new MultiFeatureOptions
        {
            SamplingRate = Signal.SamplingRate,
            FrameDuration = (double)windowSize / (double)Signal.SamplingRate,
            HopDuration = (double)hopSize / (double)Signal.SamplingRate,
            FftSize = windowSize,
            FeatureList = AudioFeaturesToNWaves(features)
        };

        FeatureExtractor extractor;
        switch (type)
        {
            case FeatureExtractorType.Spectral:
                extractor = new SpectralFeaturesExtractor(multiOpts);
                break;
            case FeatureExtractorType.Temporal:
                extractor = new TimeDomainFeaturesExtractor(multiOpts);
                break;
            default:
                throw new Exception("Invalid feature extractor type");
        }

        ProcessFeatureExtractor(extractor, features);
    }


    private void ComputeMfccFeatures(List<AudioFeature> features)
    {
        Debug.Log($"Processing Feature List: {AudioFeaturesToNWaves(features)} | FeatureExtractorType.MFCC");
        var mfccOpts = new MfccOptions
        {
            SamplingRate = Signal.SamplingRate,
            FrameDuration = (double)windowSize / (double)Signal.SamplingRate,
            HopDuration = (double)hopSize / (double)Signal.SamplingRate,
            FftSize = windowSize,
            FilterBankSize = 26,
            FeatureCount = 8
        };

        var mfccExtractor = new MfccExtractor(mfccOpts);
        ProcessFeatureExtractor(mfccExtractor, features);
    }

    private void ProcessFeatureExtractor(FeatureExtractor extractor, List<AudioFeature> features)
    {
        List<float[]> vectors = extractor.ParallelComputeFrom(Signal);
        FeaturePostProcessing.NormalizeMean(vectors);
        // if (WindowTimes == null) CalculateWindowTimes();
        var names = extractor.FeatureDescriptions;    
        var namesJoined = string.Join(",", names);
        Debug.Log("Running feature extraction on " + names.Count + " " + namesJoined + " features");
        for (int featureIdx = 0; featureIdx < names.Count - 1; featureIdx++)
        {
            // sometimes we get more than we asked for (with MFCCs) so we need to query it this way
            var kvp = featureExtractors.FirstOrDefault(kv => kv.Value.alias == names[featureIdx]);
            AudioFeature feature = kvp.Key;
            float[] _featureValues = new float[vectors.Count];
            for (int j = 0; j < vectors.Count; j++) { // transpose values
                // change NaN values to 0
                if (float.IsNaN(vectors[j][featureIdx])) {
                    vectors[j][featureIdx] = 0;
                }
                _featureValues[j] = vectors[j][featureIdx];
            }
            FeatureVectors[feature] = new FeatureVector(_featureValues, feature);
        }
    }

    private void CalculateWindowTimes()
    {
        var _windowTimes = new List<WindowTime>();
        int nFrames = (Signal.Length - windowSize) / hopSize + 1;
        for (int i = 0; i < nFrames; i++)
        {
            int startIdx = i * hopSize;
            int endIdx = startIdx + windowSize;
            double start = (double)startIdx / Signal.SamplingRate;
            double end = (double)endIdx / Signal.SamplingRate;
            _windowTimes.Add(new WindowTime(start, end, windowSize));
        }
        WindowTimes = _windowTimes.ToArray();
    }
}