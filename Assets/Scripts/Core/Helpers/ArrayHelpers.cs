using UnityEngine;

public static class ArrayHelpers {
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
}