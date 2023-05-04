using UnityEngine;
using System.Collections;
using System;

public class TestGrainCloudPlayback : MonoBehaviour {
    public string metadataURI;
    public float featureChangeInterval = 5.0f;
    public int bpm = 120;
    public GranularParameters parameterValues;
    private GrainCloud _grainCloud;
    int numFeatures = Enum.GetNames(typeof(AudioFeature)).Length;


    public Sequence sequence;

    private Quaternion _modelRotationTarget;

    void Start() 
    {
        GrainCloudSpawner.SpawnFromMetadataURI(metadataURI).ContinueWith((task) => {
            _grainCloud = task.Result;
            _modelRotationTarget = Quaternion.identity;
            
            _grainCloud.OnCloudReset += () => {
                Debug.Log("Cloud Reset!");

                int[] indices = _grainCloud.Buffer.SortedGrainIndices(AudioFeatureUtils.RandomAudioFeature(), true);
                BeatGenerator beatGenerator = new BeatGenerator(sequence.Clock);

                int randIdx = UnityEngine.Random.Range(0 , indices.Length);
                // var beatPattern = beatGenerator.BeatPatternFromString("x-x-x-x-x-x-x-x-");

                // foreach (BeatIndex beatIndex in beatGenerator.RepeatPattern(beatPattern, numBars))
                // {
                //     sequence.AddSequenceableAtBeatIndex(
                //         _grainCloud.Grains[indices[randIdx]], 
                //         beatIndex, 
                //         new SequenceableParameters { Gain = 0.25f }
                //     );
                // }
                
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
            _grainCloud.ParameterHandler.XFeature = x;
            _grainCloud.ParameterHandler.YFeature = y;
            _grainCloud.ParameterHandler.ZFeature = z;
            Debug.Log("Updating features: X: " + x + ", Y: " + y + ", Z: " + z);
        }
    }

    void Update() {
        // if (grainCloud != null && grainCloud.Box != null) {
        //     modelRotationTarget *= Quaternion.Euler(Vector3.up * Time.deltaTime * 10f);
        //     // Vector3 currentScale = grainCloud.Box.transform.localScale;
        //     TransformSnapshot targetTransform = grainCloud.CurrentTransform;
        //     targetTransform.Rotation = modelRotationTarget;
        //     grainCloud.MoveTo(targetTransform);
        // }
    }
}