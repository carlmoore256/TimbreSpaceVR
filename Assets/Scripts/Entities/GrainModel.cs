using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;
using System;
using NWaves.Signals;
using System.Threading.Tasks;
using UnityEngine;


public enum GrainModelState {
    Unplaced,
    Placed
}


// ideas - consider making an object pool for GrainModel, to reduce CPU load
// when loading a new audio file => https://learn.unity.com/tutorial/introduction-to-object-pooling#5ff8d015edbc2a002063971d

/// <summary>
/// Manager and container for a collection of playable grains
/// </summary>
public class GrainModel : MonoBehaviour {
    // public float ModelScale { get => transform.localScale.x; set => TargetScale = Vector3.one * value; }
    public DiscreteSignal AudioBuffer { get; protected set; }
    public List<Grain> Grains { get; protected set; }
    public GrainModelState State { get; set; } = GrainModelState.Unplaced;
    public GrainModelParameters Parameters { get; protected set; }

    public Color colorProcessing = new Color(222, 92, 0, 0.6f);
    public Color colorPlaced = new Color(255, 255, 255, 0.0f);
    public Color colorUnplaced = new Color(255, 255, 255, 1f);

    private float grainScale = 2f;
    
    // eventually this will be in its own constellation class
    public float seqPlayRate = 0.1f;
    private bool useHSV = false;

    private Vector3 posAxisScale = Vector3.one;

    Thread T_FeatureExtractor = null;
    ConcurrentQueue<GrainFeatures> grainFeaturesQueue = new ConcurrentQueue<GrainFeatures>();
    ConcurrentQueue<GrainAudioFeatures> grainAudioFeaturesQueue = new ConcurrentQueue<GrainAudioFeatures>();

    private GrainModelPlayback grainModelPlayback;
    public GameObject boundingBox;

    public TransformCoroutineManager coroutineManager;

    AudioFeatureExtractor featureExtractor;

    /// ===== MonoBehaviours ===========================================================

    void OnEnable() {

        grainModelPlayback = gameObject.AddComponent<GrainModelPlayback>();
        coroutineManager = new TransformCoroutineManager(this, () => {
            Debug.Log("Sending Spring Toggle broadcast message -> OFF");
            BroadcastMessage("ToggleSpring", false, SendMessageOptions.DontRequireReceiver);
        }, () => {
            Debug.Log("Sending Spring Toggle broadcast message -> ON");
            BroadcastMessage("ToggleSpring", true, SendMessageOptions.DontRequireReceiver);
        });

        Grains = new List<Grain>();
        // Initialize parameter callbacks which will update the grains when changed
        this.Parameters = new GrainModelParameters(
            (AudioFeature[] posFeatures) => {
                if (this.AudioBuffer == null) return;
                if (TsvrApplication.Settings.DebugLogging)
                TsvrApplication.DebugLogger.Log("Position Parameters changed " + posFeatures[0].ToString() + " " + posFeatures[1].ToString() + " " + posFeatures[2].ToString(), "[GrainModel]");
                featureExtractor.ExtractFeatures(this.AudioBuffer, posFeatures, () => { // make sure we compute any features that need to be computed
                    foreach(Grain grain in Grains)
                        grain.UpdatePosition(posFeatures[0], posFeatures[1], posFeatures[2], posAxisScale);
                });
            }, 
            (AudioFeature[] colFeatures) => {
                if (this.AudioBuffer == null) return;
                TsvrApplication.DebugLogger.Log("Color Parameters changed " + colFeatures[0].ToString() + " " + colFeatures[1].ToString() + " " + colFeatures[2].ToString(), "[GrainModel]");
                featureExtractor.ExtractFeatures(this.AudioBuffer, colFeatures, () => {
                    foreach(Grain grain in Grains)
                        grain.UpdateColor(colFeatures[0], colFeatures[1], colFeatures[2], useHSV);
                });
            },
            (AudioFeature sclFeature) => {
                if (this.AudioBuffer == null) return;
                TsvrApplication.DebugLogger.Log("Scale Parameters changed " + sclFeature.ToString(), "[GrainModel]");
                featureExtractor.ExtractFeatures(this.AudioBuffer, new AudioFeature[] {sclFeature}, () => {
                    foreach(Grain grain in Grains)
                        grain.UpdateScale(sclFeature, grainScale);
                });
            },
            (int windowSize, int hopSize) => {
                TsvrApplication.DebugLogger.Log("Window Parameters changed " + windowSize.ToString() + " | Hop Size: " + hopSize.ToString(), "[GrainModel]");
                // Here we need to recalculate all grains
                ClearGrains(); // <- instead, consider using pool, and returning to pool
            }
        );

        featureExtractor = new AudioFeatureExtractor(this.Parameters.WindowSize, this.Parameters.HopSize);

        ChangeState(GrainModelState.Unplaced);
    }

    void OnDisable() {
        grainModelPlayback.StopAllCoroutines();
        grainModelPlayback = null;
        T_FeatureExtractor?.Abort();
    }

    // DEGUGGING - remove me!
    public void PlaySequentially() {
        StartCoroutine(PlayGrainsSequentially());
    }

    IEnumerator PlayGrainsSequentially()
    {
        int seqIndex = 0;
        while(true) {
            if (Grains.Count == 0) {
                yield return null; 
            } else {
                Grains[seqIndex].PlayGrain();
                seqIndex = (seqIndex + 1) % Grains.Count;
                // for (int i = 0; i < 10; i++) {
                //     // print($"playing grain {seqIndex}");
                // }
                yield return new WaitForSeconds(seqPlayRate);
            }
        }
    }

    /// <summary>
    /// Set the grain model's audio samples and trigger audio analysis and grain spawning routines
    /// </summary>
    public void SetAudioBuffer(DiscreteSignal audioBuffer) {
        this.AudioBuffer = audioBuffer;
        Debug.Log("=> Setting audio buffer for " + gameObject.name + " with " + audioBuffer.Length + " samples");
        grainModelPlayback.SetAudioBuffer(audioBuffer);
        ClearGrains();
        T_FeatureExtractor = new Thread(() => {
            var selectedFeatures = this.Parameters.CurrentlySelected();
            featureExtractor.ExtractFeatures(
                audioBuffer, 
                selectedFeatures, 
                () => {
                    for(int i = 0; i < featureExtractor.FeatureValues[selectedFeatures[0]].Length; i++) {
                        grainAudioFeaturesQueue.Enqueue(new GrainAudioFeatures(featureExtractor, i));
                    }
                }
            );
            print($"Completed audio feature analysis");
        });
        T_FeatureExtractor.Start();
        StartCoroutine(SpawnGrainsFromQueue());
    }

    /// <summary>
    /// Remove all grains from the model
    /// </summary>
    void ClearGrains() {
         // consider using pool, and returning to pool
        foreach(Grain grain in Grains)
            Destroy(grain.gameObject);
        Grains.Clear();
    }

    /// <summary>
    /// Watch a thread safe queue and wait for new grains to enter, spawning them as they are enqued
    /// </summary>
    IEnumerator SpawnGrainsFromQueue(int batchSize = 16) {
        while (grainAudioFeaturesQueue.Count == 0)
            yield return null;
       
        Debug.Log($"Spawning grains, total count: {grainAudioFeaturesQueue.Count}");
        while (grainAudioFeaturesQueue.Count > 0) {
            for (int i = 0; i < batchSize; i++) {
                if (grainAudioFeaturesQueue.TryDequeue(out GrainAudioFeatures gf)) {
                    SpawnGrain(gf);
                } else {
                    break;
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// Spawn a single grain object into the model given a GrainAudioFeatures object
    /// </summary>
    public void SpawnGrain(GrainAudioFeatures features)
    {
        GameObject grainObject = Instantiate(TsvrApplication.Config.grainPrefab, transform);
        Grain grain = grainObject.GetComponent<Grain>();
        grain.Initialize(features, Parameters, grainModelPlayback.RegisterPlaybackEvent);
        Grains.Add(grain);
    }


    public void ChangeState(GrainModelState state) {
        State = state;
        switch (state) {
            case GrainModelState.Unplaced:
                SetBBoxColor(colorUnplaced, 2f);
                break;
            case GrainModelState.Placed:
                coroutineManager.Freeze();
                SetBBoxColor(colorPlaced, 3f);
                break;
        }
    }


    public void SetBBoxColor(Color color, float duration = 1f, float delay = 0f) {
        Color currentColor = boundingBox.GetComponent<Renderer>().material.color;
        if (Color.Equals(currentColor, color)) return;
        coroutineManager.TimedAction("bbox-opacity", 
            (progress) => {
                Color currentColor = boundingBox.GetComponent<Renderer>().material.color;
                boundingBox.GetComponent<Renderer>().material.color = Color.Lerp(currentColor, color, progress);
            },
            onComplete : () => {
                boundingBox.GetComponent<Renderer>().material.color = color;
            },
            duration : duration,
            delay : delay
        );
    }
}




// add check for framerate, apportion proper number of updates per frame
// based on average delta time
// this way, each grain model is responsible for updating its own grains
// in a way that doesn't affect performance
// int grainsUpdated = 0;

// while (grainsUpdated < grains.Count && 1.0/Time.deltaTime >= TsvrApplication.Settings.targetFramerate) {
//     grains[updateIdx].UpdateAppearance();
//     grainsUpdated++;
//     updateIdx++;
//     updateIdx %= grains.Count;
// }

// // condition where nothing has been updated
// if (grainsUpdated < 64)
//     Debug.Log("[!] GrainsUpdated: " + grainsUpdated +  " | frame rate: " + 1.0/Time.deltaTime + " is under target (" + TsvrApplication.Settings.targetFramerate + ")");


// void OnFeatureUpdate(AudioFeature feature) {
//     foreach(Grain grain in grains)
//         grain.UpdatePosition(x_F, y_F, z_F, ax_scale);
// }

// change the positions of all grains to new feature ordering
// void UpdateFeaturePositions(string x_F, string y_F, string z_F, Vector3 ax_scale)
// {
//     foreach(Grain grain in grains)
//         grain.UpdatePosition(parameters.XFeature, y_F, z_F, ax_scale);
// }

// void UpdateFeatureScales(AudioFeature feature, float scale)
// {
//     foreach (Grain grain in grains)
//         grain.UpdateScale(sc_F, sc_F, sc_F, scale);
// }

// void UpdateFeatureColors(string r_F, string g_F, string b_F, bool hsv=false)
// {
//     foreach (Grain grain in grains)
//         grain.UpdateColor(r_F, g_F, b_F, hsv);
// }