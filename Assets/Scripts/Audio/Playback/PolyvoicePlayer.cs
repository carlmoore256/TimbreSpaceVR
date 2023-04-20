using System.Collections.Generic;
using UnityEngine;
using NWaves.Signals;
using System.Threading;
using System;

public class PolyvoicePlayer : MonoBehaviour {
    public int NumVoices { get; protected set; } = 256;
    protected PlaybackVoice[] playbackVoices;
    protected AudioSource audioSource;

    public void SetAudioBuffer(DiscreteSignal signal) {
        Debug.Log("Setting audio buffer");
        foreach(PlaybackVoice voice in playbackVoices) {
            voice.SetSignal(signal);
        }
        audioSource.Play();
    }

}
