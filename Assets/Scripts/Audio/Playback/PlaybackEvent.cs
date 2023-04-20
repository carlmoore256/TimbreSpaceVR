using UnityEngine;
using System;
using NWaves.Windows;

public class PlaybackEvent
{
    public string ID = Guid.NewGuid().ToString();
    public int submitterID;
    public float gain;

    public Action onPlayStart = null;
    public Action onPlayEnd = null;

    public PlaybackEvent() {}

    public PlaybackEvent(float gain, int submitterID)
    {
        this.gain = gain;
        this.submitterID = submitterID;
    }
}

/// a playbackEvent may or may not relate to an ISequenceable, 
/// that is why they aren't coupled, but also have their own onPlayStart and onPlayEnd events
/// maybe should be renamed to WindowedPlaybackEvent, and inherit from a generic playback event
public class WindowedPlaybackEvent : PlaybackEvent
{
    public float rms = 1f;
    public WindowTime bufferWindow;
    public float[] window;
    public DateTime createdAt;

    public WindowedPlaybackEvent() : base() {}

    public WindowedPlaybackEvent(
            WindowTime bufferWindow,
            WindowTypes windowType,
            float gain,
            int submitterID,
            Action onPlayStart = null,
            Action onPlayEnd = null) : base(gain, submitterID) {

        Initialize(bufferWindow, windowType, gain, submitterID);
    }

    public void Initialize(
            WindowTime bufferWindow,
            WindowTypes windowType,
            float gain,
            int submitterID) {

        this.bufferWindow = bufferWindow;

        // don't bother allocating new memory for rect window
        if (windowType == WindowTypes.Rectangular) {
            this.window = null;
        } else {
            this.window = Windowing.Instance.GetWindow(windowType, bufferWindow.numSamples); // retrieves a cached version
        }

        this.gain = gain;
        this.submitterID = submitterID;
        this.createdAt = DateTime.Now;

        // Set these to null, and subscribe/unsubscribe later
        onPlayStart = null;
        onPlayEnd = null;
    }

    public bool IsExpired(float expirationTime=5f)
    {
        return DateTime.Now.Subtract(this.createdAt).TotalSeconds > expirationTime;
    }

    public bool IsExpired(DateTime expirationTime)
    {
        return DateTime.Now.Subtract(this.createdAt).TotalSeconds > expirationTime.Subtract(this.createdAt).TotalSeconds;
    }

    public void RegisterSequenceableEvents(ISequenceable sequenceable)
    {
        onPlayStart += sequenceable.SequenceablePlayStart;
        onPlayEnd += sequenceable.SequenceablePlayEnd;
    }

    // public void Set(float gain, WindowTime windowTime, float rms=1f, int submitterID=0) 
    // {
    //     this.gain = gain;
    //     this.bufferWindow = windowTime;
    //     this.submitterID = submitterID;
    //     this.createdAt = DateTime.Now;
    // }
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