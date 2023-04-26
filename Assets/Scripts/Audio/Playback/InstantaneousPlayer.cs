using System.Collections.Generic;
using UnityEngine;
using NWaves.Signals;
using System.Threading;
using System;

/// <summary>
/// Polyphonic playback engine that manages a group of PlaybackVoices to facilitate
/// incoming PlaybackEvents, determining priority among voices and choosing the most ideal
/// </summary>
public class InstantaneousPlayer : PolyvoicePlayer
{
    private Queue<PlaybackVoice> availableVoices = new Queue<PlaybackVoice>();
    private SMABuffer smaScore = new SMABuffer(10);
    Dictionary<Guid, List<DateTime>> requestCounts = new Dictionary<Guid, List<DateTime>>();
    private readonly TimeSpan timeWindow = TimeSpan.FromSeconds(3); // adjust this time window based on your needs

    
    void OnEnable()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        TsvrApplication.AudioManager.ConnectGrainModelAudioSource(audioSource);
        playbackVoices = new PlaybackVoice[NumVoices];

        
        for(int i = 0; i < NumVoices; i++) {
            playbackVoices[i] = new PlaybackVoice(voice => {
                // Debug.Log("Voice has become available " + voice.GetHashCode());
                availableVoices.Enqueue(voice);
            });
            availableVoices.Enqueue(playbackVoices[i]);
        }
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

    private void ExecuteEvent(float score, WindowedPlaybackEvent e, PlaybackVoice v) {
        smaScore.Add(score);
        v.Play(e);
        if (!requestCounts.ContainsKey(e.SubmitterId))
            requestCounts[e.SubmitterId] = new List<DateTime>();
        requestCounts[e.SubmitterId].Add(e.CreatedAt);

        for (int i = requestCounts[e.SubmitterId].Count - 1; i >= 0; i--) {
            if (DateTime.Now - requestCounts[e.SubmitterId][i] > timeWindow) {
                requestCounts[e.SubmitterId].RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Takes in a playback event and queues it to the best PlaybackVoice
    /// </summary>
    public void Play(WindowedPlaybackEvent playbackEvent) {
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
