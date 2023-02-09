using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "TSVR/AudioManager", menuName = "Audio Manager (Singleton)")]
public class AudioManager : ScriptableObject
{
    public AudioMixer grainModelAudioMixer;

    private int _sampleRate;
    public int SampleRate => _sampleRate;
    public static string[] MicrophoneDevices => UnityEngine.Microphone.devices;

    private static AudioManager _instance;
    public static AudioManager Instance { get { return _instance; } }
    

    private void OnEnable() { 
        _instance = this;
        _sampleRate = AudioSettings.GetConfiguration().sampleRate;
    }

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

    public string GetDefaultAudioFilePath() {
        return Path.GetFullPath(Path.Combine(Application.streamingAssetsPath, "Audio"));
        // return Path.Combine(Application.streamingAssetsPath, "/Audio");
        // return Application.streamingAssetsPath + "/Audio";
    }

    public FileInfo[] GetDefaultAudioFiles() {
        string filePath = Application.streamingAssetsPath + "/Audio";
        DirectoryInfo dir = new DirectoryInfo(filePath);
        FileInfo[] info = dir.GetFiles("*.*");
        return info;
    }

    // eventually make into a flexible dictionary that can patch
    public void ConnectGrainModelAudioSource(AudioSource source) {
        source.outputAudioMixerGroup = grainModelAudioMixer.FindMatchingGroups("Dry")[0];
    }
}



//float buffSize = 1 / (float)sampleRate * 256;
// public Setting interfaceSounds = new Setting("Interface Sounds", true, typeof(bool), true);        // string fileString;
        // if (Application.platform == RuntimePlatform.Android) {
        //     WWW reader = new WWW(filePath);
        //     while (!reader.isDone) { }
        //     fileString = reader.text;
        // } else {
        //     fileString = File.ReadAllText(filePath);
        // }