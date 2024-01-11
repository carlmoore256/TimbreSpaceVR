using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[ExecuteInEditMode]
public class TestMusicalSequencing : TestGrainCloud 
{
    public List<string> patterns = new List<string> {
        "x-x-x-x-x-x-x-x-",
        "x---x---x---x---",
        "x-x-x-x-xx-x-x-x-",
        "---x---x---x---x"
    };

    public TimeSignature timeSignature;
    [SerializeField] private float _bpm = 120;

    public float BPM { 
        get => _bpm;
        set
        {
            if (_bpm != value)
            {
                _bpm = value;
                if (Sequence != null)
                {
                    Debug.Log("Setting BPM to " + value);
                    Sequence.Clock.BPM = value;
                }
            }
        }}
    public int numBars = 4;
    public int numSections = 10;
    public NoteScrubber sequenceDisplay;
    public Sequence Sequence;
    private Quaternion _modelRotationTarget = Quaternion.identity;
    private float _lastTime = 0;

    // IDEA - make snake like sequence that evolves over time


    public override void OnGrainCloudReset()
    {
        base.OnGrainCloudReset();
        Debug.Log("Cloud Reset!");
        if (Sequence != null)
        {
            Sequence.Stop();
        }

        // build the sequence
        Sequence = new Sequence();
        Sequence.Clock.BPM = _bpm;
        // sequence.Clock.TimeSignature = new TimeSignature(7, NoteValue.Quarter);
        SequenceRenderer renderer = GrainCloud.gameObject.GetOrAddComponent<SequenceRenderer>();
        renderer.SetSequence(Sequence);
        sequenceDisplay = gameObject.GetComponentInChildren<NoteScrubber>();
        sequenceDisplay.SetSequence(Sequence);
        // DelayedStopResume();
        ResetSequences();
    }

    public void ResetSequences()
    {
        Sequence.Stop();
        Sequence.Clear();
        BeatGenerator beatGenerator = new BeatGenerator(Sequence.Clock);
        AddStringPatterns(GrainCloud, Sequence, patterns, numBars);
        Sequence.Play();
    }

    private void DelayedStopResume()
    {
        CoroutineHelpers.DelayedAction(() =>
        {
            Debug.Log("[TEST] Stopping sequence");
            Sequence.Stop();
        }, 5f, this);

        CoroutineHelpers.DelayedAction(() =>
        {
            Debug.Log("[TEST] RESUMING sequence");
            Sequence.Resume();
        }, 8f, this);
    }

    private void AddStringPatterns(GrainCloud cloud, Sequence sequence, List<string> patterns, int numBars) {
        int i = 0;
        int[] indices = cloud.Buffer.SortedGrainIndices(AudioFeatureUtils.RandomAudioFeature(), true);
        BeatGenerator beatGenerator = new BeatGenerator(sequence.Clock);

        for (int j = 0; j < numBars; j++) {
            foreach (string pattern in patterns) {
                // int randIdx = UnityEngine.Random.Range(0, cloud.GranularBuffer.NumWindows);
                int randIdx = UnityEngine.Random.Range(0 , indices.Length);
                var beatPattern = beatGenerator.BeatPatternFromString(pattern);
                foreach (BeatIndex beatIndex in beatGenerator.RepeatPattern(beatPattern, numBars))
                {
                    beatIndex.Bar += j * numBars;
                    sequence.AddSequenceableAtBeatIndex(
                        cloud.Grains[indices[randIdx]], 
                        beatIndex, 
                        new SequenceableParameters { Gain = 0.25f }
                    );
                    i++;
                }
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
        var RMS = cloud.Buffer.SortedGrainIndicesWithValues(AudioFeature.RMS, false);
        var noiseness = cloud.Buffer.SortedGrainIndicesWithValues(AudioFeature.Noiseness, false);
        // sort based on 2 criteria - RMS and noiseness
        var bestIndexes = CollectionHelpers.ArgsortTopPairs(RMS, noiseness, 10, (a, b) => a * b).ToArray();

        int hh = 0;
        foreach (BeatIndex beatIndex in hhPattern)
        {
            int rndFeatureIdx = UnityEngine.Random.Range(0, (int)(bestIndexes.Length));
            cloud.Buffer.GetFeatureValue(AudioFeature.RMS, bestIndexes[rndFeatureIdx]);
            sequence.AddSequenceableAtBeatIndex(cloud.Grains[bestIndexes[hh%8]], beatIndex, new SequenceableParameters { Gain = 0.025f });
            hh++;
        }

        List<BeatIndex> kickPattern = beatGenerator.GenerateKickPattern(numBars, 1);
        int[] kickIndices = cloud.Buffer.SortedGrainIndices(AudioFeature.Centroid, true);
        int k = 0;
        foreach (BeatIndex beatIndex in kickPattern)
        {
            sequence.AddSequenceableAtBeatIndex(cloud.Grains[kickIndices[2]], beatIndex);
            k++;
        }

        List<BeatIndex> snarePattern = beatGenerator.GenerateSnarePattern(numBars, 1);
        int[] snareIndices = cloud.Buffer.SortedGrainIndices(AudioFeature.Crest, true);
        int s = 0;
        foreach (BeatIndex beatIndex in snarePattern)
        {
            sequence.AddSequenceableAtBeatIndex(cloud.Grains[snareIndices[s%4]], beatIndex, new SequenceableParameters { Gain = 0.325f });
            s++;
        }

        List<BeatIndex> pattern = beatGenerator.GenerateAlgorithmicPattern(numBars);
        int[] indices = cloud.Buffer.SortedGrainIndices(AudioFeatureUtils.RandomAudioFeature(), true);
        int i = 0;
        foreach (BeatIndex beatIndex in pattern)
        {
            sequence.AddSequenceableAtBeatIndex(cloud.Grains[indices[i%sequence.Clock.TimeSignature.BeatsPerBar]], beatIndex, new SequenceableParameters { Gain = 0.245f });
            i++;
        }
    }


    void Update() {


        if (Input.GetKey(KeyCode.Space)) {
            base.RotateHorizontal(10f);
        }

        if (Input.mouseScrollDelta.y != 0) {
            base.ZoomInOut(Input.mouseScrollDelta.y * 8f);
        }

        // if (Math.Abs(Input.mousePosition.x) > 0.2) {
        //     base.RotateHorizontal((((Input.mousePosition.x / Screen.width) * 2f) - 1f) * -20f);
        // }
    }
}