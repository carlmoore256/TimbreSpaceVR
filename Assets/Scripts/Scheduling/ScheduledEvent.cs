using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

// add a DSP Sequencer/Scheduler or DSP Dispatcher or something like that,
// which is a central dispatcher for any events that need to occur based
// on the DSP time. This can run in an OnAudioFilterRead() thread,
// and should track the heavyness of the load of the events. If some
// are taking too long, it should be able to prioritize jobs. This
// can be implemented with the IScheduable interface, that makes scheduling
// any event for audio time an easy process with accessable API to the program
// this allows us to cancel events, save the state of all upcoming events

public enum ScheduablePriority
{
    Low,
    Medium,
    High
}

public class ScheduledEvent
{

    public Guid Id { get; set; } = Guid.NewGuid();
    // if we want to have an audio event occur, we can have
    // an IScheduable wrap around another ISchedulable. The first fires
    // an audio block before so that the block dispatcher can send off
    // the wrapped Scheduable to the audio sequencer, which has better
    // ability to have things occur at sample level
    public event Action OnSchedule;

    public bool IsCancelled { get => CancellationToken.IsCancelled; }
    private double _scheduledTime;

    public double ScheduleTime => _scheduledTime;

    public ScheduleCancellationToken CancellationToken { get; private set; }

    public ScheduledEvent(double scheduleTime)
    {
        Schedule(scheduleTime);
    }

    public ScheduledEvent(double scheduleTime, Action onSchedule) : this(scheduleTime)
    {
        OnSchedule = onSchedule;
    }

    public ScheduledEvent(double scheduleTime, Action onSchedule, ScheduleCancellationToken cancellationToken) : this(scheduleTime, onSchedule)
    {
        CancellationToken = cancellationToken;
    }

    public static ScheduledEvent CreateAndSchedule(double scheduleTime, Action onSchedule)
    {
        var scheduledEvent = new ScheduledEvent(scheduleTime, onSchedule);
        scheduledEvent.Schedule();
        return scheduledEvent;
    }

    public static ScheduledEvent CreateAndSchedule(double scheduleTime, Action onSchedule, ScheduleCancellationToken cancellationToken)
    {
        var scheduledEvent = new ScheduledEvent(scheduleTime, onSchedule, cancellationToken);
        scheduledEvent.Schedule();
        return scheduledEvent;
    }

    public void Schedule(double dspTime)
    {
        _scheduledTime = dspTime;
    }
    public void Cancel()
    {
        // IsCancelled = true;
        CancellationToken.Cancel();
    }

    public bool OccursBetween(double startTime, double endTime)
    {
        if (!IsCancelled)
            return _scheduledTime >= startTime && _scheduledTime <= endTime;
        return false;
    }

    public bool OccursBefore(double time)
    {
        if (!IsCancelled)
            return _scheduledTime < time;
        return false;
    }

    public bool OccursAfter(double time)
    {
        if (!IsCancelled)
            return _scheduledTime > time;
        return false;
    }

    // public bool OccursBefore(DateTime time) {
    //     if (IsScheduled)
    //         return _scheduledTime < time;
    //     return false;
    // }

    // maybe add some methods to dispatch these async
    public void Invoke()
    {
        OnSchedule?.Invoke();
    }


    public void Schedule()
    {
        DSPSchedulerSingleton.Schedule(this);
    }
}

