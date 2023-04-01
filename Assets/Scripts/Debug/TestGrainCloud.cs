using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGrainCloud : MonoBehaviour 
{
    public string metadataURI = "E:/UnityProjects/TimbreSpaceVR/Python/data/metadata/jazzGuitar.json";

    public GrainCloud grainCloud;

    void Start() 
    {
        Debug.Log("TestGrainCloud Start");
        GrainCloudSpawner.SpawnFromMetadataURI(metadataURI).ContinueWith((task) => {
            grainCloud = task.Result;
            Debug.Log("GrainCloud Spawned");
        });
    }
}