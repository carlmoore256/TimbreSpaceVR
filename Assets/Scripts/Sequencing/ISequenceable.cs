using UnityEngine;
using System;


public class SequenceableParameters
{
    public float Gain { get; set; } = 1f;
    public bool IsMuted { get; set; } = false;
    public float Pitch { get; set; } = 1f;

    public SequenceableParameters Merge(SequenceableParameters parameters) {
        return new SequenceableParameters {
            Gain = Gain * parameters.Gain,
            IsMuted = IsMuted || parameters.IsMuted
        };
    }
}

public class ScheduleCancellationToken
{
    public bool IsCancelled { get; set; } = false;
    public Action OnCancel { get; set; }
    public void Cancel() {
        IsCancelled = true;
        OnCancel?.Invoke();
    }

    public ScheduleCancellationToken() { }
    public ScheduleCancellationToken(Action onCancel) {
        OnCancel = onCancel;
    }
}

public interface ISequenceable 
{
    ScheduleCancellationToken Schedule(double scheduleTime, SequenceableParameters parameters); // RETURN A CANCELLATION TOKEN
    event EventHandler<(double, SequenceableParameters, ScheduleCancellationToken)> OnSchedule;

    // would it be a good idea to add a List<ISequenceable> SubSequences to make it recursive?
    // esentially the subSequences could be scheduled along with the main? Maybe nah

    event Action OnSequenceablePlayStart; // <=============*
    event Action OnSequenceablePlayEnd; // <----------*    |
                                //       bind to event|    |
    void SequenceablePlayStart(); // <=============== | ===*
    void SequenceablePlayEnd(); // <------------------*


    public Guid Id { get; }
}

public interface IPositionedSequenceable : ISequenceable
{
    Vector3 Position { get; }
}
