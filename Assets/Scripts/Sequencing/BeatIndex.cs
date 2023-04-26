using UnityEngine;
using System;

public class BeatIndex
{
    public int Bar { get; set; }
    public NoteValue NoteValue { get; set; }
    public int NoteValuePosition { get; set; }

    public override string ToString()
    {
        return $"Bar: {Bar}, NoteValue: {NoteValue}, NoteValuePosition: {NoteValuePosition}";
    }
}