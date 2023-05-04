using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TestGrainCloud : MonoBehaviour 
{
    public string MetadataURI = "E:/UnityProjects/TimbreSpaceVR/Python/data/metadata/jazzGuitar.json";
    public GrainCloud GrainCloud { get; private set; }
    private TransformSnapshot _modelTransformTarget;

    public event Action OnGrainCloudResetEvent;
    public event Action OnGrainCloudSpawnEvent; 

    private bool _enableRotation = false;

    void Start() 
    {
        Debug.Log("TestGrainCloud Start");
        GrainCloudSpawner.SpawnFromMetadataURI(MetadataURI).ContinueWith((task) => {
            GrainCloud = task.Result;
            
            Debug.Log("GrainCloud Spawned");
            GrainCloud.OnCloudReset += OnGrainCloudReset;
            // OnGrainCloudReset();
        });
    }

    public virtual void OnGrainCloudSpawn()
    {
        Debug.Log("GrainCloud Spawned!");
        OnGrainCloudSpawnEvent?.Invoke();
    }

    public virtual void OnGrainCloudReset() {
        _enableRotation = true;
        Debug.Log("GrainCloud Reset!");
        _modelTransformTarget = new TransformSnapshot(GrainCloud.transform);
        OnGrainCloudResetEvent?.Invoke();
    }

    public void ZoomInOut(float speed=10f)
    {
        if(!_enableRotation || GrainCloud == null) return;
        _modelTransformTarget.Position += Vector3.forward * Time.deltaTime * speed;
        GrainCloud.MoveTo(_modelTransformTarget);
    }


    public void RotateHorizontal(float speed=10f)
    {
        if(!_enableRotation || GrainCloud == null) return;
        _modelTransformTarget.Rotation *= Quaternion.Euler(Vector3.up * Time.deltaTime * speed);
        GrainCloud.MoveTo(_modelTransformTarget);
    }
}