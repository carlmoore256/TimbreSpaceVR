using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeMgr : MonoBehaviour
{
    AudioFeatures audioFeatures;

    public string audioFile = "AnalogDrums.wav";
    public int frameSize = 4096;
    public int hop = 4096;

    public GameObject grainPf;
    public GameObject modelPf;

    // grainModel contains a collection of grains
    public Transform grainModel;
    GrainModel selectedModel;

    public List<GameObject> allGrains;

    private int grainCount = 0;

    int count = 0;

    public bool lookAtGrain = false;
    public Vector3 lookAtPos = new Vector3(0,-1,-4);

    void Start()
    {
        SpawnGrainModel(Vector3.zero, audioFile);
    }

    // create a GrainModel
    void SpawnGrainModel(Vector3 spawnPos, string audioPath=null)
    {
        // how objects will be spawned from now on
        GameObject newModel = Instantiate(modelPf, GameObject.Find("--SCENE--").transform);
        GrainModel gm = newModel.AddComponent<GrainModel>();
        selectedModel = gm;
        gm.Initialize(grainPf, spawnPos, audioPath);

    }

    // consider moving to "grainModel" which will control aspects of an individual model,
    // and the inner workings of those collections
    //void SpawnGrain(GrainFeatures gf)
    //{
    //    GameObject grain = Instantiate(grainPf, grainModel);
    //    gameObject.name = $"grain_{grainCount}";
    //    grain.transform.position = new Vector3(gf.mfccs[0], gf.mfccs[1], gf.mfccs[2]);
    //    grain.transform.localScale = new Vector3(gf.rms, gf.rms, gf.rms);
    //    AudioClip newClip = AudioClip.Create(name, gf.audioSamples.Length, 1, gf.sampleRate, false);
    //    AudioSource grainAudioSource = grain.GetComponent<AudioSource>();
    //    grainAudioSource.clip = AudioClip.Create(name, gf.audioSamples.Length, 1, gf.sampleRate, false);
    //    grain.GetComponent<AudioSource>().clip.SetData(gf.audioSamples, 0);
    //    grain.GetComponent<Renderer>().material.color = new Color(gf.mfccs[3], gf.mfccs[4], gf.mfccs[5]);
    //    grain.GetComponent<Grain>().features = gf;
    //    //grain.GetComponent<GrainBehavior>().rtmgr = this;
    //    allGrains.Add(grain);
    //    grainCount++;
    //}


    void Update()
    {
        if (lookAtGrain && selectedModel != null)
        {
            lookAtGrain = false;
            selectedModel.MoveLookAt(
                Camera.main.transform.position,
                Camera.main.transform.position + lookAtPos);
        }
    }
}
