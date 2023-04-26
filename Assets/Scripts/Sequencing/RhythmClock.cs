using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[SerializeField]
public class RhythmClock
{

    public TimeSignature TimeSignature { get => _timeSignature; set {
        _timeSignature = value;
        OnTimeSignatureChanged?.Invoke();
    } }

    public float BPM { get => _bpm; set {
        _bpm = value;
        OnTempoChanged?.Invoke();
    } }

    public Action OnTempoChanged;
    public Action OnTimeSignatureChanged;
      
    private TimeSignature _timeSignature = new TimeSignature(4, NoteValue.Quarter);
    private float _bpm  = 120f; 

    public double BarFromTime(double time)
    {
        return time / (60.0 / BPM) / TimeSignature.BeatsPerBar;
    }

    public float BeatPositionFromTime(double time)
    {
        return (float)(time / (60.0 / BPM));
    }

    /// <summary>
    /// Return a time given a float beat. Round numbers represent a full beat
    /// </summary>
    public double BeatPositionToTime(float beat)
    {
        return (60.0 / BPM) * beat;
    }

    public float TimeToBeatPosition(double time)
    {
        return (float)(time * BPM / 60.0);
    }

    public double TimeFromBars(int bars)
    {
        return BarDuration() * bars;
    }

    public double BarDuration() =>  (60.0 / BPM) * TimeSignature.BeatsPerBar;

    public double TimePerBeat(NoteValue noteValue) => (60.0 / BPM) * ((double)TimeSignature.BaseNoteValue / (double)noteValue);


    /// <summary>
    /// Return a double in relativeScheduleTime within the sequence where the note will be played
    /// </summary>
    public double TimeFromNotePosition(int bar, NoteValue noteValue, int noteValuePosition)
    {
        double barTime = TimeFromBars(bar);
        double beatTime = TimePerBeat(noteValue) * noteValuePosition;
        double time = barTime + beatTime;
        return time;
    }

    public double TimeFromBeatIndex(BeatIndex beatIndex)
    {
        return TimeFromNotePosition(beatIndex.Bar, beatIndex.NoteValue, beatIndex.NoteValuePosition);
    }

}