using System;
using NWaves.Signals;
using UnityEngine;
using System.Threading.Tasks;

public static class AudioFilters {

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

    public static void StripSilenceAsync(DiscreteSignal signal, float thresholdDb, int windowSize, Action<DiscreteSignal> callback) {
        var tcs = new TaskCompletionSource<DiscreteSignal>();
        Task.Run(() => {
            var strippedSignal = AudioFilters.StripSilence(signal, thresholdDb, windowSize);
            tcs.SetResult(strippedSignal);
        }).ContinueWith(task => {
            if (task.Status == TaskStatus.RanToCompletion) {
                callback(tcs.Task.Result);
            }
        }, TaskScheduler.FromCurrentSynchronizationContext());
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