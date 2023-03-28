using System.Collections;
using System.Collections.Generic;
using UnityEngine.Pool;
using UnityEngine;
using NWaves.Signals;
using System;
using System.Threading;

// buffer based on audioFeatures
public class GranularBuffer {
    public AudioFeatureAnalyzer featureAnalyzer;
    public PolyvoicePlayer player;
    private DiscreteSignal audioBuffer;
    private ObjectPool<PlaybackEvent> playbackEventPool; // Object pool for PlaybackEvent instances
    public WindowTime[] WindowTimes {get {
        if (featureAnalyzer == null) {
            Debug.LogError("Feature analyzer has not been set yet, run ResetAnalysis() before accessing WindowTimes");
            return null;
        }
        return featureAnalyzer.WindowTimes;
    } } // Array of WindowTime instances
    private Thread analyzerThread;
    
    public GranularBuffer(DiscreteSignal audioBuffer, PolyvoicePlayer player) {
        this.audioBuffer = audioBuffer;
        this.player = player;
        player.SetAudioBuffer(audioBuffer);
    }

    /// <summary>
    /// Set/reset feature analyzer, which needs to occur whenever there is a new
    /// window or hop size
    /// </summary>
    public void ResetAnalysis(int windowSize, int hopSize, AudioFeature[] features, Action onComplete = null) {
        featureAnalyzer = new AudioFeatureAnalyzer(windowSize, hopSize, audioBuffer);
        playbackEventPool = new ObjectPool<PlaybackEvent>(
            () => {
                var playbackEvent = new PlaybackEvent();
                playbackEvent.onComplete += () => playbackEventPool.Release(playbackEvent);
                return playbackEvent;
            }, 
            playbackEvent => playbackEvent.Reset(),
            defaultCapacity: WindowTimes.Length,
            maxSize: WindowTimes.Length * 10
        );
        RunBatchAnalysis(features, onComplete);
    }

    /// <summary>
    /// Batch computes the given features, and fires onComplete when finished
    /// <summary>
    public void RunBatchAnalysis(AudioFeature[] features, Action onComplete = null) {
        if (featureAnalyzer == null) {
            Debug.LogError("Feature analyzer has not been set yet, run ResetAnalysis() before running batch analysis");
            return;
        }
        
        // TO DO: ADD THREADING
        analyzerThread = new Thread(() => {
            Debug.Log("Runn/ing batch analysis on " + features.Length + " features");
            featureAnalyzer.BatchComputeFeatures(
                audioBuffer, 
                features, 
                () => Dispatcher.RunOnMainThread(onComplete) // return to main thread for onComplete
            );
        });
    }

    /// <summary>
    /// Plays a grain at a given index
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
    /// Returns the value of a feature at a given index
    /// </summary>
    public float GetFeatureValue(AudioFeature feature, int index, bool normalized = true) {
        var featureVector = featureAnalyzer.GetFeatureVector(feature);
        return normalized ? featureVector.GetNormalized(index, 0, 1) : featureVector[index];
    }
}
