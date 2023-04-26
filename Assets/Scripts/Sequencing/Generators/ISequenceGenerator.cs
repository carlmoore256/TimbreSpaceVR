using UnityEngine;
using System;
using System.Collections.Generic;


public interface ISequenceGenerator
{
    Sequence Generate(); // generates from current list of sequenceables

    void AddSequenceable(ISequenceable sequenceable);
    void RemoveSequenceable(ISequenceable sequenceable);

    void AddSequenceables(IEnumerable<ISequenceable> sequenceables);
    void RemoveSequenceables(IEnumerable<ISequenceable> sequenceables);

    // void SetOrder(IEnumerable<(ISequenceable sequenceable, int order)> sort);

    void SetTime(ISequenceable sequenceable, double time);

    void Clear();
}