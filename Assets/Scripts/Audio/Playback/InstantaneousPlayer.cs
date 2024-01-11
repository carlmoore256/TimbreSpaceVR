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
    private Queue<PlaybackVoice> _availableVoices = new Queue<PlaybackVoice>();
    private SMABuffer _movingAvgScore = new SMABuffer(10);
    Dictionary<Guid, List<DateTime>> _requestLog = new Dictionary<Guid, List<DateTime>>();
    private readonly TimeSpan _requestLogExpiration = TimeSpan.FromSeconds(3); // adjust this time window based on your needs

    
   protected override void InitializePlaybackVoices()
    {
        base.InitializePlaybackVoices();
        foreach(var voice in _playbackVoices)
        {
            _availableVoices.Enqueue(voice);
        }
    }

    protected override void OnPlaybackVoiceReleased(PlaybackVoice voice)
    {
        _availableVoices.Enqueue(voice);
    }

    private struct PlaybackVoiceScore
    {
        public PlaybackVoice PlaybackVoice { get; set; }

        public float Score { get; set; }

    }

    private (float score, PlaybackVoice voice) LowestScoreVoice() {
        PlaybackVoice lowestScoreVoice = _playbackVoices[0];
        float score = Mathf.Infinity;
        foreach(PlaybackVoice v in _playbackVoices) {
            score = v.Score();
            if (score < lowestScoreVoice.Score()) {
                lowestScoreVoice = v;
            }
        }
        return (score, lowestScoreVoice);
    }

    private void ExecuteEvent(float score, WindowedPlaybackEvent e, PlaybackVoice v) {
        _movingAvgScore.Add(score);
        v.Play(e);
        if (!_requestLog.ContainsKey(e.SubmitterId))
            _requestLog[e.SubmitterId] = new List<DateTime>();
        _requestLog[e.SubmitterId].Add(e.CreatedAt);

        for (int i = _requestLog[e.SubmitterId].Count - 1; i >= 0; i--) {
            if (DateTime.Now - _requestLog[e.SubmitterId][i] > _requestLogExpiration) {
                _requestLog[e.SubmitterId].RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Takes in a playback event and queues it to the best PlaybackVoice
    /// </summary>
    public void Play(WindowedPlaybackEvent playbackEvent) {
        PlaybackVoice voice = null;
        _availableVoices.TryDequeue(out voice);
        if (voice != null) {
            // Debug.Log("Taking free voice!");
            ExecuteEvent(0f, playbackEvent, voice);
            return;
        }

        // here we can determine if the playback event has had many repeated plays
        var avgScore = _movingAvgScore.Average();
        if (avgScore > Mathf.Pow(UnityEngine.Random.Range(0.0f, 1.0f), 0.5f)) {
            // Debug.Log($"Randomly not playing event with score {avgScore}");
            return;
        }

        var scoredVoice = LowestScoreVoice();
        // Debug.Log($"Lowest score voice has score {scoredVoice.score}");
        ExecuteEvent(scoredVoice.score, playbackEvent, scoredVoice.voice);
    }
}
