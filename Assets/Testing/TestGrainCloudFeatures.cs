using UnityEngine;
using System.Collections;
using System;
public class TestGrainCloudFeatures : MonoBehaviour {
    public string metadataURI = "E:/UnityProjects/TimbreSpaceVR/Python/data/metadata/jazzGuitar.json";
    public GranularParameters parameterValues;
    private GrainCloud grainCloud;
    int numFeatures = Enum.GetNames(typeof(AudioFeature)).Length;

    public Sequence sequence;

    void Start() 
    {
        Debug.Log("TestGrainCloud Start");
        GrainCloudSpawner.SpawnFromMetadataURI(metadataURI).ContinueWith((task) => {
            grainCloud = task.Result;
            Debug.Log("GrainCloud Spawned");
            grainCloud.OnCloudReset += () => {
                Debug.Log("Cloud Reset!");
                StartCoroutine(UpdateGrainCloudFeatures());
            };
        });
    }

    private IEnumerator UpdateGrainCloudFeatures() {
        sequence = grainCloud.CreateLinearSequence(600);
        grainCloud.Sequences.Add(sequence);
        grainCloud.Play(1.0f);
        Debug.Log("GrainCloud Playing Sequence!");
        while (true) {
            // Update the XFeature, YFeature, and ZFeature with random values
            var x = AudioFeatures.RandomAudioFeature();
            var y = AudioFeatures.RandomAudioFeature();
            var z = AudioFeatures.RandomAudioFeature();
            grainCloud.parameterHandler.XFeature = x;
            grainCloud.parameterHandler.YFeature = y;
            grainCloud.parameterHandler.ZFeature = z;

            Debug.Log("Updating features: X: " + x + ", Y: " + y + ", Z: " + z);
            // Wait for a few seconds before updating the features again
            yield return new WaitForSeconds(1.0f);
        }
    }
}