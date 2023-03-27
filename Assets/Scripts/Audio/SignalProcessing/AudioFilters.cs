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
using System.Threading;

public static class AudioFilters {

    // public static DiscreteSignal FilterSignalRMS(DiscreteSignal signal, float thresholdDb, int _windowSize) {
    //     var currentTime = AudioSettings.dspTime;
    //     float[] windowSamples = Window.OfType(WindowTypes.Hamming, _windowSize);
    //     List<float> filteredSignal = new List<float>();
    //     float[] _window = new float[_windowSize];
    //     float threshold = DBToRMS(thresholdDb);
    //     for (int i = 0; i < signal.Length / _windowSize; i++) {
    //         float rms = signal.Rms(i * _windowSize, (i * _windowSize) +  _windowSize);
    //         if (rms > threshold)
    //             filteredSignal.AddRange(signal.Samples.Skip(i * _windowSize).Take(_windowSize));
    //     }

    //     var newSignal = new DiscreteSignal(signal.SamplingRate, filteredSignal.ToArray());
    //     var elapsedTime = AudioSettings.dspTime - currentTime;
    //     Debug.Log($"Finished filtering audio RMS in {elapsedTime} s");
    //     return newSignal;
    // }

    public static DiscreteSignal StripSilence(DiscreteSignal signal, float thresholdDb, int windowSize) {
        var currentTime = AudioSettings.dspTime;
        var threshold = DBToRMS(thresholdDb);
        var maxSamples = (signal.Length / windowSize) * windowSize;
        int filteredLength = 0;
        var windowIndex = 0;
        float rmsSum = 0;
        for (int i = 0; i < maxSamples; i++)
        {
            var sampleIndex = i % windowSize;
            signal[filteredLength + windowIndex] = signal[i]; // overwrite samples in place
            rmsSum += signal[i] * signal[i];

            if (sampleIndex == windowSize - 1)
            {
                var rms = Mathf.Sqrt(rmsSum / windowSize);
                if (rms > threshold)
                {
                    filteredLength += windowSize;
                }
                rmsSum = 0;
            }

            windowIndex = (windowIndex + 1) % windowSize;
        }

        var newSignal = signal[0, filteredLength];
        var elapsedTime = AudioSettings.dspTime - currentTime;
        Debug.Log($"Finished filtering audio RMS in {elapsedTime} s");
        return newSignal;
    }

    private static float RMS(float[] samples) {
        float sum = 0;
        for (int i = 0; i < samples.Length; i++)
            sum += samples[i] * samples[i];
        return Mathf.Sqrt(sum / samples.Length);
    }

    public static float RMSToDB(float rms) {
        return 20 * (float)Math.Log10(rms);
    }

    public static float DBToRMS(float db) {
        return (float)Math.Pow(10, db / 20);
    }
}


// DiscreteSignal x = signal[i * windowSize, (i * windowSize) +  windowSize].Copy();
// WindowExtensions.ApplyWindow(x, windowSamples);
// float rms = x.Rms();