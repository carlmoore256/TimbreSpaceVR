using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NWaves.Signals;
using System.Linq;
using System;

/// <summary>
/// Audio buffer player that accepts WindowedPlaybackEvents, optimized for
/// scheduled events in the future (set WindowedPlaybackEvent.scheduleTime)
/// </summary>
public class ScheduledPlayer : PolyvoicePlayer
{   

    // public List<ScheduledEvent> ScheduledPlaybacks { get; private set; } = new List<ScheduledEvent>();
    private int _ringBufferIndex = 0;
    private double _blockLength;
    private const int _blockPreSchedule = 2; // blocks ahead to preschedule (must be >= 1)    double blockLength;

    private Guid _id = Guid.NewGuid();


    // private List<(WindowedPlaybackEvent playbackEvent, double scheduleTime)> scheduledEvents = new List<(WindowedPlaybackEvent playbackEvent, double scheduleTime)>();
    
    void OnEnable()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        TsvrApplication.AudioManager.ConnectGrainModelAudioSource(audioSource);
        playbackVoices = new PlaybackVoice[NumVoices];
            
        
        int blockSize = AudioSettings.GetConfiguration().dspBufferSize;
        _blockLength = blockSize / (double)AudioSettings.outputSampleRate;;
        
        for(int i = 0; i < NumVoices; i++) {
            playbackVoices[i] = new PlaybackVoice(voice => {
                // Debug.Log("Voice for ScheduledPlayer at " + voice.Id + " finished playing");
            });
        }
    }

    private List<int> eventQueueIndexes = new List<int>(); // store this in a list for efficiency

    private void OnAudioFilterRead(float[] data, int channels) {
            
        // schedule any events at least 1 block ahead of time
        double bufferEndTime = AudioSettings.dspTime + (_blockLength * _blockPreSchedule);

        // eventQueueIndexes.Clear();
        // find any indexes where scheduleTime is less than bufferEndTime
        // loop through scheduled events and play them if they're ready
        // for(int i = 0; i < scheduledEvents.Count; i++) {
            
        //     // in the playbackEvent, times are absolute
        //     if (scheduledEvents[i].scheduleTime <= bufferEndTime) {
        //         eventQueueIndexes.Add(i);
                
        //         playbackVoices[ringBufferIndex].Play(
        //             scheduledEvents[i].playbackEvent, 
        //             scheduledEvents[i].scheduleTime
        //         );

        //         ringBufferIndex = (ringBufferIndex + 1) % playbackVoices.Length;
        //     }
        // }

        // // remove the events that have been played
        // foreach(int idx in eventQueueIndexes.OrderByDescending(v => v)) {
        //     scheduledEvents.RemoveAt(idx);
        // }

        float gain = 0.4f;
        for(int i = 0; i < playbackVoices.Length; i++) {
            if (playbackVoices[i].IsPlaying) {
                playbackVoices[i].ProcessBlock(data, channels, gain);
            }
        }
    }

    /// <summary>
    /// Takes in a playback event and queues it to the best PlaybackVoice
    /// </summary>
    public ScheduledEvent SchedulePlay(WindowedPlaybackEvent playbackEvent, double scheduleTime = 0d) {
        // scheduledEvents.Add((playbackEvent, scheduleTime));

        // this will handle scheduling the events, with a padded amount of time before the block (blockPreSchedule)
        // once this fires, it will take the playbackVoice at the current ringBuffer index and play it, with the given pre-pad
        // before it really needs to start playing

        // this is needed so we can dispense ScheduledEvents, and cancel them before they happen if needed
        ScheduledEvent scheduledEvent = new ScheduledEvent(
            scheduleTime - (_blockLength * _blockPreSchedule),
            () => {
                playbackVoices[_ringBufferIndex].Play(
                    playbackEvent, 
                    scheduleTime
                );
                _ringBufferIndex = (_ringBufferIndex + 1) % playbackVoices.Length;
            }
        );
        DSPScheduler.Schedule(scheduledEvent);
        return scheduledEvent;
    }
}
