using System.Threading;
using System;
using System.Collections.Generic;
using UnityEngine;
using NWaves.Signals;

public class Playhead {
    public int startSample;
    public int endSample;
    public int windowSamples;
    public int position;
    public int startPad;
    public PlaybackEvent playbackEvent = null;

    
    public Playhead(int startSample, int endSample) {
        this.startSample = startSample;
        this.endSample = endSample;
        this.windowSamples = endSample - startSample;
        this.position = startSample;
        this.startPad = 0;
    }

    public Playhead() {
        this.startSample = 0;
        this.endSample = 0;
        this.windowSamples = 0;
        this.position = 0;
        this.startPad = 0;
    }

    public void SetPlaybackEvent(PlaybackEvent playbackEvent, DiscreteSignal signal, int startPad=0) {
        this.startSample = (int)Mathf.Floor((float)(playbackEvent.windowTime.startTime * signal.SamplingRate));
        this.endSample = (int)Mathf.Floor((float)(playbackEvent.windowTime.endTime * signal.SamplingRate));
        this.endSample = Mathf.Min(endSample, signal.Length);
        this.position = startSample;
        this.windowSamples = endSample - startSample;
        this.playbackEvent = playbackEvent;
        this.startPad = UnityEngine.Random.Range(0, this.windowSamples); // <- REMOVE ME
        // this.startPad = startPad;
    }

    public float Score() {
        // float score = endSample - playhead;
        float score = endSample - position;
        if (score == 0) return 0f;
        score = score / (float)windowSamples;
        return score * Mathf.Abs(playbackEvent.rms);
    }

    public int WindowIndex() {
        return position - startSample;
    }

    public int SamplesRemaining() {
        return endSample - position;
    }

    public bool IsFinished() {
        return position >= endSample;
    }

    public void DebugMessage() {
        Debug.Log($"PLAYHEAD: {position} | {startSample} -> {endSample} | Length: {windowSamples} | Current Index: {WindowIndex()}");
    }
}

public class PlaybackVoice {
    private DiscreteSignal _signal;
    private Playhead playhead;
    private Mutex mut = new Mutex();
    private bool isProcessing = false;
    private float[] window;
    private double scheduledEndTime;
    private PlaybackEvent playbackEvent;
    private Action<PlaybackVoice, PlaybackEvent> onPlaybackFinished;
    public bool IsPlaying { get; set; } = false;

    public PlaybackVoice(Action<PlaybackVoice, PlaybackEvent> _onPlaybackFinished) {
        window = new float[0];
        playhead = new Playhead();
        onPlaybackFinished = _onPlaybackFinished;
    }

    public void Play(PlaybackEvent playbackEvent) {
        mut.WaitOne();       
        IsPlaying = true;
        this.playhead.SetPlaybackEvent(playbackEvent, _signal);
        this.playbackEvent = playbackEvent;

        if (window.Length != this.playhead.windowSamples) {
            window = CosineWindow(this.playhead.windowSamples);
        }

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
        return playhead.Score();
    }
    
    public void SetSignal(DiscreteSignal signal) {
        _signal = signal;
    }

    public void ProcessBlock(float[] buffer, int channels, float gain) {
        mut.WaitOne(); // prevent monobehavior methods from changing playhead during process block
        if (playhead.IsFinished()) {
            IsPlaying = false;
            mut.ReleaseMutex();
            onPlaybackFinished?.Invoke(this, playbackEvent);
            return;
        }

        int numSamples = buffer.Length / channels;
        numSamples = Mathf.Min(numSamples, playhead.SamplesRemaining());

        for (int i = 0; i < numSamples; i++) {
            float sample = _signal.Samples[playhead.position] * window[playhead.WindowIndex()];
            for (int c = 0; c < channels; c++) {
                buffer[i * channels + c] += sample * playbackEvent.gain * gain;
            }

            playhead.position++;
        }
        
        mut.ReleaseMutex();
    }

}
