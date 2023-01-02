using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NWaves.Signals;
using System;

public class SequencedPlayback {
    public double playTime;
    public double endTime;
    public SequencedPlayback(double scheduledPlayTime, double scheduledEndTime) {
        this.playTime = scheduledPlayTime;
        this.endTime = scheduledEndTime;
    }
}

public struct PlaybackEvent {
    public float gain;
    public float rms;
    public double createdAt;
    public WindowTime windowTime;
    public SequencedPlayback sequencedPlayback;
    public PlaybackEvent(float gain, WindowTime windowTime, float rms=0f, SequencedPlayback sequencedPlayback=null) {
        this.gain = gain;
        this.windowTime = windowTime;
        this.rms = rms;
        this.sequencedPlayback = sequencedPlayback;
        this.createdAt = AudioSettings.dspTime;
    }
}

public class PlaybackVoice {

    public AudioSource audioSource;

    private AudioClip _clip;
    public AudioClip Clip { 
        get { return _clip; }
        set {
            _clip = value; 
            audioSource.clip = _clip;
        } 
    }

    private Coroutine playbackCoroutine;
    private double scheduledEndTime;
    private PlaybackEvent playbackEvent;

    public PlaybackVoice(AudioSource audioSource, AudioClip clip) {
        this.audioSource = audioSource;
        this.Clip = clip;
    }

    public void Play(PlaybackEvent playbackEvent) {
        this.playbackEvent = playbackEvent;
        scheduledEndTime = AudioSettings.dspTime+(playbackEvent.windowTime.duration);
        audioSource.time = (float)playbackEvent.windowTime.startTime;
        audioSource.volume = playbackEvent.gain;
        if (!audioSource.isPlaying) 
            audioSource.PlayScheduled(AudioSettings.dspTime);
            // audioSource.Play();
        audioSource.SetScheduledEndTime(scheduledEndTime);
    }

    /// higher score = more priority for this voice to continue playing
    public float Score() {
        float playTimeRemaining = (float)(scheduledEndTime - AudioSettings.dspTime);
        if (playTimeRemaining < 0) return 0f;
        return  (playTimeRemaining /  (float)playbackEvent.windowTime.duration) * Mathf.Abs(playbackEvent.rms); 
    }

    // call from a monobehaviour update, which will update the fade of the audio clip accordingly
    public void FadeUpdate() {
        // lerp through a cosine function to winow the audio at the current playhead position
        if (audioSource.isPlaying) {
            float playTimeRemaining = (float)(scheduledEndTime - AudioSettings.dspTime);
            // get the gain at cosine window position
            float gain = (Mathf.Cos(Mathf.PI * 2 * (playTimeRemaining / (float)playbackEvent.windowTime.duration)) - 1f) * -0.5f;
            audioSource.volume = gain;
        }
    }
}

/// <summary>
/// Main audio manager for the grain model, handling audio playback with multiple voices
/// </summary>
public class GrainModelPlayback : MonoBehaviour
{
    public int NumVoices { get; private set; } = 16;
    private PlaybackVoice[] playbackVoices;
    
    void OnEnable()
    {
        playbackVoices = new PlaybackVoice[NumVoices];
        for(int i = 0; i < NumVoices; i++) {
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            TsvrApplication.AudioManager.ConnectGrainModelAudioSource(audioSource);
            playbackVoices[i] = new PlaybackVoice(audioSource, null);
        }
    }

    public void SetAudioBuffer(DiscreteSignal signal) {
        AudioClip clip = AudioClip.Create("Grain", signal.Length, 1, signal.SamplingRate, false);
        clip.SetData(signal.Samples, 0);
        foreach(PlaybackVoice voice in playbackVoices) {
            voice.Clip = clip;
        }
    }

    void Update() {
        foreach(PlaybackVoice voice in playbackVoices) {
            voice.FadeUpdate();
        }
    }

    /// <summary>
    /// Takes in a playback event and queues it to the proper PlaybackVoice
    /// </summary>
    public void RegisterPlaybackEvent(PlaybackEvent playbackEvent) {
        if (playbackEvent.sequencedPlayback != null) {
            // Debug.Log($"Registering Sequenced Playback {playbackEvent.sequencedPlayback.playTime} {playbackEvent.sequencedPlayback.endTime}");
            // call function which will handle sequential playback (a separate bank of voices)
            // StartCoroutine(PlaySequenced(playbackEvent));
            return;
        } 
        // find the voice with the lowest score
        PlaybackVoice lowestScoreVoice = playbackVoices[0];
        float score = 1f;
        foreach(PlaybackVoice voice in playbackVoices) {
            score = voice.Score();
            if (score == 0f) {
                voice.Play(playbackEvent);
                return;
            } 
            if (score < lowestScoreVoice.Score()) {
                lowestScoreVoice = voice;
            }
        }
        Debug.Log($"Stealing Voice with score {score}");
        lowestScoreVoice.Play(playbackEvent);
    }
}