using System;
using UnityEngine;

public enum NoteValue
{
    Whole = 1,
    Half = 2,
    Quarter = 4,
    Eighth = 8,
    Sixteenth = 16,
    ThirtySecond = 32,
    SixtyFourth = 64,
    HundredTwentyEighth = 128
}

public class TimeSignature
{
    public int BeatsPerBar { get; set; }
    public NoteValue BaseNoteValue { get; set; }


    public TimeSignature(int beatsPerBar, NoteValue baseNoteValue)
    {
        BeatsPerBar = beatsPerBar;
        BaseNoteValue = baseNoteValue;
    }
}
