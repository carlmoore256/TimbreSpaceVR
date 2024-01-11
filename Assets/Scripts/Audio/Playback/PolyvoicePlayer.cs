using System.Collections.Generic;
using UnityEngine;
using NWaves.Signals;
using System.Threading;
using System;


public abstract class PolyvoicePlayer : MonoBehaviour, IAudioSource
{
    public float Gain { get; set; }
    public AudioSource AudioSource { get; set; }

    public int NumVoices { get; protected set; } = 256;


    protected PlaybackVoice[] _playbackVoices;
    protected bool _isEnabled = false;


    public virtual void OnEnable()
    {
        _isEnabled = true;
        AudioSource = gameObject.GetComponent<AudioSource>();
        
        // eventually change this, so that it doesn't connect itself but is connected
        // by something else
        TsvrApplication.AudioManager.ConnectGrainModelAudioSource(AudioSource);
        InitializePlaybackVoices();
    }

    public virtual void OnDisable()
    {
        _isEnabled = false;
    }

    public void Mute()
    {
        _isEnabled = false;
        AudioSource.mute = true;
    }

    public void UnMute()
    {
        _isEnabled = true;
        AudioSource.mute = false;
    }

    protected virtual void InitializePlaybackVoices() 
    {
        _playbackVoices = new PlaybackVoice[NumVoices];
        for (int i = 0; i < NumVoices; i++)
        {
            _playbackVoices[i] = new PlaybackVoice(OnPlaybackVoiceReleased);
        }
    }

    protected virtual void OnPlaybackVoiceReleased(PlaybackVoice voice)
    {
        // UnityEngine.Debug.Log("Playback voice released");
    }

    public void SetAudioBuffer(DiscreteSignal signal)
    {
        foreach (PlaybackVoice voice in _playbackVoices)
        {
            voice.SetSignal(signal);
        }
        AudioSource.Play();
    }

    protected virtual void OnAudioFilterRead(float[] data, int channels)
    {

        if (!_isEnabled) { return; }

        float gain = 0.4f;
        for (int i = 0; i < _playbackVoices.Length; i++)
        {
            if (_playbackVoices[i].IsPlaying)
            {
                _playbackVoices[i].ProcessBlock(data, channels, gain);
            }
        }
    }
}
