using System;

public class AudioFeatureUtils
{
    public static AudioFeature RandomAudioFeature() {
        // return a random audio feature from the enum
        int numFeatures = Enum.GetNames(typeof(AudioFeature)).Length;
        return (AudioFeature)UnityEngine.Random.Range(0, numFeatures);
    }
}
