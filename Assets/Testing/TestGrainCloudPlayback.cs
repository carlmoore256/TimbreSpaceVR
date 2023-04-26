using UnityEngine;
using System.Collections;
using System;

public class TestGrainCloudPlayback : MonoBehaviour {
    public string metadataURI;
    public float featureChangeInterval = 5.0f;
    public int bpm = 120;
    public GranularParameters parameterValues;
    private GrainCloud grainCloud;
    int numFeatures = Enum.GetNames(typeof(AudioFeature)).Length;


    public Sequence sequence;

    private Quaternion modelRotationTarget;

    void Start() 
    {
        GrainCloudSpawner.SpawnFromMetadataURI(metadataURI).ContinueWith((task) => {
            grainCloud = task.Result;
            modelRotationTarget = Quaternion.identity;
            
            grainCloud.OnCloudReset += () => {
                Debug.Log("Cloud Reset!");
                sequence = grainCloud.CreateLinearSequence(bpm);
                grainCloud.Sequences.Add(sequence);
                
                // ----- How to Schedule -----------------

                // We could take advantage of the fact that GrainCloud is
                // itself an ISequentiable:
                // grainCloud.Schedule(new SequenceableScheduleParameters {
                //     scheduleTime = AudioSettings.dspTime + 0.5d,
                //     gain = 1.0f
                // });

                // OR we can schedule the sequence itself
                sequence.Schedule(AudioSettings.dspTime + 0.5d, new SequenceableParameters {
                    Gain = 1.0f
                });

                sequence.OnSequenceablePlayEnd += () => {
                    Debug.Log("Sequence ended, looping...");
                    sequence.Schedule(AudioSettings.dspTime,
                        new SequenceableParameters {
                            Gain = 1.0f
                        });
                };

                // in either case, GrainCould has a private void OnGrainScheduled(), which
                // reports any new scheduling events to the playbackHandler
                // which will then dilligently wait for the right time to play the event

                StartCoroutine(UpdateGrainCloudFeatures());
            };
        });
    }

    private IEnumerator UpdateGrainCloudFeatures() {

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
            grainCloud.GrainModel.RotateAngle(new Vector3(0, 100, 100), 0.5f);
        }
    }

    void Update() {
        if (grainCloud != null && grainCloud.GrainModel != null) {
            modelRotationTarget *= Quaternion.Euler(Vector3.up * Time.deltaTime * 10f);
            Vector3 currentScale = grainCloud.GrainModel.transform.localScale;
            grainCloud.GrainModel.Reposition(
                        grainCloud.transform.position,
                        modelRotationTarget,
                        currentScale,
                        0.5f
                    );
        }
    }
}