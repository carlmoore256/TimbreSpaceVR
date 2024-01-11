using UnityEngine;
using System;
using System.Collections.Generic;


public class SequenceFactory
{
    public static Sequence CreateSequence(IEnumerable<ISequenceable> sequenceables, float gain = 1.0f, bool isMuted = false)
    {
        var sequence = new Sequence();
        foreach (var sequenceable in sequenceables)
        {
            sequence.AddSequenceable(sequenceable);
        }
        return sequence;
    }
}