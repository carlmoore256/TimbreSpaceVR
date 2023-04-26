using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[ExecuteInEditMode]
public class TestMusicalSequencing : MonoBehaviour {
    public string metadataURI;
    public float bpm = 120;
    public GranularParameters parameterValues;
    private GrainCloud grainCloud;
 
    int numBars = 100;

    private Quaternion modelRotationTarget = Quaternion.identity;

    public List<string> patterns = new List<string> {
        "x-x-x-x-x-x-x-x-",
        "x---x---x---x---",
        "x-x-x-x-xx-x-x-x-",
        "---x---x---x---x"
    };


    void Start() 
    {
        GrainCloudSpawner.SpawnFromMetadataURI(metadataURI).ContinueWith((task) => {
            SetupSequence(task.Result);
        });
    }

    private void AddStringPatterns(GrainCloud cloud, Sequence sequence, List<string> patterns, int numBars) {
        int i = 0;
        int[] indices = cloud.GranularBuffer.SortedGrainIndices(AudioFeatureUtils.RandomAudioFeature(), true);
        BeatGenerator beatGenerator = new BeatGenerator(sequence.Clock);

        foreach (string pattern in patterns) {
            // int randIdx = UnityEngine.Random.Range(0, cloud.GranularBuffer.NumWindows);
            int randIdx = UnityEngine.Random.Range(0 , indices.Length);
            foreach (BeatIndex beatIndex in beatGenerator.RepeatPattern(beatGenerator.BeatPatternFromString(pattern), numBars))
            {
                sequence.AddSequenceableAtBeatIndex(
                    cloud.Grains[indices[randIdx]], 
                    beatIndex, 
                    new SequenceableParameters { Gain = 0.25f }
                );
                i++;
            }
        }
    }


    /// <summary>
    /// Demo of how to construct drum patterns with the BeatGenerator
    /// </summary>
    private void AddDrumPatterns(GrainCloud cloud, Sequence sequence, int numBars)
    {
        BeatGenerator beatGenerator = new BeatGenerator(sequence.Clock);
        
        List<BeatIndex> hhPattern = beatGenerator.GenerateHiHatPattern(numBars, 1);
        var RMS = cloud.GranularBuffer.SortedGrainIndicesWithValues(AudioFeature.RMS, false);
        var noiseness = cloud.GranularBuffer.SortedGrainIndicesWithValues(AudioFeature.Noiseness, false);
        // sort based on 2 criteria - RMS and noiseness
        var bestIndexes = CollectionHelpers.ArgsortTopPairs(RMS, noiseness, 10, (a, b) => a * b).ToArray();

        int hh = 0;
        foreach (BeatIndex beatIndex in hhPattern)
        {
            int rndFeatureIdx = UnityEngine.Random.Range(0, (int)(bestIndexes.Length));
            cloud.GranularBuffer.GetFeatureValue(AudioFeature.RMS, bestIndexes[rndFeatureIdx]);
            sequence.AddSequenceableAtBeatIndex(cloud.Grains[bestIndexes[hh%8]], beatIndex, new SequenceableParameters { Gain = 0.025f });
            hh++;
        }

        List<BeatIndex> kickPattern = beatGenerator.GenerateKickPattern(numBars, 1);
        int[] kickIndices = cloud.GranularBuffer.SortedGrainIndices(AudioFeature.Centroid, true);
        int k = 0;
        foreach (BeatIndex beatIndex in kickPattern)
        {
            sequence.AddSequenceableAtBeatIndex(cloud.Grains[kickIndices[2]], beatIndex);
            k++;
        }


        List<BeatIndex> snarePattern = beatGenerator.GenerateSnarePattern(numBars, 1);
        int[] snareIndices = cloud.GranularBuffer.SortedGrainIndices(AudioFeature.Crest, true);
        int s = 0;
        foreach (BeatIndex beatIndex in snarePattern)
        {
            sequence.AddSequenceableAtBeatIndex(cloud.Grains[snareIndices[s%4]], beatIndex, new SequenceableParameters { Gain = 0.325f });
            s++;
        }


        List<BeatIndex> pattern = beatGenerator.GenerateAlgorithmicPattern(numBars);
        int[] indices = cloud.GranularBuffer.SortedGrainIndices(AudioFeatureUtils.RandomAudioFeature(), true);
        int i = 0;
        foreach (BeatIndex beatIndex in pattern)
        {
            sequence.AddSequenceableAtBeatIndex(cloud.Grains[indices[i%sequence.Clock.TimeSignature.BeatsPerBar]], beatIndex, new SequenceableParameters { Gain = 0.245f });
            i++;
        }
    }

    private void SetupSequence(GrainCloud cloud) {
        grainCloud = cloud;

        cloud.OnCloudReset += () => {
            Debug.Log("Cloud Reset!");

            // build the sequence
            Sequence sequence = new Sequence();
            sequence.Clock.BPM = bpm;
            // sequence.Clock.TimeSignature = new TimeSignature(7, NoteValue.Quarter);

            SequenceRenderer renderer = cloud.gameObject.GetOrAddComponent<SequenceRenderer>();
            renderer.SetSequence(sequence);
            BeatGenerator beatGenerator = new BeatGenerator(sequence.Clock);

            AddStringPatterns(cloud, sequence, patterns, numBars);


            sequence.Play();

            StartCoroutine(CancelSequence(sequence));


        };
    }

    private IEnumerator CancelSequence(Sequence sequence)
    {
        Debug.Log("Waiting for 5 seconds...");
        yield return new WaitForSeconds(5f);
        sequence.Stop();
    }


    float lastTime = 0;
    float lastBPM = 0;
    void Update() {
        if (Time.time - lastTime > 1.0f) {
            Debug.Log("Current DSP TIME: " + AudioSettings.dspTime);
            lastTime = Time.time;
        }


        // if (sequence != null) {
        //     if (lastBPM != bpm) {
        //         Debug.Log("Setting BPM to " + bpm);
        //         lastBPM = bpm;
        //         // sequence.SetBPM(bpm);
        //     }
        // }

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

// for(int bar = 0; bar < numBars; bar++) {
//     var sortByFeature = AudioFeatures.RandomAudioFeature();
//     Debug.Log("Sorting by " + sortByFeature.ToString());
//     int[] indicesRand = cloud.GranularBuffer.RandomGrainIndices(sequence.Clock.TimeSignature.BeatsPerBar);
//     int[] indicesFeat = cloud.GranularBuffer.SortedGrainIndices(sortByFeature);
//     // add the sequenceable to the sequence
//     for(int beat = 0; beat < sequence.Clock.TimeSignature.BeatsPerBar; beat++)
//     {
//         if (beat % 2 == 0) {
//             sequence.AddSequenceableAtNoteValue(cloud.Grains[indicesFeat[beat]], bar, NoteValue.Quarter, beat, 1.0f);
//         } else {
//             sequence.AddSequenceableAtNoteValue(cloud.Grains[indicesRand[beat]], bar, NoteValue.Quarter, beat, 1.0f);
//         }
//         if (beat == 1) {
//             sequence.AddSequenceableAtNoteValue(cloud.Grains[indicesRand[beat]], bar, NoteValue.Eighth, beat, 1.0f);
//         }
//     }
// }

// for (int bar = 0; bar < numBars; bar++)
// {
//     for (int beat = 0; beat < sequence.Clock.TimeSignature.BeatsPerBar; beat++)
//     {
//         int[] indicesCall = cloud.GranularBuffer.SortedGrainIndices(AudioFeatures.RandomAudioFeature());
//         int[] indicesResponse = cloud.GranularBuffer.SortedGrainIndices(AudioFeatures.RandomAudioFeature());

//         if (bar % 2 == 0) // Call
//         {
//             sequence.AddSequenceableAtNoteValue(cloud.Grains[indicesCall[beat]], bar, NoteValue.Quarter, beat);
//         }
//         else // Response
//         {
//             sequence.AddSequenceableAtNoteValue(cloud.Grains[indicesResponse[beat]], bar, NoteValue.Quarter, beat);
//         }
//     }
// }
