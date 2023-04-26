using UnityEngine;
using System;
using NWaves.Windows;

public class PlaybackEvent
{
    public Guid Id = Guid.NewGuid();
    public Guid SubmitterId { get; set; }
    public float Gain { get; set; }
    public ScheduleCancellationToken CancellationToken { get; set; } = new ScheduleCancellationToken();

    public Action onPlayStart = null;
    public Action onPlayEnd = null;

    public PlaybackEvent() {}

    public PlaybackEvent(float gain, Guid submitterID)
    {
        this.Gain = gain;
        this.SubmitterId = submitterID;
    }

    public PlaybackEvent(float gain, Guid submitterID, ScheduleCancellationToken cancellationToken) : this(gain, submitterID)
    {
        if (cancellationToken != null)
            this.CancellationToken = cancellationToken;
    }
    

}

/// a playbackEvent may or may not relate to an ISequenceable, 
/// that is why they aren't coupled, but also have their own onPlayStart and onPlayEnd events
/// maybe should be renamed to WindowedPlaybackEvent, and inherit from a generic playback event
public class WindowedPlaybackEvent : PlaybackEvent
{
    public float RMS { get; set; } = 1f;
    public WindowTime BufferWindow { get; set; }
    public float[] Window { get; set; }
    public DateTime CreatedAt { get; set; }

    public WindowedPlaybackEvent() : base() {}

    public WindowedPlaybackEvent(
            WindowTime bufferWindow,
            WindowTypes windowType,
            float gain,
            Guid submitterId,
            Action onPlayStart = null,
            Action onPlayEnd = null,
            ScheduleCancellationToken cancellationToken=null) : base(gain, submitterId, cancellationToken) {

        Initialize(bufferWindow, windowType, gain, submitterId);
    }

    public void Initialize(
            WindowTime bufferWindow,
            WindowTypes windowType,
            float gain,
            Guid submitterID) {

        this.BufferWindow = bufferWindow;

        // don't bother allocating new memory for rect window
        if (windowType == WindowTypes.Rectangular) {
            this.Window = null;
        } else {
            this.Window = Windowing.Instance.GetWindow(windowType, bufferWindow.numSamples); // retrieves a cached version
        }

        this.Gain = gain;
        this.SubmitterId = submitterID;
        this.CreatedAt = DateTime.Now;

        // Set these to null, and subscribe/unsubscribe later
        onPlayStart = null;
        onPlayEnd = null;
    }

    public bool IsExpired(float expirationTime=5f)
    {
        return DateTime.Now.Subtract(this.CreatedAt).TotalSeconds > expirationTime;
    }

    public bool IsExpired(DateTime expirationTime)
    {
        return DateTime.Now.Subtract(this.CreatedAt).TotalSeconds > expirationTime.Subtract(this.CreatedAt).TotalSeconds;
    }

    public void RegisterSequenceableEvents(ISequenceable sequenceable)
    {
        onPlayStart += sequenceable.SequenceablePlayStart;
        onPlayEnd += sequenceable.SequenceablePlayEnd;
    }
}
















































// public PlaybackEvent(float gain, int submitterID=0) {
//     this.gain = gain;
//     this.submitterID = submitterID;
//     this.createdAt = DateTime.Now;
// }


// public class InstantPlaybackEvent : PlaybackEvent {
//     public InstantPlaybackEvent() : base() {}
//     public DateTime createdAt;

//     public InstantPlaybackEvent(float gain, int submitterID) : base(gain, submitterID) 
//     {
//         this.createdAt = DateTime.Now;
//     }

//     public void Set(float gain, WindowTime windowTime, float rms=1f, int submitterID=0) 
//     {
//         this.gain = gain;
//         this.windowTime = windowTime;
//         this.submitterID = submitterID;
//         this.createdAt = DateTime.Now;
//     }
// }

// public class ScheduledPlaybackEvent : PlaybackEvent {
//     public double scheduledTime;

//     public ScheduledPlaybackEvent() : base() {}

//     public ScheduledPlaybackEvent(float gain, int submitterID, double scheduledTime, WindowTime windowTime) : base(gain, submitterID) 
//     {
//         this.scheduledTime = scheduledTime;
//         this.windowTime = windowTime;
//     }
// }