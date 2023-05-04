using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NWaves.Signals;
using UnityEngine.Pool;


// IPlaybackHandler
public class BufferPlaybackHandler : MonoBehaviour
{
    private int playbackEventPoolInitSize = 1000;
    private int playbackEventPoolMaxSize = 100000;
    private InstantaneousPlayer instantaneousPlayer;
    private ScheduledPlayer scheduledPlayer;
    private ObjectPool<WindowedPlaybackEvent> playbackEventPool;

    private void OnEnable() {
        instantaneousPlayer = gameObject.GetOrAddComponent<InstantaneousPlayer>();
        scheduledPlayer = gameObject.GetOrAddComponent<ScheduledPlayer>();

        playbackEventPool = new ObjectPool<WindowedPlaybackEvent>(
            createFunc : () => {
                var playbackEvent = new WindowedPlaybackEvent();
                playbackEvent.onPlayEnd += () => {
                    playbackEventPool.Release(playbackEvent);
                };
                return playbackEvent;
            }, 
            // actionOnRelease : playbackEvent => playbackEvent.Reset(),
            defaultCapacity : playbackEventPoolInitSize,
            maxSize : playbackEventPoolMaxSize
        );
    }

    private void OnDisable() {
        Stop();
    }

    public WindowedPlaybackEvent GetPooledPlaybackEvent() {
        return playbackEventPool.Get();
    }

    public void SetAudioBuffer(DiscreteSignal buffer) {
        instantaneousPlayer.SetAudioBuffer(buffer);
        scheduledPlayer.SetAudioBuffer(buffer);
    }



    /// <summary>
    /// Play buffer given a WindowedPlaybackEvent using the instantaneous player
    /// </summary>
    public void PlayNow(WindowedPlaybackEvent playbackEvent) {
        instantaneousPlayer.Play(playbackEvent);
    }

    /// <summary>
    /// Play buffer in the future given a WindowedPlaybackEvent using the scheduled player
    /// </summary>
    public ScheduledEvent PlayScheduled(
            WindowedPlaybackEvent playbackEvent, 
            double scheduleTime = 0d, 
            ScheduleCancellationToken cancellationToken = null)
    {
        return scheduledPlayer.SchedulePlay(playbackEvent, scheduleTime, cancellationToken);
    }

    public void Stop() {
        instantaneousPlayer.StopAllCoroutines();
        scheduledPlayer.StopAllCoroutines();
    }

}
