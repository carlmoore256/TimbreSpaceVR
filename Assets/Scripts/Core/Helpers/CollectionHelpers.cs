using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public static class CollectionHelpers {
    public static void Shuffle<T>(T[] array) {
        int n = array.Length;
        for (int i = 0; i < n; i++) {
            int r = i + (int)(UnityEngine.Random.value * (n - i));
            T temp = array[r];
            array[r] = array[i];
            array[i] = temp;
        }
    }


    public static void FloatArrReplaceNaN(float[] array, float replacement = 0f) {
        for (int i = 0; i < array.Length; i++) {
            if (float.IsNaN(array[i])) {
                array[i] = replacement;
            }
        }
    }

    public static float[] Transpose(float[][] array) {
        int width = array.Length;
        int height = array[0].Length;

        float[] result = new float[width * height];

        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                result[i * height + j] = array[i][j];
            }
        }

        return result;
    }

    public static List<int> ArgsortTopPairs(Dictionary<int, float> dict1, Dictionary<int, float> dict2, int topN, Func<float, float, float> combineFunc)
    {
        var combinedDict = new Dictionary<int, float>();

        foreach (var kvp in dict1)
        {
            int index = kvp.Key;
            float value1 = kvp.Value;

            if (dict2.TryGetValue(index, out float value2))
            {
                float combinedValue = combineFunc(value1, value2);
                combinedDict[index] = combinedValue;
            }
        }

        var sortedIndexes = combinedDict.OrderByDescending(kvp => kvp.Value).Take(topN).Select(kvp => kvp.Key).ToList();
        return sortedIndexes;
    }
}