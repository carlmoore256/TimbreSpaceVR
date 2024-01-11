using UnityEngine;
using System;
using NWaves.Signals;


public class BeatIndex
{
    public int Bar { get; set; }
    public NoteValue NoteValue { get; set; }
    public int RelativeBeatPosition { get; set; }
    public TimeSignature TimeSignature { get; set; }

    public BeatIndex(int bar, NoteValue noteValue, int relativeBeatPosition, TimeSignature timeSignature)
    {
        Bar = bar;
        NoteValue = noteValue;
        RelativeBeatPosition = relativeBeatPosition;
        TimeSignature = timeSignature;
    }

    public override string ToString()
    {
        return $"Bar: {Bar}, NoteValue: {NoteValue}, NoteValuePosition: {RelativeBeatPosition}";
    }

    public float NormalizedPositionWithinBar()
    {
        float totalNoteValuePositionsPerBar = TimeSignature.BeatsPerBar * (float)TimeSignature.BaseNoteValue;
        float currentNoteValuePosition = (float)NoteValue * RelativeBeatPosition;
        return currentNoteValuePosition / totalNoteValuePositionsPerBar;
    }

    public void Quantize(NoteValue precision)
    {
        var noteValue = (int)NoteValue;
        var precisionValue = (int)precision;
        var remainder = noteValue % precisionValue;
        // if (remainder != 0)
        // {
        //     NoteValue = (NoteValue)(noteValue - remainder);
        // }

        // we have to find the closest normalized position at new value
    }

    public void QuantizeToBar()
    {
        RelativeBeatPosition = 0;
    }

    public void QuantizeToBar(int bar)
    {
        Bar = bar;
        RelativeBeatPosition = 0;
    }

    public static BeatIndex Lerp(BeatIndex a, BeatIndex b, float t, NoteValue quantizationLevel)
    {
        if (a.TimeSignature != b.TimeSignature)
        {
            throw new ArgumentException("Both BeatIndexes must have the same TimeSignature.");
        }

        TimeSignature timeSignature = a.TimeSignature;
        float totalNoteValuePositionsPerBar = timeSignature.BeatsPerBar * (float)timeSignature.BaseNoteValue;

        float aGlobalPosition = a.Bar * totalNoteValuePositionsPerBar + (float)a.NoteValue * a.RelativeBeatPosition;
        float bGlobalPosition = b.Bar * totalNoteValuePositionsPerBar + (float)b.NoteValue * b.RelativeBeatPosition;
        float interpolatedPosition = Mathf.Lerp(aGlobalPosition, bGlobalPosition, t);

        // Quantize the interpolated position
        int quantizationDivisions = (int)(totalNoteValuePositionsPerBar / (int)quantizationLevel);
        int quantizedIndex = Mathf.RoundToInt(interpolatedPosition * quantizationDivisions / totalNoteValuePositionsPerBar) * (int)quantizationLevel;

        // Calculate the bar, note value, and relative beat position
        int bar = quantizedIndex / (int)totalNoteValuePositionsPerBar;
        int relativeBeatPosition = quantizedIndex % (int)totalNoteValuePositionsPerBar;

        return new BeatIndex(bar, quantizationLevel, relativeBeatPosition, timeSignature);
    }


    # region Operators

    public static bool operator <(BeatIndex a, BeatIndex b)
    {
        return a.NormalizedPositionWithinBar() < b.NormalizedPositionWithinBar();
    }

    public static bool operator >(BeatIndex a, BeatIndex b)
    {
        return a.NormalizedPositionWithinBar() > b.NormalizedPositionWithinBar();
    }

    public static bool operator <=(BeatIndex a, BeatIndex b)
    {
        return a.NormalizedPositionWithinBar() <= b.NormalizedPositionWithinBar();
    }

    public static bool operator >=(BeatIndex a, BeatIndex b)
    {
        return a.NormalizedPositionWithinBar() >= b.NormalizedPositionWithinBar();
    }

    // public static bool operator ==(BeatIndex a, BeatIndex b)
    // {
    //     return Mathf.Approximately(a.NormalizedPositionWithinBar(), b.NormalizedPositionWithinBar());
    // }    
    
    // public static bool operator !=(BeatIndex a, BeatIndex b)
    // {
    //     return !(a == b);
    // }

    # endregion
}