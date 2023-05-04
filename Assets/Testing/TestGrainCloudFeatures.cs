using UnityEngine;
using System.Collections;
using System;

public class TestGrainCloudFeatures : MonoBehaviour {


    public float featureChangeInterval = 5.0f;
    private Coroutine _featureUpdateCoroutine;

    public TestGrainCloud testGrainCloud;



    private void OnEnable()
    {
        testGrainCloud = GetComponent<TestGrainCloud>();
        // testGrainCloud.OnGrainCloudResetEvent += () => {
        //     if (_featureUpdateCoroutine != null) {
        //         StopCoroutine(_featureUpdateCoroutine);
        //     }
        //     _featureUpdateCoroutine = StartCoroutine(UpdateGrainCloudFeatures(testGrainCloud.GrainCloud, featureChangeInterval));
        // };
        // testGrainCloud.OnGrainCloudSpawnEvent += OnGrainCloudSpawn;
    }

    public void UpdateFeatureRandom(string axis)
    {
        var featureType = AudioFeatureUtils.RandomAudioFeature();
        if (testGrainCloud == null || testGrainCloud.GrainCloud == null) return;
        if (axis == "x")
        {
            testGrainCloud.GrainCloud.ParameterHandler.XFeature = featureType;
        }
        else if (axis == "y")
        {
            testGrainCloud.GrainCloud.ParameterHandler.YFeature = featureType;
        }
        else if (axis == "z")
        {
            testGrainCloud.GrainCloud.ParameterHandler.ZFeature = featureType;
        }
    }

    // public void UpdateAllFeatures()
    // {
    //     StartCoroutine(UpdateGrainCloudFeatures(testGrainCloud.GrainCloud, featureChangeInterval));
    // }

    private IEnumerator UpdateGrainCloudFeatures(GrainCloud grainCloud, float featureChangeInterval) {
        Debug.Log("GrainCloud Playing Sequence!");
        while (true) {
            yield return new WaitForSeconds(featureChangeInterval);
            // Update the XFeature, YFeature, and ZFeature with random values
            var x = AudioFeatureUtils.RandomAudioFeature();
            var y = AudioFeatureUtils.RandomAudioFeature();
            var z = AudioFeatureUtils.RandomAudioFeature();
            grainCloud.ParameterHandler.XFeature = x;
            grainCloud.ParameterHandler.YFeature = y;
            grainCloud.ParameterHandler.ZFeature = z;
            Debug.Log("Updating features: X: " + x + ", Y: " + y + ", Z: " + z);
        }
    }
}