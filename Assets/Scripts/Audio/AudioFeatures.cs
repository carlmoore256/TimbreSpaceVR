using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using NWaves.Signals;
using NWaves.Audio;
using NWaves.FeatureExtractors.Multi;
using NWaves.FeatureExtractors.Options;
using NWaves.FeatureExtractors;
using NWaves.FeatureExtractors.Base;
using NWaves.Windows;
using NWaves.Filters.Base;


public class AudioIO : ScriptableObject {
    public static DiscreteSignal ReadMonoAudioFile(string path) {
        DiscreteSignal signalMono;
        using (var stream = new FileStream(path, FileMode.Open))
        {
            var waveFile = new WaveFile(stream);
            if (waveFile.Signals.Count == 0) {
                Debug.LogError("Invalid number of channels for audio file: " + path);
                return null;
            }
            signalMono = waveFile[Channels.Average];
        }
        return signalMono;
    }

    public static DiscreteSignal[] ReadStereoAudioFile(string path) {
        DiscreteSignal[] signalStereo = new DiscreteSignal[2];
        using (var stream = new FileStream(path, FileMode.Open))
        {
            var waveFile = new WaveFile(stream);
            if (waveFile.Signals.Count == 0) {
                Debug.LogError("Invalid number of channels for audio file: " + path);
                return null;
            }
            if (waveFile.Signals.Count == 1)
            {
                signalStereo[0] = waveFile[Channels.Average];
                signalStereo[1] = waveFile[Channels.Average];
            }
            else if (waveFile.Signals.Count >= 2) {
                signalStereo[0] = waveFile[Channels.Left];
                signalStereo[1] = waveFile[Channels.Right];
            }
        }
        return signalStereo;
    }

    public static void WriteAudioFileMono(string path, DiscreteSignal signalMono) {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            var waveFile = new WaveFile(signalMono);
            waveFile.SaveTo(stream);
        }
    }

    public static void WriteAudioFileStereo(string path, DiscreteSignal[] signalStereo) {
        using (var stream = new FileStream(path, FileMode.Create))
        {
            var waveFile = new WaveFile(signalStereo);
            waveFile.SaveTo(stream);
        }
    }

    // public static void CaptureMicInput()
}

// consider using unity's job system to parallelize feature extraction

public class AudioFeatures
{
    DiscreteSignal ReadAudioMono(string path)
    {
        DiscreteSignal audio;

        using (var stream = new FileStream(path, FileMode.Open))
        {
            var waveFile = new WaveFile(stream);
            audio = waveFile[Channels.Average];
        }

        return audio;
    }
    List<float[]> ComputeMfccVectors(
        DiscreteSignal signal,
        int numFeatures,
        int fftSize,
        int hop,
        int filterBankSize)
    {
        var mfccOptions = new MfccOptions
        {
            SamplingRate = signal.SamplingRate,
            FeatureCount = numFeatures,
            FrameDuration = (float)fftSize / signal.SamplingRate,
            HopDuration = (float)hop / signal.SamplingRate,
            FilterBankSize = filterBankSize,
        };
        var mfccExtractor = new MfccExtractor(mfccOptions);
        var mfccVectors = mfccExtractor.ComputeFrom(signal);
        FeaturePostProcessing.NormalizeMean(mfccVectors);

        return mfccVectors;
    }

    float[][] ExtractMultiFeatures(DiscreteSignal signal)
    {
        var opts = new MultiFeatureOptions
        {
            SamplingRate = signal.SamplingRate,
            FeatureList = "centroid, flattness, c1+c2+c3"
            //centroid, flattness, 
        };
        var spectralExtractor = new SpectralFeaturesExtractor(opts);
        var tdExtractor = new TimeDomainFeaturesExtractor(opts);
        float[][] featureVectors = FeaturePostProcessing.Join(
                  tdExtractor.ParallelComputeFrom(signal),
                  spectralExtractor.ParallelComputeFrom(signal));
        return featureVectors;
    }
    
    // returns mfccs along with windowed audio
    public List<float[][]> LoadAudioMfccs(string path, 
                                        int numFeatures, 
                                        int fftSize, 
                                        int hop, 
                                        int filterBankSize)
    {
        DiscreteSignal signal = ReadAudioMono(path);
        List<float[]> mfccVectors = ComputeMfccVectors(signal, numFeatures, fftSize, hop, filterBankSize);
        float[][] mfccCoeffs = mfccVectors.ToArray();
        float[][] windowedAudio = WindowAudio(signal, fftSize, hop);
        List<float[][]> audioMfccs = new List<float[][]>() { windowedAudio, mfccCoeffs };
        return audioMfccs;
    }


    /// <summary>
    /// Returns a list of windowed audio signals
    /// </summary>
    float[][] WindowAudio(DiscreteSignal signal, int windowSize, int hop, WindowTypes windowType = WindowTypes.Hamming)
    {
        int numWindows = (int)((signal.Length - windowSize - 1) / hop);
        float[][] windowedAudio = new float[numWindows][];
        float[] win = Window.OfType(windowType, windowSize);
        for (int i = 0; i < numWindows; i++)
        {
            float[] thisWindow = signal[i * hop, (i * hop) + windowSize].Samples;
            thisWindow.ApplyWindow(win);
            windowedAudio[i] = thisWindow;
        }
        return windowedAudio;
    }

    /// <summary>
    /// Loads an audio signal and returns a list of feature vectors
    /// </summary>
    /// <param name="path">Path to audio file</param>
    /// <param name="windowSize">Size of window in samples</param>
    /// <param name="hop">Hop size in samples</param>
    /// <param name="mfccs">Number of mfccs to extract</param>
    public GrainFeatures[] GenerateAudioFeatures(string path, int windowSize, int hop, int mfccs)
    {
        DiscreteSignal signal = AudioIO.ReadMonoAudioFile(path);
        float[][] audioFrames = WindowAudio(signal, windowSize, hop);
        // clip the audio to total frames size so that features line up with windows
        int sigLen = (hop * audioFrames.Length) + windowSize - 1;
        signal = signal[0, sigLen];
        float[][] mfccCoeffs = ComputeMfccVectors(signal, mfccs, windowSize, hop, 26).ToArray();

        var opts = new MultiFeatureOptions
        {
            SamplingRate = signal.SamplingRate,
            FeatureList = "all",
            FrameDuration = (float)windowSize / signal.SamplingRate,
            HopDuration = (float)hop / signal.SamplingRate,
            FftSize = windowSize
        };

        var spectralExtractor = new SpectralFeaturesExtractor(opts);
        var timeDomainExtractor = new TimeDomainFeaturesExtractor(opts);

        float[][] vectors = FeaturePostProcessing.Join(
              timeDomainExtractor.ParallelComputeFrom(signal),
              spectralExtractor.ParallelComputeFrom(signal));

        FeaturePostProcessing.NormalizeMean(vectors);
        GrainFeatures[] grainFeatures = new GrainFeatures[audioFrames.Length];
        for (int i = 0; i < audioFrames.Length - 1; i++)
        {
            float[] v = vectors[i];
            float[] contrast = new float[] { v[12], v[13], v[14], v[15], v[16] };
            float[] audioSamples = new float[audioFrames[i].Length];
            audioFrames[i].CopyTo(audioSamples, 0);
            grainFeatures[i] = new GrainFeatures(
                audioSamples,
                mfccCoeffs[i],
                contrast,
                v[4],
                v[5],
                v[6],
                v[7],
                v[8],
                v[9],
                v[10],
                v[11],
                v[0],
                v[1],
                v[2],
                v[3],
                i/(float)audioFrames.Length, // normalized index along audio file
                signal.SamplingRate);
        }

        return grainFeatures;
    }

}
