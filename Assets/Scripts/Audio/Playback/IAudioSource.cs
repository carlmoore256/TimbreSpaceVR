using System.Collections.Generic;
using UnityEngine;
using NWaves.Signals;
using System.Threading;
using System;

/// <summary>
/// A general interface for an audio signal generator
/// For instance, GrainCloud will have an instance of an IAudioSource that will be the BufferPlayback thingy
/// </summary>
public interface IAudioSource
{
    AudioSource AudioSource { get; set; }

    float Gain { get; set; }

    void Mute();
    void UnMute();

}