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
    private const int _blockPreSchedule = 8; // blocks ahead to preschedule (must be >= 1)
    private int _ringBufferIndex = 0;
    private double _blockLength;

    public override void OnEnable()
    {    
        int blockSize = AudioSettings.GetConfiguration().dspBufferSize;
        _blockLength = blockSize / (double)AudioSettings.outputSampleRate;;
        base.OnEnable();
    }

    /// <summary>
    /// Takes in a playback event and queues it to the best PlaybackVoice
    /// </summary>
    public ScheduledEvent SchedulePlay(
            WindowedPlaybackEvent playbackEvent, 
            double scheduleTime, 
            ScheduleCancellationToken cancellationToken = null) 
    {
        // scheduledEvents.Add((playbackEvent, scheduleTime));

        // this will handle scheduling the events, with a padded amount of time before the block (blockPreSchedule)
        // once this fires, it will take the playbackVoice at the current ringBuffer index and play it, with the given pre-pad
        // before it really needs to start playing

        // this is needed so we can dispense ScheduledEvents, and cancel them before they happen if needed
        ScheduledEvent scheduledEvent = new ScheduledEvent(
            scheduleTime: scheduleTime - (_blockLength * _blockPreSchedule),
            onSchedule: () => {
                _playbackVoices[_ringBufferIndex].Play(
                    playbackEvent, 
                    scheduleTime
                );
                _ringBufferIndex = (_ringBufferIndex + 1) % _playbackVoices.Length;
            },
            cancellationToken: cancellationToken == null ? new ScheduleCancellationToken() : cancellationToken
        );

        scheduledEvent.Id = playbackEvent.Id;
        // _scheduler.Schedule(scheduledEvent);
        DSPSchedulerSingleton.Schedule(scheduledEvent);
        // scheduledEvent.Schedule();
        return scheduledEvent;
    }
}
