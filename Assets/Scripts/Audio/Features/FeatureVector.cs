using System;
using System.Collections.Generic;
using NWaves.Signals;
using NWaves.Audio;
using NWaves.FeatureExtractors.Multi;
using NWaves.FeatureExtractors.Options;
using NWaves.FeatureExtractors;
using NWaves.FeatureExtractors.Base;
using NWaves.Windows;
using NWaves.Filters.Base;

using System.Linq;
using UnityEngine;



public class FeatureVector
{
    public AudioFeature feature;
    public float[] values;
    public float min;
    public float max;
    public int Length { get { return values.Length; } }
    public FeatureVector(float[] values, AudioFeature feature)//, int windowSize)
    {
        this.values = values;
        this.feature = feature;
        this.min = values.Min();
        this.max = values.Max();
    }

    public float GetNormalized(int index, float rangeLow = 0, float rangeHigh = 1)
    {
        return (((values[index] - min) / (max - min)) * (rangeHigh - rangeLow)) + rangeLow;
    }

    /// <summary>
    /// Return an array of indexes that sort the array values
    /// </summary>
    public int[] Argsort(bool ascending = true)
    {
        var selection = Enumerable.Range(0, values.Length)
                        .Select(index => new KeyValuePair<int, float>(index, values[index]));

        if (ascending) {
            return selection.OrderBy(pair => pair.Value)
                .Select(pair => pair.Key)
                .ToArray();
        } else {
            return selection.OrderByDescending(pair => pair.Value)
                .Select(pair => pair.Key)
                .ToArray();
        }
    }

    public float this[int index]
    {
        get { return values[index]; }
        set { values[index] = value; }
    }
  }