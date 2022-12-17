using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "TSVR/AudioManager", menuName = "Audio Manager (Singleton)")]
public class AudioManager : SingletonScriptableObject<AudioManager>
{
    // public Setting interfaceSounds = new Setting("Interface Sounds", true, typeof(bool), true);

    // public 

    public void PlayInterfaceSound(AudioClip clip) {
        // if (TsvrApplication.Settings.interfaceSounds.Value == false) return;

        // PlaySound(clip, AppManager.Settings.interfaceSoundsGain.Value);
    }
}