using System.Collections.Generic;
using UnityEngine;
using NWaves.Signals;
using System.Threading;
using System;


public interface IPlaybackHandler {
    public void Play(PlaybackEvent playbackEvent);
}

/// <summary>
/// Polyphonic playback engine that manages a group of PlaybackVoices to facilitate
/// incoming PlaybackEvents, determining priority among voices and choosing the most ideal
/// </summary>
public class PolyvoicePlayer : MonoBehaviour
{
    public int NumVoices { get; private set; } = 256;
    private PlaybackVoice[] playbackVoices;
    private Queue<PlaybackVoice> availableVoices = new Queue<PlaybackVoice>();
    private double lastPlayTime = 0d;
    private double delayThresh = 0d;

    private AudioSource audioSource;
    private SMABuffer smaScore = new SMABuffer(10);
    
    void OnEnable()
    {
        playbackVoices = new PlaybackVoice[NumVoices];
        audioSource = gameObject.GetComponent<AudioSource>();

        TsvrApplication.AudioManager.ConnectGrainModelAudioSource(audioSource);
        
        for(int i = 0; i < NumVoices; i++) {
            playbackVoices[i] = new PlaybackVoice((voice, playbackEvent) => {
                // Debug.Log("Voice has become available " + voice.GetHashCode());
                availableVoices.Enqueue(voice);
                playbackEvent.onComplete?.Invoke();
            });
            availableVoices.Enqueue(playbackVoices[i]);
        }
    }

    public void SetAudioBuffer(DiscreteSignal signal) {

        foreach(PlaybackVoice voice in playbackVoices) {
            voice.SetSignal(signal);
        }

        audioSource.Play();
    }

    private void OnAudioFilterRead(float[] data, int channels) {
        // float gain = 1f/NumVoices;
        float gain = 0.1f;
        // don't even hop into the playback voices that aren't active
        for(int i = 0; i < playbackVoices.Length; i++) {
            if (playbackVoices[i].IsPlaying)
                playbackVoices[i].ProcessBlock(data, channels, gain);
        }
    }

    private Queue<PlaybackEvent> playbackEventQueue = new Queue<PlaybackEvent>();
    private int maxQueueSize = 1000;

    private float QueueAverageScore() {
        float score = 0f;
        foreach(PlaybackEvent e in playbackEventQueue) {
            score += e.score;
        }
        score /= playbackVoices.Length;
        return score;
    }

    private (float score, PlaybackVoice voice) LowestScoreVoice() {
        PlaybackVoice lowestScoreVoice = playbackVoices[0];
        float score = Mathf.Infinity;
        foreach(PlaybackVoice v in playbackVoices) {
            score = v.Score();
            if (score < lowestScoreVoice.Score()) {
                lowestScoreVoice = v;
            }
        }
        return (score, lowestScoreVoice);
    }

    Dictionary<int, List<DateTime>> requestCounts = new Dictionary<int, List<DateTime>>();
    private readonly TimeSpan timeWindow = TimeSpan.FromSeconds(3); // adjust this time window based on your needs


    private void ExecuteEvent(float score, PlaybackEvent e, PlaybackVoice v) {
        e.score = score;
        smaScore.Add(score);
        // playbackEventQueue.Enqueue(e);
        // if (playbackEventQueue.Count > maxQueueSize) {
        //     playbackEventQueue.Dequeue();
        // }
        v.Play(e);
        if (!requestCounts.ContainsKey(e.submitterID))
            requestCounts[e.submitterID] = new List<DateTime>();
        requestCounts[e.submitterID].Add(e.createdAt);

        for (int i = requestCounts[e.submitterID].Count - 1; i >= 0; i--) {
            if (DateTime.Now - requestCounts[e.submitterID][i] > timeWindow) {
                requestCounts[e.submitterID].RemoveAt(i);
            }
        }
    }



    /// <summary>
    /// Takes in a playback event and queues it to the best PlaybackVoice
    /// </summary>
    public void Play(PlaybackEvent playbackEvent) {
        PlaybackVoice voice = null;
        availableVoices.TryDequeue(out voice);
        if (voice != null) {
            // Debug.Log("Taking free voice!");
            ExecuteEvent(0f, playbackEvent, voice);
            return;
        }

        // here we can determine if the playback event has had many repeated plays
        var avgScore = smaScore.Average();
        if (avgScore > Mathf.Pow(UnityEngine.Random.Range(0.0f, 1.0f), 0.5f)) {
            // Debug.Log($"Randomly not playing event with score {avgScore}");
            return;
        }

        var scoredVoice = LowestScoreVoice();
        // Debug.Log($"Lowest score voice has score {scoredVoice.score}");
        ExecuteEvent(scoredVoice.score, playbackEvent, scoredVoice.voice);
    }
}