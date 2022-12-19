using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "TSVR/AudioManager", menuName = "Audio Manager (Singleton)")]
public class AudioManager : SingletonScriptableObject<AudioManager>
{
    public int SampleRate => AudioSettings.GetConfiguration().sampleRate;
    public static string[] MicrophoneDevices => UnityEngine.Microphone.devices;


    private static AudioSource _microphone;
    public static AudioSource Microphone {
        get {
            if (_microphone == null) {
                _microphone = new GameObject("Microphone").AddComponent<AudioSource>();
            }
            return _microphone;
        }
    }

    public static void StartMicrophone(string deviceName=null) {
        Microphone.loop = true;
        Microphone.mute = false;
        Microphone.clip = UnityEngine.Microphone.Start(deviceName, true, 1, TsvrApplication.AudioManager.SampleRate);
        while(!(UnityEngine.Microphone.GetPosition(null) > 0)) {}
        Microphone.Play();
    }

    public void PlayInterfaceSound(AudioClip clip) {
        // if (TsvrApplication.Settings.interfaceSounds.Value == false) return;
        // PlaySound(clip, TsvrApplication.Settings.interfaceSoundsGain.Value);
    }

}



//float buffSize = 1 / (float)sampleRate * 256;
// public Setting interfaceSounds = new Setting("Interface Sounds", true, typeof(bool), true);