using UnityEngine;
using System.Collections.Generic;
using System;
using NWaves.Windows;

public class Windowing {
    private static Windowing _instance;
    private static readonly object _lock = new object();
    private readonly Dictionary<(WindowTypes, int), float[]> _windowCache;

    private Windowing()
    {
        _windowCache = new Dictionary<(WindowTypes, int), float[]>();
    }

    public static Windowing Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new Windowing();
                    }
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Provides a cached window if one exists, otherwise generate it
    /// </summary>
    public float[] GetWindow(WindowTypes windowType, int size)
    {
        var key = (windowType, size);

        if (_windowCache.TryGetValue(key, out float[] window))
        {
            return window;
        }

        window = GenerateWindow(windowType, size);
        _windowCache[key] = window;
        return window;
    }

    private float[] GenerateWindow(WindowTypes windowType, int size)
    {
        return Window.OfType(windowType, size);
    }

    // public static float[] CosineWindow(int windowSamples) {
    //     float[] window = new float[windowSamples];
    //     for (int i = 0; i < windowSamples; i++) {
    //         window[i] = (Mathf.Cos(Mathf.PI * 2 * ((float)i / (float)windowSamples)) - 1f) * -0.5f;
    //     }
    //     return window;
    // }
}