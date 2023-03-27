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
    Placed,
    Positioning,
    Playable
}

// ideas - consider making an object pool for GrainModel, to reduce CPU load
// when loading a new audio file => https://learn.unity.com/tutorial/introduction-to-object-pooling#5ff8d015edbc2a002063971d

/// <summary>
/// Manager and container for a collection of playable grains
/// </summary>
public class GrainModel : MonoBehaviour {
    // public float ModelScale { get => transform.localScale.x; set => TargetScale = Vector3.one * value; }
    public bool HasBeenPlaced { get; protected set; } = false;

    public DiscreteSignal AudioBuffer { get; protected set; }
    public List<Grain> Grains { get; protected set; }
    public GrainModelState State { get; set; } = GrainModelState.Unplaced;
    public GrainModelParameters Parameters { get; protected set; }

    public Color colorProcessing = new Color(222, 92, 0, 0.6f);
    public Color colorPlayable = new Color(255, 255, 255, 0.0f);
    public Color colorPositioning = new Color(255, 255, 255, 1f);
    public Color colorInspecting = new Color(255, 255, 255, 1f);

    private float grainScale = 2f;
    
    // eventually this will be in its own constellation class
    public float seqPlayRate = 0.1f;
    private bool useHSV = false;
    private Vector3 posAxisScale = Vector3.one;
    Thread T_FeatureExtractor = null;
    private ConcurrentQueue<GrainAudioFeatures> grainAudioFeaturesQueue = new ConcurrentQueue<GrainAudioFeatures>();

    private GrainModelPlayback grainModelPlayback;
    public GameObject boundingBox;

    public TransformCoroutineManager coroutineManager;
    private AudioFeatureExtractor featureExtractor;


    /// ===== MonoBehaviours ===========================================================

    private void OnEnable() {

        grainModelPlayback = gameObject.AddComponent<GrainModelPlayback>();
        coroutineManager = new TransformCoroutineManager(this, () => {
            Debug.Log("Sending Spring Toggle broadcast message -> OFF");
            BroadcastMessage("ToggleReposition", true, SendMessageOptions.DontRequireReceiver);
            // BroadcastMessage("ToggleSpring", false, SendMessageOptions.DontRequireReceiver);
        }, () => {
            Debug.Log("Sending Spring Toggle broadcast message -> ON");
            if (HasBeenPlaced)
                BroadcastMessage("ToggleReposition", false, SendMessageOptions.DontRequireReceiver);
                // BroadcastMessage("ToggleSpring", true, SendMessageOptions.DontRequireReceiver);
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

    private void OnDisable() {
        grainModelPlayback.StopAllCoroutines();
        grainModelPlayback = null;
        T_FeatureExtractor?.Abort();
    }

    // DEGUGGING - remove me!
    public void PlaySequentially() {
        StartCoroutine(PlayGrainsSequentially());
    }

    private IEnumerator PlayGrainsSequentially()
    {
        int seqIndex = 0;
        while(true) {
            if (Grains.Count == 0) {
                yield return null; 
            } else {
                Grains[seqIndex].PlayGrain();
                seqIndex = (seqIndex + 1) % Grains.Count;
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
    private void ClearGrains() {
         // consider using pool, and returning to pool
        foreach(Grain grain in Grains)
            Destroy(grain.gameObject);
        Grains.Clear();
    }

    /// <summary>
    /// Watch a thread safe queue and wait for new grains to enter, spawning them as they are enqued
    /// </summary>
    private IEnumerator SpawnGrainsFromQueue(int batchSize = 16) {
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
    private void SpawnGrain(GrainAudioFeatures features)
    {
        GameObject grainObject = Instantiate(TsvrApplication.Config.grainPrefab, transform);
        Grain grain = grainObject.GetComponent<Grain>();
        grain.Initialize(features, Parameters, grainModelPlayback.RegisterPlaybackEvent);
        grain.TriggerPlayAnimation(); // make a nice animation when grain appears
        Grains.Add(grain);
    }

    public void Inspect() {
        boundingBox.GetComponent<Renderer>().material.color = colorInspecting;
        coroutineManager.TimedAction("inspect-timeout", null, Time.deltaTime * 2f, null, () => {
            SetBBoxColor(colorPlayable, 0.2f);
        });
    }

    public void Place() {
        ChangeState(GrainModelState.Playable);
        HasBeenPlaced = true;
        BroadcastMessage("ToggleReposition", false, SendMessageOptions.DontRequireReceiver);
    }

    public void Reposition(Vector3 position, Quaternion rotation, Vector3 scale, float duration = 0.5f) {
        coroutineManager.MoveTo(position, duration);
        coroutineManager.RotateTo(rotation, duration);
        coroutineManager.ScaleTo(scale, duration);
    }

    public void Reposition(Vector3 position, float duration = 0.5f) {
        coroutineManager.MoveTo(position, duration);
    }

    // eventually make this private
    public void ChangeState(GrainModelState newState) {
        var currentState = State;

        switch (newState) {
            case GrainModelState.Positioning:
                SetBBoxColor(colorPositioning, 0.3f);
                break;
            case GrainModelState.Playable:
                coroutineManager.Freeze();
                SetBBoxColor(colorPlayable, 0.3f, 1f);
                break;
            // case GrainModelState.Unplaced:
            //     SetBBoxColor(colorUnplaced, 2f);
            //     break;
            // case GrainModelState.Placed:
            //     coroutineManager.Freeze();
            //     SetBBoxColor(colorPlaced, 3f);
            //     break;
        }
        State = newState;
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

    /// <summary>
    /// Spawn a grain model from a TsvrSamplePack sample. Sample packs are installed in resources
    /// </summary>
    public static GrainModel SpawnFromSample(TsvrSamplePackMetadata pack, TsvrSample sample, Vector3 position, Quaternion rotation) {
        GameObject grainModelObject = Instantiate(Resources.Load<GameObject>("Prefabs/GrainModel"), GameObject.Find("GrainParent").transform);
        grainModelObject.transform.position = position;
        grainModelObject.transform.rotation = rotation;
        GrainModel grainModel = grainModelObject.GetComponent<GrainModel>();
        var sampleFileName = sample.file.Replace(".wav", "");
        AudioIO.LoadAudioFromAssets($"SamplePacks/{pack.id}/{sampleFileName}", (signal) => {
            int sigLen = signal.Length;
            // signal = AudioUtils.FilterSignalRMS(signal, TsvrApplication.Settings.AudioDbThreshold, TsvrApplication.Settings.AudioDbThresholdWindow);
            signal = AudioFilters.StripSilence(signal, -30, 1024);
            // Debug.Log($"Filtered out {sigLen - signal.Length} samples below threshold of {TsvrApplication.Settings.AudioDbThreshold}dB");
            Debug.Log($"Filtered out {sigLen - signal.Length} samples below threshold of {-30}dB");
            grainModel.SetAudioBuffer(signal);
        });
        return grainModel;
    }

    /// <summary>
    /// Spawn a grain model directly from a wav or mp3 file
    /// </summary>
    public static GrainModel SpawnFromFile(string filePath, Vector3 position, Quaternion rotation) {
        // GameObject grainModelObject = Instantiate(TsvrApplication.Config.grainModel, GameObject.Find("GrainParent").transform);
        GameObject grainModelObject = Instantiate(Resources.Load<GameObject>("Prefabs/GrainModel"), GameObject.Find("GrainParent").transform);
        grainModelObject.transform.position = position;
        grainModelObject.transform.rotation = rotation;
        GrainModel grainModel = grainModelObject.GetComponent<GrainModel>();
        // AudioIO.LoadAudioFromAssets(filePath, (signal) => {
        //     grainModel.SetAudioBuffer(signal);
        // });
        return grainModel;
    }


    /// <summary>
    /// Spawn a grain model from a directory containing audio files. The size of the grains will be optimized for grain count
    /// </summary>
    public static GrainModel SpawnFromFolder(string folderPath, Vector3 position, Quaternion rotation) {
        // GameObject grainModelObject = Instantiate(TsvrApplication.Config.grainModel, GameObject.Find("GrainParent").transform);
        GameObject grainModelObject = Instantiate(Resources.Load<GameObject>("Prefabs/GrainModel"), GameObject.Find("GrainParent").transform);
        grainModelObject.transform.position = position;
        grainModelObject.transform.rotation = rotation;
        GrainModel grainModel = grainModelObject.GetComponent<GrainModel>();
        // AudioIO.LoadAudioFromAssets(filePath, (signal) => {
        //     grainModel.SetAudioBuffer(signal);
        // });
        return grainModel;
    }
}
