using System.Collections.Generic;
using UnityEngine;
using NWaves.Signals;
using System.Threading;
using System;

/// <summary>
/// Main audio manager for the grain model, handling audio playback with multiple voices
/// </summary>
public class GrainModelPlayback : MonoBehaviour
{
    public int NumVoices { get; private set; } = 64;
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

    // private float CalculateReplayPenalty(PlaybackEvent e) {
    //     if (!requestCounts.ContainsKey(e.submitterID)) {
    //         return 0f;
    //     }
    //     // var now = DateTime.Now;
    //     var eventTime = e.createdAt;
    //     var count = 0;
    //     foreach(var t in requestCounts[e.submitterID]) {
    //         if (e.createdAt - t < timeWindow) {
    //             count++;
    //         }
    //     }
    //     if (count > 10) {
    //         Debug.Log($"Too many requests from {e.submitterID} in the last {timeWindow.TotalSeconds} seconds");
    //         return 1f;
    //     }
    // }



    /// <summary>
    /// Takes in a playback event and queues it to the best PlaybackVoice
    /// </summary>
    public void RegisterPlaybackEvent(PlaybackEvent playbackEvent) {
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






// foreach(PlaybackVoice voice in playbackVoices) {
//     score = voice.Score();
//     if (score == 0f) {
//         lowestScoreVoice = voice;
//         Debug.Log("Score is 0, playing immediately! " + playbackEvent.submitterID);
//         ExecuteEvent(score, playbackEvent, voice);
//         return;
//     } 
//     if (score < lowestScoreVoice.Score()) {
//         lowestScoreVoice = voice;
//     }
// }

// var avgScore = QueueAverageScore();












// public void RegisterPlaybackEvent(PlaybackEvent playbackEvent) {
//     // find the voice with the lowest score
//     PlaybackVoice lowestScoreVoice = playbackVoices[0];
//     double playbackTimeDelta = AudioSettings.dspTime - lastPlayTime;
//     double delay = 0d;
//     if (playbackTimeDelta < delayThresh) {
//         // Debug.Log($"Playback Time Delta {playbackTimeDelta}");
//         delay = delayThresh - playbackTimeDelta;
//         if (delay < 0) {
//             delay = 0;
//         }
//         delayThresh = playbackTimeDelta;
//     }
//     float score = 1f;
//     foreach(PlaybackVoice voice in playbackVoices) {
//         score = voice.Score();
//         if (score == 0f) {
//             voice.Play(playbackEvent, delay);
//             lastPlayTime = AudioSettings.dspTime;
//             return;
//         } 
//         if (score < lowestScoreVoice.Score()) {
//             lowestScoreVoice = voice;
//         }
//     }
//     // Debug.Log($"Stealing Voice with score {score}");
//     lowestScoreVoice.Play(playbackEvent, delay);
//     lastPlayTime = AudioSettings.dspTime;
// }

// if (playbackEvent.sequencedPlayback != null) {
//     // Debug.Log($"Registering Sequenced Playback {playbackEvent.sequencedPlayback.playTime} {playbackEvent.sequencedPlayback.endTime}");
//     // call function which will handle sequential playback (a separate bank of voices)
//     // StartCoroutine(PlaySequenced(playbackEvent));
//     return;
// } 

// public class PlaybackVoice {

//     private DiscreteSignal _signal;

//     private Playhead playhead;
//     // private int playhead;
//     // private int startSample;
//     // private int endSample;
//     // private int windowSamples;
//     private Mutex mut = new Mutex();
//     private bool isProcessing = false;

//     private float[] window;

//     private double scheduledEndTime;
//     private PlaybackEvent playbackEvent;

//     public PlaybackVoice() {
//         // startSample = 0;
//         // endSample = 0;
//         window = new float[0];
//         playhead = new Playhead();
//     }

//     public void Play(PlaybackEvent playbackEvent, double delay = 0d) {
//         // mut.WaitOne(100);

        

//         this.playhead.SetPlaybackEvent(playbackEvent, _signal);
//         this.playbackEvent = playbackEvent;
//         // scheduledEndTime = AudioSettings.dspTime+(playbackEvent.windowTime.duration);
//         // startSample = (int)Mathf.Floor((float)(playbackEvent.windowTime.startTime * _signal.SamplingRate));
//         // endSample = (int)Mathf.Floor((float)(playbackEvent.windowTime.endTime * _signal.SamplingRate));
//         // endSample = Mathf.Min(endSample, _signal.Length);
//         // playhead = startSample;
//         // windowSamples = endSample - startSample;
//         if (window.Length != this.playhead.windowSamples) {
//             window = CosineWindow(this.playhead.windowSamples);
//         }

//         // mut.ReleaseMutex();
//     }

//     private static float[] CosineWindow(int windowSamples) {
//         float[] window = new float[windowSamples];
//         for (int i = 0; i < windowSamples; i++) {
//             window[i] = (Mathf.Cos(Mathf.PI * 2 * ((float)i / (float)windowSamples)) - 1f) * -0.5f;
//         }
//         return window;
//     }

//     /// higher score = more priority for this voice to continue playing
//     public float Score() {
//         return playhead.Score();
//         // float score = endSample - playhead;
//         // float score = endSample - playhead;
//         // if (score == 0) return 0f;
//         // score = score / (float)windowSamples;
//         // return score * Mathf.Abs(playbackEvent.rms);
//     }
    
//     public void SetSignal(DiscreteSignal signal) {
//         _signal = signal;
//     }

//     public void ProcessBlock(float[] buffer, int channels) {
//         // mut.WaitOne(); // prevent monobehavior methods from changing playhead during process block
//         if (playhead.IsFinished()) {
//             // mut.ReleaseMutex();
//             return;
//         }

//         // if (playhead.position >= endSample) {
//         //     return;
//         // }

//         // while (endSample - playhead < buffer.Length / channels) {
//         //     // Debug.Log($"Not enough samples to fill buffer. {endSample - playhead} < {buffer.Length / channels}
//         // }

//         int numSamples = buffer.Length / channels;

//         // numSamples = Mathf.Min(numSamples, endSample - playhead);
//         numSamples = Mathf.Min(numSamples, playhead.SamplesRemaining());

//         for (int i = 0; i < numSamples; i++) {
//             // if (playhead >= endSample) {
//             //     break;
//             // }
//             // if (playhead - startSample >= window.Length) {

//             if (playhead.WindowIndex() >= window.Length) {
//                 playhead.DebugMessage();
//                 continue;
//             }
//             // float winSample = window[playhead - startSample];
//             float winSample = window[playhead.WindowIndex()];
            
//             // float sample = _signal.Samples[playhead] * window[playhead - startSample];
//             float sample = _signal.Samples[playhead.position] * winSample;
//             for (int c = 0; c < channels; c++) {
//                 buffer[i * channels + c] += sample * playbackEvent.gain;
//             }

//             playhead.position++;
//             // playhead++;
//         }
//         // mut.ReleaseMutex();
//     }

// }


// /// <summary>
// /// Main audio manager for the grain model, handling audio playback with multiple voices
// /// </summary>
// public class GrainModelPlayback : MonoBehaviour
// {
//     public int NumVoices { get; private set; } = 16;
//     private PlaybackVoice[] playbackVoices;
//     private double lastPlayTime = 0d;
//     private double delayThresh = 0d;
    
//     void OnEnable()
//     {
//         playbackVoices = new PlaybackVoice[NumVoices];
//         for(int i = 0; i < NumVoices; i++) {
//             AudioSource audioSource = gameObject.AddComponent<AudioSource>();
//             TsvrApplication.AudioManager.ConnectGrainModelAudioSource(audioSource);
//             playbackVoices[i] = new PlaybackVoice(audioSource, null);
//         }
//     }

//     public void SetAudioBuffer(DiscreteSignal signal) {
//         // AudioClip clip = AudioClip.Create("Grain", signal.Length, 1, signal.SamplingRate, false);
//         // clip.SetData(signal.Samples, 0);
//         // foreach(PlaybackVoice voice in playbackVoices) {
//         //     voice.Clip = clip;
//         // }
//     }


//     private void OnAudioFilterRead(float[] data, int channels) {
        
//     }

//     void Update() {
//         foreach(PlaybackVoice voice in playbackVoices) {
//             voice.FadeUpdate();
//         }
//     }

//     /// <summary>
//     /// Takes in a playback event and queues it to the proper PlaybackVoice
//     /// </summary>
//     public void RegisterPlaybackEvent(PlaybackEvent playbackEvent) {
//         if (playbackEvent.sequencedPlayback != null) {
//             // Debug.Log($"Registering Sequenced Playback {playbackEvent.sequencedPlayback.playTime} {playbackEvent.sequencedPlayback.endTime}");
//             // call function which will handle sequential playback (a separate bank of voices)
//             // StartCoroutine(PlaySequenced(playbackEvent));
//             return;
//         } 
//         // find the voice with the lowest score
//         PlaybackVoice lowestScoreVoice = playbackVoices[0];
//         double playbackTimeDelta = AudioSettings.dspTime - lastPlayTime;
//         double delay = 0d;
//         if (playbackTimeDelta < delayThresh) {
//             // Debug.Log($"Playback Time Delta {playbackTimeDelta}");
//             delay = delayThresh - playbackTimeDelta;
//             if (delay < 0) {
//                 delay = 0;
//             }
//             delayThresh = playbackTimeDelta;
//         }
//         float score = 1f;
//         foreach(PlaybackVoice voice in playbackVoices) {
//             score = voice.Score();
//             if (score == 0f) {
//                 voice.Play(playbackEvent, delay);
//                 lastPlayTime = AudioSettings.dspTime;
//                 return;
//             } 
//             if (score < lowestScoreVoice.Score()) {
//                 lowestScoreVoice = voice;
//             }
//         }
//         // Debug.Log($"Stealing Voice with score {score}");
//         lowestScoreVoice.Play(playbackEvent, delay);
//         lastPlayTime = AudioSettings.dspTime;
//     }
// }



// public class PlaybackVoice {

//     public AudioSource audioSource;

//     private AudioClip _clip;
//     public AudioClip Clip { 
//         get { return _clip; }
//         set {
//             _clip = value; 
//             audioSource.clip = _clip;
//         } 
//     }

//     private Coroutine playbackCoroutine;
//     private double scheduledEndTime;
//     private PlaybackEvent playbackEvent;

//     public PlaybackVoice(AudioSource audioSource, AudioClip clip) {
//         this.audioSource = audioSource;
//         this.Clip = clip;
//     }

//     public void Play(PlaybackEvent playbackEvent, double delay = 0d) {
//         this.playbackEvent = playbackEvent;
//         scheduledEndTime = AudioSettings.dspTime+(playbackEvent.windowTime.duration);
//         audioSource.time = (float)playbackEvent.windowTime.startTime;
//         audioSource.volume = playbackEvent.gain;
//         if (!audioSource.isPlaying) 
//             audioSource.PlayScheduled(AudioSettings.dspTime + delay);
//             // audioSource.Play();
//         audioSource.SetScheduledEndTime(scheduledEndTime);
//     }

//     /// higher score = more priority for this voice to continue playing
//     public float Score() {
//         float playTimeRemaining = (float)(scheduledEndTime - AudioSettings.dspTime);
//         if (playTimeRemaining < 0) return 0f;
//         return  (playTimeRemaining /  (float)playbackEvent.windowTime.duration) * Mathf.Abs(playbackEvent.rms); 
//     }

//     // call from a monobehaviour update, which will update the fade of the audio clip accordingly
//     public void FadeUpdate() {
//         // lerp through a cosine function to winow the audio at the current playhead position
//         if (audioSource.isPlaying) {
//             float playTimeRemaining = (float)(scheduledEndTime - AudioSettings.dspTime);
//             // get the gain at cosine window position
//             float gain = (Mathf.Cos(Mathf.PI * 2 * (playTimeRemaining / (float)playbackEvent.windowTime.duration)) - 1f) * -0.5f;
//             audioSource.volume = gain;
//         }
//     }

//     public float[] blockSamples;

//     public void UpdateBlockSamples() {
        
//     }
// }