using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NWaves.Signals;
using System;
using System.Threading;
using NWaves.Utils;
using System.Linq;

// basic idea is that a granularBuffer works with a collection
// of objects that represent the windows within the granular buffer
// and the analysis data for those windows,
// providing APIs to access the data

// when stored in user data, the sequence is stored as a list of ints
// which represent their index in the buffer

/// <summary>
/// Analysis for a provided DiscreteSignal audio buffer, handling an AudioFeatureAnalyzer
/// and providing APIs to access analysis data
/// </summary>
public class GranularBuffer
{    
    public int NumWindows { get { return Windows.Length; } }
    
    public WindowTime[] Windows
    {
        get
        {
            if (_featureAnalyzer == null)
            {
                Debug.LogError("Feature analyzer has not been set yet, run ResetAnalysis() before accessing WindowTimes");
                return null;
            }
            return _featureAnalyzer.WindowTimes;
        }
    }

    public WindowTime this[int index]
    {
        get
        {
            return GetWindow(index);
        }
    }

    private AudioFeatureAnalyzer _featureAnalyzer;
    private DiscreteSignal _audioBuffer;
    private Thread _analyzerThread;

    public GranularBuffer(DiscreteSignal audioBuffer)
    {
        this._audioBuffer = audioBuffer;
    }

    

    /// <summary>
    /// Set/reset feature analyzer, which will occur whenever there is a new window or hop size
    /// </summary>
    public void ResetAnalysis(int windowSize, int hopSize, AudioFeature[] features, Action onComplete=null)
    {
        _featureAnalyzer = new AudioFeatureAnalyzer(windowSize, hopSize, _audioBuffer);
        RunBatchAnalysis(features, onComplete);
    }

    /// <summary>
    /// Batch computes the given features, and invokes onComplete when finished.
    /// Cached features will not be recomputed.
    /// </summary>
    public void RunBatchAnalysis(AudioFeature[] features, Action onComplete=null)
    {
        if (_featureAnalyzer == null)
        {
            Debug.LogError("Feature analyzer has not been set yet, run ResetAnalysis() before running batch analysis");
            return;
        }
        _analyzerThread = new Thread(() =>
        {
            Debug.Log("Running batch analysis on " + features.Length + " features");
            _featureAnalyzer.BatchComputeFeatures(
                features, () => Dispatcher.RunOnMainThread(onComplete)
            );
        });
        _analyzerThread.Start();
    }

    /// <summary>
    /// Returns the audio samples as a DiscreteSignal at the given grain index
    /// </summary>
    public DiscreteSignal GetWindowSignal(int grainIndex)
    {
        if (grainIndex < 0 || grainIndex >= NumWindows)
        {
            throw new ArgumentException(string.Format(ErrorInvalidGrainIndex, NumWindows));
        }
        var windowTime = Windows[grainIndex];
        var (startSample, endSample, _) = windowTime.GetSampleRange(_audioBuffer.SamplingRate);
        return _audioBuffer[startSample, endSample];
    }

    /// <summary>
    /// Returns the audio samples as a float[] at the given grain index
    /// </summary>
    public float[] GetWindowSamples(int grainIndex)
    {
        return GetWindowSignal(grainIndex).Samples;
    }

    /// <summary>
    /// Returns the WindowTime for a given grain ID
    /// </summary>
    public WindowTime GetWindow(int grainIndex)
    {
        if (grainIndex < 0 || grainIndex >= NumWindows)
        {
            throw new ArgumentException(string.Format(ErrorInvalidGrainIndex, NumWindows));
        }
        return Windows[grainIndex];
    }

    /// <summary>
    /// Returns all the WindowTimes for a collection of given indices
    /// </summary>
    public WindowTime[] GetWindows(IEnumerable<int> grainIndices)
    {
        return grainIndices.Select(grainIndex => GetWindow(grainIndex)).ToArray();
    }

    /// <summary>
    /// Returns all the WindowTimes between two normalized points (0-1)
    /// </summary>
    public WindowTime[] GetWindowsBetween(float normalizedStart, float normalizedEnd)
    {
        if (normalizedStart < 0 || normalizedStart > 1 || normalizedEnd < 0 || normalizedEnd > 1)
        {
            Debug.LogError(ErrorNormalizedValuesOutOfRange);
            return null;
        }
        return GetWindowsBetween(Mathf.Floor(normalizedStart * NumWindows), Mathf.Floor(normalizedEnd * NumWindows));
    }

    /// <summary>
    /// Return all window times greater than index grainStart and less than grainEnd
    /// </summary>
    public WindowTime[] GetWindowsBetween(int grainStart, int grainEnd)
    {
        if (grainEnd <= grainStart)
        {
            Debug.LogError(ErrorGrainEndLessThanOrEqualToStart);
            return null;
        }

        return Windows.Skip(grainStart).Take(grainEnd - grainStart).ToArray();
    }


    /// <summary>
    /// Returns the value of an AudioFeature computed for the current buffer, provided an index into the granular windows (grainID)
    /// </summary>
    public float GetFeatureValue(AudioFeature feature, int grainIndex, bool normalized=true, float normalizedHi=1f, float normalizedLo=-1f)
    {
        var featureVector = _featureAnalyzer.GetFeatureVector(feature);
        return normalized ? featureVector.GetNormalized(grainIndex, normalizedHi, normalizedLo) : featureVector[grainIndex];
    }

    public float GetFeatureValue(AudioFeature feature, WindowTime window, bool normalized=true, float normalizedHi=1f, float normalizedLo=-1f)
    {
        // here we can looked at all cached windowTimes, and if it hasn't yet been calculated we can do so
        // for now just return the index of the window
        return GetFeatureValue(feature, Windows.ToList().IndexOf(window), normalized, normalizedHi, normalizedLo);
    }

    /// <summary>
    /// Stops all playback and audio processing workers
    /// </summary>
    public void Stop()
    {
        _analyzerThread?.Abort();
    }



    # region Rendering

    /// <summary>
    /// Returns a copy of the current audioBuffer, cropped based on the given grain IDs window times
    /// </summary>
    public DiscreteSignal GetCroppedBuffer(int[] grainIds)
    {
        int newLength = (int)Mathf.Ceil((float)grainIds.Length * (float)Windows[0].duration * (float)_audioBuffer.SamplingRate);
        float[] newSamples = new float[0];
        foreach (int grainID in grainIds)
        {
            var windowTime = Windows[grainID];
            var sampleRange = windowTime.GetSampleRange(_audioBuffer.SamplingRate);
            newSamples = MemoryOperationExtensions.MergeWithArray(
                newSamples,
                _audioBuffer[sampleRange.start, sampleRange.end].Samples
            );
        }
        return new DiscreteSignal(_audioBuffer.SamplingRate, newSamples);
    }


    public DiscreteSignal GetCroppedBuffer(WindowTime[] windows)
    {
        return GetCroppedBuffer(windows.Select(window => Windows.ToList().IndexOf(window)).ToArray());
    }

    /// <summary>
    /// Returns a copy of the current audioBuffer cropped starting at the playhead of grainStart and continuing to grainEnd
    /// </summary>
    public DiscreteSignal GetCroppedBuffer(int grainStart, int grainEnd)
    {
        if (grainEnd <= grainStart)
        {
            Debug.LogError("grainEnd must be greater than grainStart");
            return null;
        }
        var sampleRange = Windows[grainStart].GetSampleRange(_audioBuffer.SamplingRate);
        var endSampleRange = Windows[grainEnd].GetSampleRange(_audioBuffer.SamplingRate);
        return _audioBuffer[sampleRange.start, endSampleRange.end];
    }

    /// <summary>
    /// Returns a concatenated version of the current audioBuffer and another buffer
    /// </summary>
    public DiscreteSignal ConcatenateBuffer(GranularBuffer otherAudioBuffer)
    {
        return DiscreteSignalExtensions.Concatenate(_audioBuffer, otherAudioBuffer._audioBuffer);
    }

    # endregion



    # region Window Index Subsets

    public int[] RandomGrainIndices(int subsetSize)
    {
        return RandomGrainIndices(subsetSize, 0, NumWindows);
    }

    public int[] RandomGrainIndices(int subsetSize, int minIndex, int maxIndex)
    {
        if (subsetSize > maxIndex - minIndex)
        {
            Debug.LogError("subsetSize must be less than or equal to maxIndex - minIndex");
            return null;
        }
        var subset = new int[subsetSize];
        var random = new System.Random();
        for (int i = 0; i < subsetSize; i++)
        {
            subset[i] = random.Next(minIndex, maxIndex);
        }
        return subset;
    }

    /// <summary>
    /// Sort all windows by given feature and return the indices
    /// </summary>
    public int[] SortedGrainIndices(AudioFeature feature, bool ascending=true) {
        FeatureVector featureVector = _featureAnalyzer.GetFeatureVector(feature);
        return featureVector.Argsort(ascending);
    }

    /// <summary>
    /// Sort all windows by given feature and return the indices with their values
    /// </summary>
    public Dictionary<int, float> SortedGrainIndicesWithValues(AudioFeature feature, bool ascending=true) {
        FeatureVector featureVector = _featureAnalyzer.GetFeatureVector(feature);
        int [] indices = featureVector.Argsort(ascending);
        Dictionary<int, float> sortedIndices = new Dictionary<int, float>();
        for (int i = 0; i < indices.Length; i++) {
            sortedIndices.Add(indices[i], featureVector[indices[i]]);
        }
        return sortedIndices;
    }

    # endregion


    private const string ErrorInvalidGrainIndex = "grainIndex must be between 0 and {0}";
    private const string ErrorNormalizedValuesOutOfRange = "normalizedStart and normalizedEnd must be between 0 and 1";
    private const string ErrorGrainEndLessThanOrEqualToStart = "grainEnd must be greater than grainStart";
}
