using System.Threading;
using System;
using System.Collections.Generic;
using UnityEngine;
using NWaves.Signals;

public class PlaybackVoice {
    public string ID { get { return _ID; } }
    private string _ID = Guid.NewGuid().ToString();
    private DiscreteSignal _signal;
    private Playhead playhead;
    private Mutex mut = new Mutex();
    private bool isProcessing = false;
    // private float[] window;

    public double ScheduledTime { get; set; } = -1;
    // private double scheduledEndTime;
    
    private WindowedPlaybackEvent playbackEvent;
    private Action<PlaybackVoice> onVoiceFree;
    public bool IsPlaying { get; set; } = false;
    private int sampleRate = AudioSettings.outputSampleRate;

    public PlaybackVoice(Action<PlaybackVoice> onVoiceFree=null) {
        playhead = new Playhead();
        this.onVoiceFree = onVoiceFree;
    }

    public void Play(WindowedPlaybackEvent playbackEvent, double scheduledTime = -1d) {
        mut.WaitOne();
        IsPlaying = true;

        this.playhead.Set(playbackEvent.bufferWindow, _signal);
        this.playbackEvent = playbackEvent;
        this.ScheduledTime = scheduledTime;

        mut.ReleaseMutex();
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
        return playhead.Score() * Mathf.Abs(playbackEvent.rms);
    }
    
    public void SetSignal(DiscreteSignal signal) {
        _signal = signal;
    }
    

    // initialize variables in ProcessBlock to avoid GC allocs
    private int destStartIdx = 0;
    private int destEndIdx = 0; 
    private int numSamples = 0;
    private float blockGain = 1f;

    /// <summary>
    /// Audio process block where samples are added to the buffer
    /// </summary>
    public void ProcessBlock(float[] buffer, int channels, float gain) {
        // prevent monobehavior methods from changing playhead during process block
        mut.WaitOne();

        destStartIdx = 0; // cached for GC

        // Check if the scheduled playback time has been reached within this block
        if (ScheduledTime > 0d && AudioSettings.dspTime < ScheduledTime) {
            // see how long it will be until the start sample
            double timeUntilStart = ScheduledTime - AudioSettings.dspTime;
            destStartIdx = (int)Mathf.Floor((float)(timeUntilStart * sampleRate));
            if (destStartIdx > buffer.Length / channels) {
                // in this case, the sampleStart is further than this block
                // so return without playing
                mut.ReleaseMutex();
                return;
            }
        }

        // calculate the number of samples to play
        numSamples = (buffer.Length / channels) - destStartIdx;
        numSamples = Mathf.Min(numSamples, playhead.SamplesRemaining());
        
        if (numSamples == 0) {
            mut.ReleaseMutex();
            return;
        }

        destEndIdx = destStartIdx + numSamples;
        blockGain = playbackEvent.gain * gain;

        if (!playhead.hasAdvanced) {
            // trigger onPlayStart events, flip the flag 
            // this is all done manually to optimize GC
            Dispatcher.RunOnMainThread(() => playbackEvent.onPlayStart?.Invoke());
            playhead.hasAdvanced = true;
        }

        // add the result to the buffer (optimized to check useWindow once, check if it's worth it)
        if (playbackEvent.window != null) {
            for (int i = destStartIdx; i < destEndIdx; i++) {
                float sample = _signal.Samples[playhead.position] * playbackEvent.window[playhead.WindowIndex()];
                for (int c = 0; c < channels; c++) {
                    buffer[i * channels + c] += sample * blockGain;
                }
                playhead.position++;
            }
        } else {
            for (int i = destStartIdx; i < destEndIdx; i++) {
                float sample = _signal.Samples[playhead.position];
                for (int c = 0; c < channels; c++) {
                    buffer[i * channels + c] += sample * blockGain;
                }
                playhead.position++;
            }
        }

        // check to see if this voice has finished playing
        if (playhead.IsFinished() || !IsPlaying) { // never ~should~ be asked to play if !IsPlaying
            // trigger onPlayEnd events and onVoiceFree (do before releasing)
            Dispatcher.RunOnMainThread(() => playbackEvent.onPlayEnd?.Invoke());
            Dispatcher.RunOnMainThread(() => onVoiceFree?.Invoke(this));
            IsPlaying = false;
        }
        
        mut.ReleaseMutex();
    }
}