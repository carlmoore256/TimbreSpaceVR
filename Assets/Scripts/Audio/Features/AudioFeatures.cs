using System;
using System.Collections.Generic;
using UnityEngine;
using NWaves.Signals;
using NWaves.Audio;
using NWaves.FeatureExtractors.Multi;
using NWaves.FeatureExtractors.Options;
using NWaves.FeatureExtractors;
using NWaves.FeatureExtractors.Base;
using NWaves.Windows;
using NWaves.Filters.Base;


// consider using unity's job system to parallelize feature extraction
public class AudioFeatures
{
    List<float[]> ComputeMfccVectors(DiscreteSignal signal, int numFeatures, int fftSize, int hop, int filterBankSize)
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
        var test = mfccExtractor.FeatureDescriptions;
        var mfccVectors = mfccExtractor.ComputeFrom(signal);
        FeaturePostProcessing.NormalizeMean(mfccVectors);
        return mfccVectors;
    }

    /// <summary>
    /// Returns a list of windowed audio signals
    /// </summary>
    float[][] WindowAudio(DiscreteSignal signal, int windowSize, int hop, WindowTypes windowType = WindowTypes.Hamming, float[] win = null)
    {
        int numHop = (int)Math.Ceiling((signal.Length - windowSize) / (double)hop);
        int numWindows = numHop;
        int remainder = signal.Length - (numHop * hop);
        if (remainder >= windowSize)
            numWindows += 1;
        // Debug.Log($"Windowing Audio: Window Size {windowSize} | Hop {hop} | Num Hop {numHop} | Num Windows {numWindows} | Signal Length {signal.Length} | Remainder {remainder}");
        float[][] windowedAudio = new float[numWindows][];
        if (win == null) // allow for cached window to be passed in
            win = Window.OfType(windowType, windowSize);
        for (int i = 0; i < numWindows; i++)
        {
            float[] clip = signal[i * hop, (i * hop) + windowSize].Samples;
            clip.ApplyWindow(win);
            windowedAudio[i] = clip;
        }
        return windowedAudio;
    }

    /// <summary>
    /// Generate a list of feature vectors for time and spectral domain
    /// </summary>
    /// <param name="signal">DiscreteSignal (audio signal) to extract features from</param>
    /// <param name="windowSize">Size of the window to use for feature extraction</param>
    /// <param name="hop">Hop size for feature extraction</param>
    /// <param name="normalize">Whether to mean normalize the features</param>
    /// <param name="featureList">List of features to extract. Supply as a string, such as  "centroid, flattness, c1+c2+c3"</param>
    float[][] ExtractMultiFeatures(DiscreteSignal signal,
            int windowSize, int hop, 
            bool normalize=true, 
            string featureList="all",
            float[] vectorMeans=null)
    {
        var opts = new MultiFeatureOptions {
            SamplingRate = signal.SamplingRate,
            FeatureList = featureList,
            FrameDuration = (float)windowSize / (float)signal.SamplingRate,
            HopDuration = (float)hop / (float)signal.SamplingRate,
            FftSize = windowSize
        };
        var spExtractor = new SpectralFeaturesExtractor(opts);
        var tdExtractor = new TimeDomainFeaturesExtractor(opts);
        float[][] vectors = FeaturePostProcessing.Join(
            tdExtractor.ParallelComputeFrom(signal),
            spExtractor.ParallelComputeFrom(signal));
        if (normalize)
            FeaturePostProcessing.NormalizeMean(vectors);
        return vectors;
    }
    
     public void ComputeFeatureBlocks(DiscreteSignal signal, int windowSize, int hop, int mfccs,
            int windowsPerBlock, Action<GrainFeatures[]> callback) {

        // DiscreteSignal signal = AudioIO.ReadMonoAudioFile(path);
        int blockSizeSamples = ((windowsPerBlock - 1) * hop) + windowSize;
        int numFrames = (int)(signal.Length - windowSize - 1) / hop;
        // int numBlocks = (int)(signal.Length / (float)blockSizeSamples);
        int numBlocks = signal.Length / (windowsPerBlock * hop);
        // int sigLen = (hop * numFrames) + windowSize - 1;
        // signal = signal[0, sigLen];

        // var opts = new MultiFeatureOptions {
        //     SamplingRate = signal.SamplingRate,
        //     FeatureList = "all",
        //     FrameDuration = (float)windowSize / (float)signal.SamplingRate,
        //     HopDuration = (float)hop / (float)signal.SamplingRate,
        //     FftSize = windowSize
        // };

        // var spExtractor = new SpectralFeaturesExtractor(opts);
        // var tdExtractor = new TimeDomainFeaturesExtractor(opts);
        float[] win = Window.OfType(WindowTypes.Hamming, windowSize);

        float[] vectorMeans = null;

        for (int i = 0; i < numBlocks; i++) {
            int start = i * (windowsPerBlock * hop);
            int end = start + blockSizeSamples;
            Debug.Log($"Getting features for block {i} of {numBlocks} | Block Size: {blockSizeSamples} | window size {windowSize} | hop {hop} | windows per block {windowsPerBlock} | num frames {numFrames} | num blocks {numBlocks} | start {start} - end {end}");
            DiscreteSignal clip = signal[start, end];
            float[][] audioFrames = WindowAudio(clip, windowSize, hop, win : win);
            float[][] mfccCoeffs = ComputeMfccVectors(clip, mfccs, windowSize, hop, 26).ToArray();
            float[][] vectors = ExtractMultiFeatures(clip, windowSize, hop, false, "all");

            // eager normalization
            if(vectorMeans == null)
                vectorMeans = CustomPostProcessing.GetVectorMeans(vectors);
            CustomPostProcessing.NormalizeMeanToReference(vectors, vectorMeans);
            
            // float[][] vectors = FeaturePostProcessing.Join(
            //     spExtractor.ParallelComputeFrom(clip),
            //     tdExtractor.ParallelComputeFrom(clip));
            // FeaturePostProcessing.NormalizeMean(vectors);
            GrainFeatures[] grainFeatures = new GrainFeatures[windowsPerBlock];
            for(int j = 0; j < windowsPerBlock; j++) {
                Debug.Log($"Making window {j+1} of {windowsPerBlock} | Block Size: {blockSizeSamples} | Audio Frame Length {audioFrames.Length} | MFCC Length {mfccCoeffs.Length} | Vectors Length {vectors.Length}");
                float[] v = vectors[j];
                float[] contrast = new float[] { v[12], v[13], v[14], v[15], v[16] };
                grainFeatures[j] = new GrainFeatures(
                    audioFrames[j],
                    mfccCoeffs[j],
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
                    i/(float)numFrames, // normalized index along audio file
                    signal.SamplingRate
                );
            }
            callback.Invoke(grainFeatures);
        }
    }

    bool COPY_AUDIO_BUFF = false;

    /// <summary>
    /// Generate an array of Grain Features from a discrete signal
    /// </summary>
    /// <param name="path">Path to audio file</param>
    /// <param name="windowSize">Size of window in samples</param>
    /// <param name="hop">Hop size in samples</param>
    /// <param name="mfccs">Number of mfccs to extract</param>
    public GrainFeatures[] GenerateAudioFeatures(DiscreteSignal signal, int windowSize, int hop, int mfccs)
    {
        float[][] audioFrames = WindowAudio(signal, windowSize, hop);
        // clip the audio to total frames size so that features line up with windows
        // int sigLen = (hop * audioFrames.Length) + windowSize - 1;
        // signal = signal[0, sigLen];

        float[][] mfccCoeffs = ComputeMfccVectors(signal, mfccs, windowSize, hop, 26).ToArray();
        float[][] vectors = ExtractMultiFeatures(signal, windowSize, hop, true, "all");

        GrainFeatures[] grainFeatures = new GrainFeatures[audioFrames.Length];
        
        for (int i = 0; i < audioFrames.Length - 1; i++)
        {
            float[] v = vectors[i];
            float[] contrast = new float[] { v[12], v[13], v[14], v[15], v[16] };
            float[] audioSamples;
            if (COPY_AUDIO_BUFF) {
                audioSamples = new float[audioFrames[i].Length];
                audioFrames[i].CopyTo(audioSamples, 0);
            } else {
                audioSamples = audioFrames[i];
            }
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




// // returns mfccs along with windowed audio
// public List<float[][]> LoadAudioMfccs(
//     string path, int numFeatures, int fftSize, int hop, int filterBankSize)
// {
//     DiscreteSignal signal = AudioIO.ReadMonoAudioFile(path);
//     List<float[]> mfccVectors = ComputeMfccVectors(signal, numFeatures, fftSize, hop, filterBankSize);
//     float[][] mfccCoeffs = mfccVectors.ToArray();
//     float[][] windowedAudio = WindowAudio(signal, fftSize, hop);
//     List<float[][]> audioMfccs = new List<float[][]>() { windowedAudio, mfccCoeffs };
//     return audioMfccs;
// }
