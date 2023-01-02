using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Collections.Concurrent;
using System;
using NWaves.Signals;
using System.Threading.Tasks;


// ideas - consider making an object pool for GrainModel, to reduce CPU load
// when loading a new audio file => https://learn.unity.com/tutorial/introduction-to-object-pooling#5ff8d015edbc2a002063971d

/// <summary>
/// Manager and container for a collection of playable grains
/// </summary>
public class GrainModel : MonoBehaviour {
    public float ModelScale { get => transform.localScale.x; set => targetScale = Vector3.one * value; }
    public DiscreteSignal AudioBuffer { get; protected set; }
    public List<Grain> Grains { get; protected set; }


    private float lerpSpeed = 15f;
    private float grainScale = 2f;
    
    // eventually this will be in its own constellation class
    public float seqPlayRate = 1f;
    private bool useHSV = false;

    private Vector3 targetPosition;
    private Vector3 targetScale;
    private Quaternion targetRotation;

    private GrainModelParameters parameters;
    private Vector3 posAxisScale = Vector3.one;

    Thread T_procAudio = null;
    ConcurrentQueue<GrainFeatures> grainFeaturesQueue = new ConcurrentQueue<GrainFeatures>();
    ConcurrentQueue<GrainAudioFeatures> grainAudioFeaturesQueue = new ConcurrentQueue<GrainAudioFeatures>();

    private GrainModelPlayback grainModelPlayback;
    public GameObject boundingBox;

    private CoroutineManager coroutineManager;

    /// <summary>
    /// Main initialization for newly spawned GrainModel
    /// </summary>
    public void Initialize(Vector3 spawnPos, float modelScale=0.3f)
    {
        transform.position = spawnPos;
        targetPosition = spawnPos;
        targetRotation = transform.rotation;
        ModelScale = modelScale;
    }

    /// ===== MonoBehaviours ===========================================================

    void OnEnable() {
        grainModelPlayback = gameObject.AddComponent<GrainModelPlayback>();
        coroutineManager = new CoroutineManager(this);

        Grains = new List<Grain>();
        // Initialize parameter callbacks which will update the grains when changed
        this.parameters = new GrainModelParameters(
            (AudioFeature[] features) => {
                Debug.Log("[GrainModel] Display Parameters changed " + features.ToString());
                foreach(Grain grain in Grains)
                    grain.UpdatePosition(features[0], features[1], features[2], posAxisScale);
            }, 
            (AudioFeature[] features) => {
                Debug.Log("[GrainModel] Display Parameters changed " + features.ToString());
                foreach(Grain grain in Grains)
                    grain.UpdateColor(features[0], features[1], features[2], useHSV);
            },
            (AudioFeature sclFeature) => {
                Debug.Log("[GrainModel] Display Parameters changed " + sclFeature.ToString());
                foreach(Grain grain in Grains)
                    grain.UpdateScale(sclFeature, grainScale);
            },
            (int windowSize, int hopSize) => {
                Debug.Log("[GrainModel] Window Parameter Changed - Window Size: "
                        + windowSize + " Hop Size: " + hopSize);
                // Here we need to recalculate all grains
                ClearGrains(); // <- instead, consider using pool, and returning to pool
            }
        );
        // boundingBox.transform.localScale = Vector3.one * ModelScale;
        coroutineManager.TimedAction("bbox-scale",
            (progress) => {
                boundingBox.transform.localScale = Vector3.one * ModelScale * progress * 2f;
            }, 0.5f
        );
    }

    void OnDisable() {
        T_procAudio?.Abort();
    }

    void Update()
    {
        if(!Vector3.Equals(transform.position, targetPosition))
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * lerpSpeed);
            float distance = Vector3.Distance(transform.position, targetPosition);
            Debug.Log("Moving grain model towards target position | distance: " + distance);
            if (distance < 0.1f) transform.position = targetPosition;
        }

        if(!Quaternion.Equals(transform.rotation, targetRotation))
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * lerpSpeed);
            float angle = Quaternion.Angle(transform.rotation, targetRotation);
            Debug.Log("Rotating grain model towards target rotation | angle: " + angle + " degrees");
            if (angle < 0.1f) transform.rotation = targetRotation;
        }

        if (!Vector3.Equals(transform.localScale, targetScale))
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, targetScale, Time.deltaTime * lerpSpeed);
            float distance = Vector3.Distance(transform.localScale, targetScale);
            Debug.Log("Scaling grain model towards target scale | distance: " + distance);
            if (distance < 0.1f) transform.localScale = targetScale;
        }

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
                print($"playing grain {seqIndex}");
                Grains[seqIndex].PlayGrain();
                seqIndex = (seqIndex + 1) % Grains.Count;
                yield return new WaitForSeconds(seqPlayRate);
            }
        }
    }

    public void SetAudioBuffer(DiscreteSignal audioBuffer) {
        this.AudioBuffer = audioBuffer;
        grainModelPlayback.SetAudioBuffer(audioBuffer);
        ClearGrains();
        T_procAudio = new Thread(() => {
            AudioFeatureExtractor extractor = new AudioFeatureExtractor(this.parameters.WindowSize, this.parameters.HopSize);
            extractor.ExtractFeatures(
                audioBuffer, 
                this.parameters.CurrentlySelected(), 
                (GrainAudioFeatures gf) => { grainAudioFeaturesQueue.Enqueue(gf); }                
            );
            print($"Completed audio feature analysis");
        });
        T_procAudio.Start();
        StartCoroutine(SpawnGrainsFromQueue());
    }

    ///<summary> 
    /// threaded audio processing and coroutines to spawn grains
    /// </summary>
    void LoadGrainsFromAudioFile(string audioPath, int windowSize, int hop)
    {
        Debug.Log("Spawning grains from audio file: " + audioPath + " | window size: " + windowSize + " | hop size: " + hop);

        T_procAudio = new Thread(() => {
            Debug.Log($"Starting audio feature analysis for {audioPath}");
            AudioBuffer = AudioIO.ReadMonoAudioFile(audioPath);
            AudioFeatureExtractor extractor = new AudioFeatureExtractor(windowSize, hop);
            extractor.ExtractFeatures(
                AudioBuffer, 
                this.parameters.CurrentlySelected(), 
                (GrainAudioFeatures gf) => { grainAudioFeaturesQueue.Enqueue(gf); }                
            );
            print($"Completed audio feature analysis for {audioPath}");
        });
        T_procAudio.Start();
        StartCoroutine(SpawnGrainsFromQueue());
    }

    void ClearGrains() {
         // consider using pool, and returning to pool
        foreach(Grain grain in Grains)
            Destroy(grain.gameObject);
        Grains.Clear();
    }

    // watch for grains to enter queue
    IEnumerator SpawnGrainsFromQueue(int batchSize = 16) {
        while (grainAudioFeaturesQueue.Count == 0)
            yield return null;
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

    //moves grain model to a location, and rotates to look at a point
    public void MoveLookAt(Vector3 lookAtPos, Vector3 _targetPosition)
    {
        targetRotation = Quaternion.LookRotation(lookAtPos);
        targetPosition = _targetPosition;
    }

    void SpawnGrain(GrainAudioFeatures features)
    {
        GameObject grainObject = Instantiate(TsvrApplication.Config.grainPrefab, transform);
        Grain grain = grainObject.GetComponent<Grain>();
        // Debug.Log("Spawning Grain with RMS feature: " + features.Get(AudioFeature.RMS).ToString());
        grain.Initialize(features, parameters, grainModelPlayback.RegisterPlaybackEvent);
        Grains.Add(grain);
    }

    // IEnumerator 
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