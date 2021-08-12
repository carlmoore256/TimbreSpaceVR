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

    // grainModel contains a collection of grains
    public Transform grainModel;

    public List<GameObject> allGrains;

    private int grainCount = 0;

    int count = 0;

    bool lookAtGrain = false;

    void Start()
    {
        audioFeatures = new AudioFeatures();
        GrainFeatures[] grainFeatures = audioFeatures.GenerateAudioFeatures($"Assets/Resources/Audio/{audioFile}", frameSize, hop, 6);

        foreach (GrainFeatures gf in grainFeatures)
        {
            SpawnGrain(gf);
        }
    }

    // create a GrainModel
    void SpawnGrainModel()
    {

    }

    // consider moving to "grainModel" which will control aspects of an individual model,
    // and the inner workings of those collections
    void SpawnGrain(GrainFeatures gf)
    {
        GameObject grain = Instantiate(grainPf, grainModel);
        gameObject.name = $"grain_{grainCount}";
        grain.transform.position = new Vector3(gf.mfccs[0], gf.mfccs[1], gf.mfccs[2]);
        grain.transform.localScale = new Vector3(gf.rms, gf.rms, gf.rms);
        AudioClip newClip = AudioClip.Create(name, gf.audioSamples.Length, 1, gf.sampleRate, false);
        AudioSource grainAudioSource = grain.GetComponent<AudioSource>();
        grainAudioSource.clip = AudioClip.Create(name, gf.audioSamples.Length, 1, gf.sampleRate, false);
        grain.GetComponent<AudioSource>().clip.SetData(gf.audioSamples, 0);
        grain.GetComponent<Renderer>().material.color = new Color(gf.mfccs[3], gf.mfccs[4], gf.mfccs[5]);
        grain.GetComponent<GrainBehavior>().grainFeatures = gf;
        //grain.GetComponent<GrainBehavior>().rtmgr = this;
        allGrains.Add(grain);
        grainCount++;
    }


    void Update()
    {
        //GameObject randGrain = allGrains[Random.Range(0, allGrains.Count - 1)];
        GameObject randGrain = allGrains[count % (allGrains.Count-1)];
        randGrain.GetComponent<GrainBehavior>().PlayGrain();
        count++;

        if(lookAtGrain)
        {

        }
    }
}
