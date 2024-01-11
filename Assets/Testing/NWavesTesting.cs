using System;
using NWaves.Signals;
using NWaves.Audio;
using NWaves.FeatureExtractors.Multi;
using NWaves.FeatureExtractors.Options;
using NWaves.FeatureExtractors;
using NWaves.FeatureExtractors.Base;
using NWaves.Windows;
using NWaves.Filters.Base;

public class TestNWaves
{
	public static void Main()
	{		
		float[] samples = new [] { 0.5f, 0.2f, -0.3f, 1.2f, 1.6f, -1.8f, 0.3f, -0.2f };

		var signal = new DiscreteSignal(8000, samples).Repeat(100);

		var length = signal.Length;
		var duration = signal.Duration;
		
		Console.WriteLine("length " + length);
		
		var windowSize = 1024;
		var frameDuration = windowSize / (double)signal.SamplingRate;
        var hopDuration = 512 / (double)signal.SamplingRate;
		
		Console.WriteLine("hopDuration " + hopDuration);
		
		FeatureExtractor extractor = new MfccExtractor(new MfccOptions {
                    SamplingRate = signal.SamplingRate,
                    FrameDuration = 1024,
                    HopDuration = hopDuration,
                    FftSize = windowSize,
                    FilterBankSize = 26,
                    FeatureCount = 8
                });
		
		Console.WriteLine("FeatureExtractor " + extractor);

	}
}