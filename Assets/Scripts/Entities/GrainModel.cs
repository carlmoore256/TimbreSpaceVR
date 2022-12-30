using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Collections.Concurrent;
using System;

/// <summary>
/// State manager for GrainModel parameters, invoking callbacks when parameters are changed
/// </summary>
public class GrainModelParameters {
    private Action<AudioFeature[]> onFeaturePosUpdate;
    private Action<AudioFeature[]> onFeatureColUpdate;
    private Action<AudioFeature> onFeatureSclUpdate;
    private Action<int, int> onWindowUpdate; // window size, hop size

    private int _windowSize = 4096;
    private int _hopSize = 2048;
    public int WindowSize { get => _windowSize; set { 
        _windowSize=value;
        onWindowUpdate(value, _hopSize); 
    } }
    
    public int HopSize { get => _hopSize; set { 
        _hopSize=value;
        onWindowUpdate(_windowSize, value); 
    } }

    private AudioFeature _XFeature  = AudioFeature.MFCC_0;
    private AudioFeature _YFeature = AudioFeature.MFCC_1;
    private AudioFeature _ZFeature = AudioFeature.MFCC_2;
    private AudioFeature _RFeature = AudioFeature.MFCC_3;
    private AudioFeature _GFeature = AudioFeature.MFCC_4;
    private AudioFeature _BFeature = AudioFeature.MFCC_5;
    private AudioFeature _ScaleFeature = AudioFeature.RMS;

    public AudioFeature[] PositionFeatures { get => new AudioFeature[3] { _XFeature, _YFeature, _ZFeature }; }
    public AudioFeature[] ColorFeatures { get => new AudioFeature[3] { _RFeature, _GFeature, _BFeature }; }

    public AudioFeature XFeature { get => _XFeature; set {
        _XFeature = value;
        // AudioFeature[] features = new AudioFeature[3] { value, _YFeature, _ZFeature };
        onFeaturePosUpdate(PositionFeatures);
    } }

    public AudioFeature YFeature { get => _YFeature; set {
        _YFeature = value;
        onFeaturePosUpdate(PositionFeatures);
    } }

    public AudioFeature ZFeature { get => _ZFeature; set {
        _ZFeature = value;
        onFeaturePosUpdate(PositionFeatures);
    } }

    public AudioFeature RFeature { get => _RFeature; set {
        _RFeature = value;
        onFeatureColUpdate(ColorFeatures);
    } }

    public AudioFeature GFeature { get => _GFeature; set {
        _GFeature = value;
        onFeatureColUpdate(ColorFeatures);
    } }

    public AudioFeature BFeature { get => _BFeature; set {
        _BFeature = value;
        onFeatureColUpdate(ColorFeatures);
    } }

    public AudioFeature ScaleFeature { get => _ScaleFeature; set {
        _ScaleFeature = value;
        onFeatureSclUpdate(value);
    } }

    public GrainModelParameters(
            Action<AudioFeature[]> onFeaturePosUpdate, 
            Action<AudioFeature[]> onFeatureColUpdate,
            Action<AudioFeature> onFeatureSclUpdate,
            Action<int, int> onWindowUpdate)
    { 
        this.onFeaturePosUpdate = onFeaturePosUpdate;
        this.onFeatureColUpdate = onFeatureColUpdate;
        this.onFeatureSclUpdate = onFeatureSclUpdate;
        this.onWindowUpdate = onWindowUpdate;
    }

    /** Set window size by the number of hops per window */
    public void SetWindowByHops(int hopCount) {
        HopSize = (int)(WindowSize /(float)hopCount);
    }

    public AudioFeature[] CurrentlySelected() {
        return new AudioFeature[7] {
            _XFeature, _YFeature, _ZFeature,
            _RFeature, _GFeature, _BFeature,
            _ScaleFeature
        };
    }
}



// ideas - consider making an object pool for GrainModel, to reduce CPU load
// when loading a new audio file => https://learn.unity.com/tutorial/introduction-to-object-pooling#5ff8d015edbc2a002063971d

/// <summary>
/// Manager and container for a collection of playable grains
/// </summary>
public class GrainModel : MonoBehaviour {
    public float ModelScale { get => transform.localScale.x; set => targetScale = Vector3.one * value; }
    private float lerpSpeed = 15f;
    private float grainScale = 2f;
    private float seqPlayRate = 1f;
    private bool useHSV = false;

    private Vector3 targetPosition;
    private Vector3 targetScale;
    private Quaternion targetRotation;

    public GrainModelParameters parameters;
    public Vector3 posAxisScale = Vector3.one;

    private List<Grain> grains;
    Thread T_procAudio = null;
    Thread T_micAudio = null;
    private bool SPAWN_CONCURRENT = false;
    ConcurrentQueue<GrainFeatures> grainFeaturesQueue = new ConcurrentQueue<GrainFeatures>();
    ConcurrentQueue<GrainAudioFeatures> grainAudioFeaturesQueue = new ConcurrentQueue<GrainAudioFeatures>();

    void OnEnable() {
        grains = new List<Grain>();
        // Initialize parameter callbacks which will update the grains when changed
        this.parameters = new GrainModelParameters(
            (AudioFeature[] features) => {
                Debug.Log("[GrainModel] Display Parameters changed " + features.ToString());
                foreach(Grain grain in grains)
                    grain.UpdatePosition(features[0], features[1], features[2], posAxisScale);
            }, 
            (AudioFeature[] features) => {
                Debug.Log("[GrainModel] Display Parameters changed " + features.ToString());
                foreach(Grain grain in grains)
                    grain.UpdateColor(features[0], features[1], features[2], useHSV);
            },
            (AudioFeature sclFeature) => {
                Debug.Log("[GrainModel] Display Parameters changed " + sclFeature.ToString());
                foreach(Grain grain in grains)
                    grain.UpdateScale(sclFeature, grainScale);
            },
            (int windowSize, int hopSize) => {
                Debug.Log("[GrainModel] Window Parameter Changed - Window Size: "
                        + windowSize + " Hop Size: " + hopSize);
                // Here we need to recalculate all grains
                ClearGrains(); // <- instead, consider using pool, and returning to pool
                
            }
        );
    }

    void OnDisable() {
        T_procAudio?.Abort();
    }
    
    public void Initialize(Vector3 spawnPos, float modelScale=0.3f, string audioPath=null)
    {
        transform.position = spawnPos;
        targetPosition = spawnPos;
        targetRotation = transform.rotation;
        ModelScale = modelScale;
        
        if (audioPath != null)
            LoadGrains(audioPath, parameters.WindowSize, parameters.HopSize);
        // StartCoroutine(PlayGrainsSequentially());
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

    IEnumerator PlayGrainsSequentially()
    {
        while(true) {
            foreach(Grain grain in grains) {
                grain.PlayGrain();
                print($"playing grain {grain}");
                yield return new WaitForSeconds(seqPlayRate);
            }
            yield return new WaitForSeconds(seqPlayRate);
        }
    }

    ///<summary> 
    /// threaded audio processing and coroutines to spawn grains
    /// </summary>
    void LoadGrains(string audioPath, int windowSize, int hop)
    {
        Debug.Log("Spawning grains from audio file: " + audioPath + " | window size: " + windowSize + " | hop size: " + hop);
        // asynchronously load grains | Consider using Unity Job System for improved performance
        T_procAudio = new Thread(() => {
            AudioFeatures audioFeatures = new AudioFeatures();
            print($"Computing features for {audioPath}");
            var audio = AudioIO.ReadMonoAudioFile(audioPath);

            AudioFeatureExtractor extractor = new AudioFeatureExtractor(windowSize, hop);
            extractor.ExtractFeatures(
                audio, 
                this.parameters.CurrentlySelected(), 
                (GrainAudioFeatures gf) => {
                    Debug.Log($"Got Grain Audio Features: {gf} | {gf.Get(AudioFeature.MFCC_0)} | Enqueuing...");
                    grainAudioFeaturesQueue.Enqueue(gf);
                }
            );
            
            // if (SPAWN_CONCURRENT) {
            //     audioFeatures.ComputeFeatureBlocks(audio, windowSize, hop, 8, 16, (GrainFeatures[] grainFeatures) => {
            //         foreach(GrainFeatures gf in grainFeatures) {
            //             grainFeaturesQueue.Enqueue(gf);
            //         }
            //     });
            // } else {
            //     foreach(GrainFeatures gf in audioFeatures.GenerateAudioFeatures(audio, windowSize, hop, 8)) {
            //         grainFeaturesQueue.Enqueue(gf);
            //     };
            // }
            print($"Completed audio feature analysis for {audioPath}");
        });
        T_procAudio.Start();
        // StartCoroutine(SpawnGrainsFromQueue());
        StartCoroutine(SpawnAudioGrainsFromQueue());
    }

    void ClearGrains() {
         // consider using pool, and returning to pool
        foreach(Grain grain in grains)
            Destroy(grain.gameObject);
        grains.Clear();
    }

    /// <summary>
    /// Spawns grains from a queue of grain features
    /// </summary>
    IEnumerator SpawnGrainsFromQueue() {
        while(true) {
            // while (grainFeaturesQueue.TryDequeue(out GrainFeatures gf)) {
            //     SpawnGrain(gf);
            //     yield return null;
            // }
            yield return null;
        }
    }


    IEnumerator SpawnAudioGrainsFromQueue() {
        while(true) {
            while (grainAudioFeaturesQueue.TryDequeue(out GrainAudioFeatures gf)) {
                SpawnGrain(gf);
                yield return null;
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
        Debug.Log("Spawning Grain with RMS feature: " + features.Get(AudioFeature.RMS).ToString());
        grain.Initialize(features, parameters);
        grains.Add(grain);
    }

    void StartAudioInput()
    {
        T_micAudio = new Thread(() => AudioInputThread());
        T_micAudio.Start();
    }

    void AudioInputThread()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        int sampleRate = AudioSettings.GetConfiguration().sampleRate;
        audioSource.clip = Microphone.Start(null, true, 1, sampleRate);
        while(!(Microphone.GetPosition(null) > 0))
        {
            //float[] micSamples = audioSource.clip.GetData()
        }
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