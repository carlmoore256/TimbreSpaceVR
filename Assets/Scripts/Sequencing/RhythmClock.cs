using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[SerializeField]
public class RhythmClock
{

    public TimeSignature TimeSignature
    {
        get => _timeSignature; set
        {
            _timeSignature = value;
            OnTimeSignatureChanged?.Invoke();
        }
    }

    public float BPM
    {
        get => _bpm; set
        {
            _bpm = value;
            OnTempoChanged?.Invoke();
        }
    }

    // we're gonna have to add a metronome, with the ability to subscribe
    public Action OnTick;

    public Action OnTempoChanged;
    public Action OnTimeSignatureChanged;


    // currentBeatIndex may not need to be set, instead we
    // should set currentBeatTime, because sometimes updates
    // will not have a beatIndex but will always have a time
    // that way, we can work out currentBeatIndex
    public BeatIndex CurrentBeatIndex
    {
        get => BeatIndexFromTime(CurrentBeatTime);
    }

    public double CurrentBeatTime { get; set; }

    public int CurrentBar
    {
        get => CurrentBeatIndex.Bar;
    }




    private TimeSignature _timeSignature = new TimeSignature(4, NoteValue.Quarter);
    private float _bpm = 120f;

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

    public double BeatDuration() => 60.0 / BPM;
    public double BarDuration() => (60.0 / BPM) * TimeSignature.BeatsPerBar;

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
        return TimeFromNotePosition(beatIndex.Bar, beatIndex.NoteValue, beatIndex.RelativeBeatPosition);
    }

    public double NormalizedPositionBetween(BeatIndex beatIndexA, BeatIndex beatIndexB, BeatIndex beatBetween)
    {
        double timeA = TimeFromBeatIndex(beatIndexA);
        double timeB = TimeFromBeatIndex(beatIndexB);
        double time = TimeFromBeatIndex(beatBetween);
        return (time - timeA) / (timeB - timeA);
    }

    // quantizes to the closest beat on TimeSignature.BeatsPerBar
    public BeatIndex BeatIndexFromTime(double time)
    {
        double bar = BarFromTime(time);
        double barTime = TimeFromBars((int)bar);
        double beatTime = time - barTime;
        double beat = beatTime / (60.0 / BPM);
        double beatPosition = beat % TimeSignature.BeatsPerBar;
        double noteValuePosition = beat / TimeSignature.BeatsPerBar;
        return new BeatIndex((int)bar, TimeSignature.BaseNoteValue, (int)beatPosition, TimeSignature);
    }

}