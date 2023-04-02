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

public class AudioFeatures
{
    public static AudioFeature RandomAudioFeature() {
        // return a random audio feature from the enum
        int numFeatures = Enum.GetNames(typeof(AudioFeature)).Length;
        return (AudioFeature)UnityEngine.Random.Range(0, numFeatures);
    }
}
