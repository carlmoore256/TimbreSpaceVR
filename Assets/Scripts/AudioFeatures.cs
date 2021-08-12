using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
//using NWaves;
using NWaves.Signals;
using NWaves.Audio;
using NWaves.FeatureExtractors.Multi;
using NWaves.FeatureExtractors.Options;
using NWaves.FeatureExtractors;
using NWaves.FeatureExtractors.Base;
using NWaves.Windows;
using NWaves.Filters.Base;

public class AudioFeatures : MonoBehaviour
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
    List<float[]> ComputeMfccVectors(DiscreteSignal signal,
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

    float[][] WindowAudio(DiscreteSignal signal, int windowSize, int hop)
    {
        int numWindows = (int)((signal.Length - windowSize - 1) / hop);
        float[][] windowedAudio = new float[numWindows][];
        float[] win = Window.OfType(WindowTypes.Hamming, windowSize);

        for (int i = 0; i < numWindows; i++)
        {
            float[] thisWindow = signal[i * hop, (i * hop) + windowSize].Samples;
            thisWindow.ApplyWindow(win);
            windowedAudio[i] = thisWindow;
        }
        return windowedAudio;
    }

    //int[] autoWindowHop(int sigLength)
    //{
        //sigLength /= 
    //}



    public float[][] LoadAudioFeatures(string path)
    {
        //DiscreteSignal audio = ReadAudioMono(path);
        //List<float[]> mfccVectors = ComputeMfccVectors(audio, 8, 2048, 512, 26);
        float[][] features = ExtractMultiFeatures(ReadAudioMono(path));
        return features;
    }

    // fill a GrainFeatures array with features based on audio file path
    public GrainFeatures[] GenerateAudioFeatures(string path, int windowSize, int hop, int mfccs)
    {
        DiscreteSignal signal = ReadAudioMono(path);

        // autocalculate ideal size
        //if (windowSize == 0)
        //{
        //    windowSize = 
        //}

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
