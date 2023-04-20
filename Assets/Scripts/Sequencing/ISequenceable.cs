using UnityEngine;
using System;

public class SequenceableScheduleParameters {
    public double scheduleTime;
    public float gain;
}

public interface ISequenceable {
    // void Schedule(double scheduleTime, float gain, Action onPlayStart, Action onPlayEnd);
    void Schedule(SequenceableScheduleParameters parameters);
    event EventHandler<SequenceableScheduleParameters> OnSchedule;

    event Action OnSequenceablePlayStart; // <=============*
    event Action OnSequenceablePlayEnd; // <----------*    |
                                //       bind to event|    |
    void SequenceablePlayStart(); // <=============== | ===*
    void SequenceablePlayEnd(); // <------------------*
    public int ID { get; }
}

public interface IPositionedSequenceable : ISequenceable
{
    Vector3 Position { get; }
}