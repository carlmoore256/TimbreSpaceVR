using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NWaves.Signals;
using System.Linq;

/// <summary>
/// Audio buffer player that accepts WindowedPlaybackEvents, optimized for
/// scheduled events in the future (set WindowedPlaybackEvent.scheduleTime)
/// </summary>
public class ScheduledPlayer : PolyvoicePlayer
{   
    private int ringBufferIndex = 0;
    double blockLength;
    int blockPreSchedule = 2; // blocks ahead to preschedule (must be >= 1)

    private List<(WindowedPlaybackEvent playbackEvent, double scheduleTime)> scheduledEvents = new List<(WindowedPlaybackEvent playbackEvent, double scheduleTime)>();
    
    void OnEnable()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        TsvrApplication.AudioManager.ConnectGrainModelAudioSource(audioSource);
        playbackVoices = new PlaybackVoice[NumVoices];
        
        int blockSize = AudioSettings.GetConfiguration().dspBufferSize;
        blockLength = blockSize / (double)AudioSettings.outputSampleRate;;
        
        for(int i = 0; i < NumVoices; i++) {
            playbackVoices[i] = new PlaybackVoice(voice => {
                // Debug.Log("Voice " + voice.VoiceIndex + " finished playing");
            });
        }
    }

    private List<int> eventQueueIndexes = new List<int>(); // store this in a list for efficiency

    private void OnAudioFilterRead(float[] data, int channels) {
        eventQueueIndexes.Clear();
            
        // schedule any events at least 1 block ahead of time
        double bufferEndTime = AudioSettings.dspTime + (blockLength * blockPreSchedule);

        // find any indexes where scheduleTime is less than bufferEndTime
        // loop through scheduled events and play them if they're ready
        for(int i = 0; i < scheduledEvents.Count; i++) {
            
            // in the playbackEvent, times are absolute
            if (scheduledEvents[i].scheduleTime <= bufferEndTime) {
                eventQueueIndexes.Add(i);
                playbackVoices[ringBufferIndex].Play(scheduledEvents[i].playbackEvent, scheduledEvents[i].scheduleTime);
                ringBufferIndex = (ringBufferIndex + 1) % playbackVoices.Length;
            }
        }

        // remove the events that have been played
        foreach(int idx in eventQueueIndexes.OrderByDescending(v => v)) {
            // Debug.Log("Remove idx: " + idx);
            scheduledEvents.RemoveAt(idx);
        }

        float gain = 0.4f;
        // don't even hop into the playback voices that aren't active
        for(int i = 0; i < playbackVoices.Length; i++) {
            if (playbackVoices[i].IsPlaying)
                playbackVoices[i].ProcessBlock(data, channels, gain);
        }
    }

    /// <summary>
    /// Takes in a playback event and queues it to the best PlaybackVoice
    /// </summary>
    public void SchedulePlay(WindowedPlaybackEvent playbackEvent, double scheduleTime = 0d) {
        scheduledEvents.Add((playbackEvent, scheduleTime));
    }
}
