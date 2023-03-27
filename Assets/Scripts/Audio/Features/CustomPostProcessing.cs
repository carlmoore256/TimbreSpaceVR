using System.Collections.Generic;
using System.Linq;
using NWaves.Signals;
using NWaves.Audio;
using UnityEngine;

public static class CustomPostProcessing {


    public static float[] GetVectorMeans(IList<float[]> vectors) {
        var featureCount = vectors[0].Length;
        float[] means = new float[featureCount];
        for (var i = 0; i < featureCount; i++) {
            means[i] = vectors.Average(t => t[i]);
        }
        return means;
    }
    
    public static void NormalizeMeanToReference(IList<float[]> vectors, float[] means)
    {
        if (vectors.Count < 2)
        {
            return;
        }

        var featureCount = vectors[0].Length;

        Debug.Log($"Feature count: {featureCount} | Means count: {means.Length}");
        for (var i = 0; i < featureCount; i++)
        {            
            foreach (var vector in vectors)
            {
                vector[i] -= means[i];
            }
        }
    }
}