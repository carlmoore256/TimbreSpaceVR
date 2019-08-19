using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GrainAudio functions as the connection between Grain Parameterization and runtime, and calculations here are sent to runtime on start
/// </summary>

public class GrainAudio : MonoBehaviour
{
    int sampNum;
    int voiceNum;
    int grainSize;

    //Polyphonic priority handled by the Unity audio engine
    public int priorityHigh = 64;
    public int priorityMed = 127;
    public int priorityLow = 255;

    public double playDuration;

    //Polyphonic voice allocation
    public AudioSource voice0;
    public AudioSource voice1;
    public AudioSource voice2;

    void Start()
    {
        sampNum = GetComponent<GrainParametrization>().sampNum;
        grainSize = GetComponent<GrainParametrization>().grainSize;
        voice1.clip = voice0.clip;
        voice2.clip = voice0.clip;
        voiceNum = 0;
        //playDuration = (double) grainSize / voice0.clip.frequency;
        GetComponent<GrainRuntime>().playDuration = (float)playDuration;
    }

    public void Playback(float activeTrig, double playbackOffset, bool constellation)
    {

        double startTime = AudioSettings.dspTime;
        if (constellation)
            startTime = playbackOffset;

        if (activeTrig != 0)
        {
            if (voiceNum == 0)
            {
                voice0.volume = activeTrig * activeTrig;
                voiceNum++;
                voice0.timeSamples = sampNum;

                voice0.PlayScheduled(startTime);
                voice0.SetScheduledEndTime(startTime + playDuration);
                GetComponent<GrainRuntime>().ActivateGrain();
                //activated = true;
                voice0.priority = priorityHigh;
                voice1.priority = priorityMed;
                voice2.priority = priorityLow;
            }
            else if (voiceNum == 1)
            {
                voice1.volume = activeTrig * activeTrig;
                voiceNum++;
                voice1.timeSamples = sampNum;
                //double startTime = AudioSettings.dspTime;
                voice1.PlayScheduled(startTime);
                voice1.SetScheduledEndTime(startTime + playDuration);
                GetComponent<GrainRuntime>().ActivateGrain();
                //activated = true;

                voice0.priority = priorityMed;
                voice1.priority = priorityHigh;
                voice2.priority = priorityLow;
            }
            else if (voiceNum == 2)
            {
                voice2.volume = activeTrig * activeTrig;
                voiceNum = 0;
                voice2.timeSamples = sampNum;
                //double startTime = AudioSettings.dspTime;
                voice2.PlayScheduled(startTime);
                voice2.SetScheduledEndTime(startTime + playDuration);
                GetComponent<GrainRuntime>().ActivateGrain();
                //activated = true;

                voice0.priority = priorityLow;
                voice1.priority = priorityMed;
                voice2.priority = priorityHigh;
            }
        }
    }
}
