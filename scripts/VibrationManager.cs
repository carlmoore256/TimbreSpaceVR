using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VibrationManager : MonoBehaviour
{

    public void TriggerVibration(int iteration, int freq, int amp, int controller)
    {
        OVRHapticsClip clip = new OVRHapticsClip();
        for(int i = 0; i < iteration; i++)
        {
            clip.WriteSample(i % freq == 0 ? (byte)amp : (byte) 0);
        }
        if (controller == 0)
        {
            OVRHaptics.LeftChannel.Mix(clip);
        } else if (controller == 1)
        {
            OVRHaptics.RightChannel.Mix(clip);
        }
    }

    
}
