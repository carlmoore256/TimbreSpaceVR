using UnityEngine;
using System;
using UnityEngine.UI;

public class SequenceableParameters
{
    public float Gain { get; set; } = 1f;
    public bool IsMuted { get; set; } = false;
    public float Pitch { get; set; } = 1f;
    public Color Color { get; set; }

    public SequenceableParameters Merge(SequenceableParameters parameters) {
        return new SequenceableParameters {
            Gain = Gain * parameters.Gain,
            IsMuted = IsMuted || parameters.IsMuted,
            Pitch = parameters.Pitch,
            Color = parameters.Color
        };
    }
}


public interface ISequenceable 
{
    Guid Id { get; }

    ScheduleCancellationToken Schedule(double scheduleTime, SequenceableParameters parameters); // RETURN A CANCELLATION TOKEN
    event EventHandler<(double, SequenceableParameters, ScheduleCancellationToken)> OnSchedule;

    // event Action OnSequenceablePlayStart; // <=============*
    // event Action OnSequenceablePlayEnd; // <----------*    |
                                //       bind to event|    |
    void SequenceablePlayStart(); // <=============== | ===*
    void SequenceablePlayEnd(); // <------------------*
}

public interface IInteractableSequenceable : ISequenceable
{
    Vector3 Position { get; }
}
