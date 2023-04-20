using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGrainCloud : MonoBehaviour 
{
    public string metadataURI = "E:/UnityProjects/TimbreSpaceVR/Python/data/metadata/jazzGuitar.json";
    public GrainCloud grainCloud;

    public Sequence sequence;

    public float bpm = 120;
    private float lastBPM = 0;

    void Start() 
    {
        Debug.Log("TestGrainCloud Start");
        GrainCloudSpawner.SpawnFromMetadataURI(metadataURI).ContinueWith((task) => {
            grainCloud = task.Result;
            Debug.Log("GrainCloud Spawned");
        });
    }

    void Update() {

        if (lastBPM != bpm && sequence != null) {
            lastBPM = bpm;
            sequence.SetBPM(bpm);
            Debug.Log("BPM: " + bpm);
        }
        if (Input.GetKeyUp(KeyCode.Space)) {
            sequence = grainCloud.CreateLinearSequence(bpm);
            grainCloud.Sequences.Add(sequence);
            grainCloud.Play(1.0f);
            Debug.Log("GrainCloud Playing Sequence!");
        }

        lastBPM = bpm;
    }
}