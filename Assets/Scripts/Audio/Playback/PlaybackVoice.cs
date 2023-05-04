using System.Threading;
using System;
using System.Collections.Generic;
using UnityEngine;
using NWaves.Signals;

public class PlaybackVoice {
    public Guid Id { get; set; } = Guid.NewGuid();
    public double ScheduledTime { get; set; } = -1;
    public bool IsPlaying { get; set; } = false;

    private Playhead _playhead;
    private DiscreteSignal _signal;
    private Mutex _playbackEventLock = new Mutex();
    private WindowedPlaybackEvent _playbackEvent;
    private Action<PlaybackVoice> _onVoiceRelease;
    private int _sampleRate;

    public PlaybackVoice(Action<PlaybackVoice> onVoiceRelease=null) {
        _playhead = new Playhead();
        _sampleRate = AudioSettings.outputSampleRate;
        _onVoiceRelease = onVoiceRelease;
    }

    public void Play(WindowedPlaybackEvent playbackEvent, double scheduledTime = -1d) {
        _playbackEventLock.WaitOne();
        
        IsPlaying = true;
        ScheduledTime = scheduledTime;
        
        _playhead.Set(playbackEvent.BufferWindow, _signal);
        _playbackEvent = playbackEvent;

        _playbackEventLock.ReleaseMutex();
    }

    private static float[] CosineWindow(int windowSamples) {
        float[] window = new float[windowSamples];
        for (int i = 0; i < windowSamples; i++) {
            window[i] = (Mathf.Cos(Mathf.PI * 2 * ((float)i / (float)windowSamples)) - 1f) * -0.5f;
        }
        return window;
    }
    
    /// <summary>
    /// Score the current voice on its viability to be stolen
    /// higher score -> more priority for this voice to continue playing
    /// lower score -> less priority, more likely to be stolen
    /// </summary>
    public float Score() {
        return _playhead.Score() * Mathf.Abs(_playbackEvent.RMS);
    }
    
    public void SetSignal(DiscreteSignal signal) {
        _signal = signal;
    }
    
    // initialize variables in ProcessBlock to avoid GC allocs
    private int _destStartIdx = 0;
    private int _destEndIdx = 0; 
    private int _numSamples = 0;
    private float _blockGain = 1f;

    /// <summary>
    /// Audio process block where samples are added to the buffer
    /// </summary>
    public void ProcessBlock(float[] buffer, int channels, float gain) {
        // prevent monobehavior methods from changing playhead during process block
        _playbackEventLock.WaitOne();

        _destStartIdx = 0; // cached for GC

        // Check if the scheduled playback time has been reached within this block
        if (ScheduledTime > 0d && AudioSettings.dspTime < ScheduledTime) {
            // see how long it will be until the start sample
            double timeUntilStart = ScheduledTime - AudioSettings.dspTime;
            _destStartIdx = (int)Mathf.Floor((float)(timeUntilStart * _sampleRate));
            if (_destStartIdx > buffer.Length / channels) {
                // in this case, the sampleStart is further than this block
                // so return without playing
                _playbackEventLock.ReleaseMutex();
                return;
            }
        }

        // calculate the number of samples to play
        _numSamples = (buffer.Length / channels) - _destStartIdx;
        _numSamples = Mathf.Min(_numSamples, _playhead.SamplesRemaining());
        
        if (_numSamples == 0) {
            _playbackEventLock.ReleaseMutex();
            return;
        }

        _destEndIdx = _destStartIdx + _numSamples;
        _blockGain = _playbackEvent.Gain * gain;

        if (!_playhead.HasAdvanced) {
            // trigger onPlayStart events, flip the flag 
            // this is all done manually to optimize GC
            // Debug.Log("ADVANCING PLAYHEAD " + _playhead.Position + " numSamples: " + _numSamples + " NUM SUBSCRIBERS " + _playbackEvent.onPlayStart.GetInvocationList().Length);
            Dispatcher.RunOnMainThread(() => _playbackEvent.onPlayStart?.Invoke());
            _playhead.HasAdvanced = true;
        }

        // add the result to the buffer (optimized to check useWindow once, check if it's worth it)
        if (_playbackEvent.Window != null) {
            for (int i = _destStartIdx; i < _destEndIdx; i++) {
                float sample = _signal.Samples[_playhead.Position] * _playbackEvent.Window[_playhead.WindowIndex()];
                for (int c = 0; c < channels; c++) {
                    buffer[i * channels + c] += sample * _blockGain;
                }
                _playhead.Position++;
            }
        } else {
            for (int i = _destStartIdx; i < _destEndIdx; i++) {
                float sample = _signal.Samples[_playhead.Position];
                for (int c = 0; c < channels; c++) {
                    buffer[i * channels + c] += sample * _blockGain;
                }
                _playhead.Position++;
            }
        }

        // check to see if this voice has finished playing
        if (_playhead.IsFinished() || !IsPlaying) { // never ~should~ be asked to play if !IsPlaying
            // trigger onPlayEnd events and onVoiceFree (do before releasing)
            Dispatcher.RunOnMainThread(() => _playbackEvent.onPlayEnd?.Invoke());
            Dispatcher.RunOnMainThread(() => _onVoiceRelease?.Invoke(this));
            IsPlaying = false;
        }
        
        _playbackEventLock.ReleaseMutex();
    }
}