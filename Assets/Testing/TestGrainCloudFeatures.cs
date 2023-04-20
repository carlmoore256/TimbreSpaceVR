using UnityEngine;
using System.Collections;
using System;
public class TestGrainCloudFeatures : MonoBehaviour {

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
                sequence.Schedule(new SequenceableScheduleParameters {
                    scheduleTime = AudioSettings.dspTime + 0.5d,
                    gain = 1.0f
                });

                sequence.OnSequenceablePlayEnd += () => {
                    Debug.Log("Sequence ended, looping...");
                    sequence.Schedule(new SequenceableScheduleParameters {
                        scheduleTime = AudioSettings.dspTime,
                        gain = 1.0f
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
            var x = AudioFeatures.RandomAudioFeature();
            var y = AudioFeatures.RandomAudioFeature();
            var z = AudioFeatures.RandomAudioFeature();
            grainCloud.parameterHandler.XFeature = x;
            grainCloud.parameterHandler.YFeature = y;
            grainCloud.parameterHandler.ZFeature = z;
            Debug.Log("Updating features: X: " + x + ", Y: " + y + ", Z: " + z);
            grainCloud.grainModel.RotateAngle(new Vector3(0, 100, 100), 0.5f);
        }
    }

    void Update() {
        if (grainCloud != null && grainCloud.grainModel != null) {
            modelRotationTarget *= Quaternion.Euler(Vector3.up * Time.deltaTime * 10f);
            Vector3 currentScale = grainCloud.grainModel.transform.localScale;
            grainCloud.grainModel.Reposition(
                        grainCloud.transform.position,
                        modelRotationTarget,
                        currentScale,
                        0.5f
                    );
        }
    }
}