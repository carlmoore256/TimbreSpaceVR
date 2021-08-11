using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeMgr : MonoBehaviour
{
    AudioFeatures audioFeatures;

    public string loadFile = "Assets/Resources/Audio/AnalogDrums.wav";
    public int frameSize = 4096;
    public int hop = 4096;

    public GameObject grainPf;
    public Transform grainParent;

    public List<GameObject> allGrains;

    private int grainCount = 0;

    void Start()
    {
        audioFeatures = new AudioFeatures();
        GrainFeatures[] grainFeatures = audioFeatures.GenerateAudioFeatures(loadFile, frameSize, hop, 6);
        foreach(GrainFeatures gf in grainFeatures)
        {
            SpawnGrain(gf);
        }
    }

    //void SpawnGrain(float[] audioClip, float[] features)
    void SpawnGrain(GrainFeatures gf)
    {
        GameObject grain = Instantiate(grainPf, grainParent);
        gameObject.name = $"grain_{grainCount}";
        grain.transform.position = new Vector3(gf.mfccs[0], gf.mfccs[1], gf.mfccs[2]);

        //print($"SAMPLE IN MIDDLE {(int)(gf.audioSamples[frameSize / 2])}");
        AudioClip clip = AudioClip.Create(name, gf.audioSamples.Length, 1, gf.sampleRate, false);
        clip.SetData(gf.audioSamples, 0);
        grain.GetComponent<AudioSource>().clip = clip;
        grain.GetComponent<Renderer>().material.color = new Color(gf.mfccs[3], gf.mfccs[4], gf.mfccs[5]);
        grain.GetComponent<GrainBehavior>().grainFeatures = gf;
        //grain.GetComponent<GrainBehavior>().rtmgr = this;
        allGrains.Add(grain);
        grainCount++;
    }


    void Update()
    {
        
    }
}
