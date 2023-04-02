using System.Collections;
using System.Collections.Generic;
using UnityEngine.Pool;
using UnityEngine;
using NWaves.Signals;
using System;
using System.Threading;
using NWaves.Utils;

// buffer based on audioFeatures
public class GranularBuffer {
    public AudioFeatureAnalyzer featureAnalyzer;
    public PolyvoicePlayer player;
    public DiscreteSignal audioBuffer;
    private ObjectPool<PlaybackEvent> playbackEventPool;
    public WindowTime[] WindowTimes {get {
        if (featureAnalyzer == null) {
            Debug.LogError("Feature analyzer has not been set yet, run ResetAnalysis() before accessing WindowTimes");
            return null;
        }
        return featureAnalyzer.WindowTimes;
    } }

    private Thread analyzerThread;
    
    public GranularBuffer(DiscreteSignal audioBuffer, PolyvoicePlayer player) {
        this.audioBuffer = audioBuffer;
        this.player = player;
        player.SetAudioBuffer(audioBuffer);
    }

    /// <summary>
    /// Initialize feature analyzer and compute features
    /// </summary>
    public void Initialize(int windowSize, int hopSize, AudioFeature[] features, Action onComplete = null) {
        ResetAnalysis(windowSize, hopSize, features, onComplete);
    }

    /// <summary>
    /// Set/reset feature analyzer, which will occur whenever there is a new window or hop size
    /// </summary>
    public void ResetAnalysis(int windowSize, int hopSize, AudioFeature[] features, Action onComplete = null) {
        featureAnalyzer = new AudioFeatureAnalyzer(windowSize, hopSize, audioBuffer);
        playbackEventPool = new ObjectPool<PlaybackEvent>(
            createFunc : () => {
                var playbackEvent = new PlaybackEvent();
                playbackEvent.onComplete += () => {
                    playbackEventPool.Release(playbackEvent);
                };
                return playbackEvent;
            }, 
            // actionOnRelease : playbackEvent => playbackEvent.Reset(),
            defaultCapacity : WindowTimes.Length
        );
        RunBatchAnalysis(features, onComplete);
    }

    /// <summary>
    /// Batch computes the given features, and invokes onComplete when finished. Cached features
    /// will not be recomputed.
    /// </summary>
    public void RunBatchAnalysis(AudioFeature[] features, Action onComplete = null) {
        if (featureAnalyzer == null) {
            Debug.LogError("Feature analyzer has not been set yet, run ResetAnalysis() before running batch analysis");
            return;
        }
        analyzerThread = new Thread(() => {
            Debug.Log("Running batch analysis on " + features.Length + " features");
            featureAnalyzer.BatchComputeFeatures(
                features, () => Dispatcher.RunOnMainThread(onComplete)
            );
        });
        analyzerThread.Start();
    }

    /// <summary>
    /// Plays a grain at a given index in the granular windows (grainID)
    /// </summary>
    public void PlayGrain(int grainID, float gain = 1f) {
        if (grainID >= 0 && grainID < WindowTimes.Length)
        {
            PlaybackEvent playbackEvent = playbackEventPool.Get();
            playbackEvent.Set(
                gain, 
                WindowTimes[grainID], 
                GetFeatureValue(AudioFeature.RMS, grainID),
                grainID
            );
            player.Play(playbackEvent);
        }
    }

    /// <summary>
    /// Returns the value of an AudioFeature computed for the current buffer, provided
    /// an index into the granular windows (grainID)
    /// </summary>
    public float GetFeatureValue(AudioFeature feature, int grainID, bool normalized = true, float normalizedHi = 1f, float normalizedLo = -1f) {
        var featureVector = featureAnalyzer.GetFeatureVector(feature);
        return normalized ? featureVector.GetNormalized(grainID, normalizedHi, normalizedLo) : featureVector[grainID];
    }

    /// <summary>
    /// Stops all playback and audio processing workers
    /// </summary>
    public void Stop() {
        player.StopAllCoroutines();
        analyzerThread?.Abort();
    }

    /// <summary>
    /// Returns a copy of the current audioBuffer, cropped based on the given grain IDs window times
    /// </summary>
    public DiscreteSignal GetCroppedBuffer(int[] grainIDs) {
        int newLength = (int)Mathf.Ceil((float)grainIDs.Length * (float)WindowTimes[0].duration * (float)audioBuffer.SamplingRate);
        float[] newSamples = new float[0];
        foreach (int grainID in grainIDs) {
            var windowTime = WindowTimes[grainID];
            var sampleRange = windowTime.GetSampleRange(audioBuffer.SamplingRate);
            newSamples = MemoryOperationExtensions.MergeWithArray(
                newSamples, 
                audioBuffer[sampleRange.start, sampleRange.end].Samples
            );
        }
        return new DiscreteSignal(audioBuffer.SamplingRate, newSamples);
    }

    /// <summary>
    /// Returns a copy of the current audioBuffer cropped starting at 
    /// the playhead of grainStart and continuing to grainEnd
    /// </summary>
    public DiscreteSignal GetCroppedBuffer(int grainStart, int grainEnd) {
        if (grainEnd <= grainStart) {
            Debug.LogError("grainEnd must be greater than grainStart");
            return null;
        }
        var sampleRange = WindowTimes[grainStart].GetSampleRange(audioBuffer.SamplingRate);
        var endSampleRange = WindowTimes[grainEnd].GetSampleRange(audioBuffer.SamplingRate);
        return audioBuffer[sampleRange.start, endSampleRange.end];
    }

    /// <summary>
    /// Returns a concatenated version of the current audioBuffer and another buffer
    /// </summary>
    public DiscreteSignal ConcatenateBuffer(GranularBuffer otherAudioBuffer) {
        return DiscreteSignalExtensions.Concatenate(audioBuffer, otherAudioBuffer.audioBuffer);
    }
}
