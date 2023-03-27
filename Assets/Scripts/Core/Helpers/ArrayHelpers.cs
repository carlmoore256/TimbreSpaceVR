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
}