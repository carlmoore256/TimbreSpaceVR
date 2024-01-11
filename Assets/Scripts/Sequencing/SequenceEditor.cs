using UnityEngine;
using System;
using System.Collections.Generic;

public class SequenceEditor
{   
    private Sequence _sequence;

    public SequenceEditor(Sequence sequence) {
        _sequence = sequence;
    }


    public NoteValue NoteValueQuantization { get; set; } = NoteValue.Quarter; // when editing, user can move
    // SequenceItems at this level of granularity


    // iterate all sequenceItems and reassign their scheduledPlayTime
    public void SetBPM(float bpm) {

    }

    public IEnumerable<SequenceItem> GetBar(int bar) {
        // calculate where item's relative Play time is relative to the bars, based on
        // time signature and bpm
        // return _sequence.SequenceItems.Where(item => item.Bar == bar);
        return null;
    }

    // public void AddSequenceableAtTime(ISequenceable sequenceable, double time)
    // {
    //     SequenceItem sequenceItem = new SequenceItem
    //     {
    //         Sequenceable = sequenceable,
    //         RelativePlayTime = time
    //     };
    //     _sequence.AddSequenceItem(sequenceItem);
    // }

    // public void AddSequenceableAtBeat(ISequenceable sequenceable, float beat)
    // {
    //     AddSequenceableAtTime(sequenceable, _sequence.Clock.BeatPositionToTime(beat), gain);
    // }

    // public void AddSequenceableAtNoteValue(ISequenceable sequenceable, int bar, NoteValue noteValue, int noteValuePosition, float gain=1.0f)
    // {
    //     // double beatPosition = (bar - 1) * _sequence.TimeSignature.BeatsPerBar + (noteValuePosition / (double)noteValue);
    //     // double time = beatPosition * 60.0 / _sequence.BPM;
    //     double time = _sequence.Clock.TimeFromNotePosition(bar, noteValue, noteValuePosition);
    //     AddSequenceableAtTime(sequenceable, time, gain);
    // }

}