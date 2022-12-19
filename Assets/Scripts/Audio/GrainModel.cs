using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
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
    private int _hopSize = 512;
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
}


// ideas - consider making an object pool for GrainModel, to reduce CPU load
// when loading a new audio file => https://learn.unity.com/tutorial/introduction-to-object-pooling#5ff8d015edbc2a002063971d

/// <summary>
/// Manager and container for a collection of playable grains
/// </summary>
public class GrainModel : MonoBehaviour {
    private float lerpSpeed = 15f;
    private float grainScale = 2f;
    private float seqPlayRate = 1;
    private bool useHSV = false;

    private Vector3 targetPosition;
    private Quaternion targetRotation;

    public GrainModelParameters parameters;
    public Vector3 posAxisScale = Vector3.one;

    private List<Grain> grains;
    Thread T_procAudio = null;
    Thread T_micAudio = null;
    GrainFeatures[] m_AllGrainFeatures;


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
            }
        );
    }
    
    public void Initialize(Vector3 spawnPos, string audioPath=null)
    {
        transform.position = spawnPos;
        targetPosition = spawnPos;
        targetRotation = transform.rotation;
        
        // if (audioPath != null)
        //     LoadGrains(audioPath, m_FrameSize, m_Hop);
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

        // add check for framerate, apportion proper number of updates per frame
        // based on average delta time
        foreach(Grain grain in grains)
            grain.Update();
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

    // threaded audio processing and coroutines to spawn grains
    void LoadGrains(string audioPath, int frameSize, int hop)
    {
        // asynchronously load grains | Consider using Unity Job System for improved performance
        T_procAudio = new Thread(() => ProcessAudioFeatures(audioPath, frameSize, hop));
        T_procAudio.Start();
        StartCoroutine(SpawnGrainsAsync());
        //SpawnGrainsFromFeatures(GrainFeatures[] grainFeatures) // << make this a coroutine which waits for new grainFeatures
    }

    /// <summary>
    /// Asynchronously spawn grains
    /// </summary>
    IEnumerator SpawnGrainsAsync()
    {
        while (m_AllGrainFeatures == null)
            yield return new WaitForEndOfFrame();
        int index = 0;
        float opTime = Time.realtimeSinceStartup;
        foreach (GrainFeatures gf in m_AllGrainFeatures)
        {
            index++;
            if (gf == null)
                continue;
            SpawnGrain(gf);
            if (Time.realtimeSinceStartup > opTime + 60.0f) {
                yield return null;
                opTime = Time.realtimeSinceStartup;
            } else {
                yield return new WaitForSeconds(0.001f);
            }
        }
    }

    //moves grain model to a location, and rotates to look at a point
    public void MoveLookAt(Vector3 lookAtPos, Vector3 _targetPosition)
    {
        targetRotation = Quaternion.LookRotation(lookAtPos);
        targetPosition = _targetPosition;
    }

    void ProcessAudioFeatures(string audioPath, int frameSize, int hop)
    {
        AudioFeatures audioFeatures = new AudioFeatures();
        print($"Computing features for {audioPath}");
        m_AllGrainFeatures = audioFeatures.GenerateAudioFeatures(audioPath, frameSize, hop, 26);
        print($"Completed audio feature analysis for {audioPath}");
    }

    void SpawnGrain(GrainFeatures features)
    {
        Grain grain = Instantiate(TsvrApplication.Config.Grain, transform) as Grain;
        grains.Add(grain);

        // GameObject grainObject = Instantiate(TsvrApplication.Config.grainPrefab, transform);
        // what do we gain from a scriptable object? 
        // - no update function: we can manually control updates of each grain from this controller
        //   - this possibly gives us greater performance control (tbd)
        //   - each grain object's prefab will also have performance optimizing
        //     aspects like LODs

        // What do we lose from a scriptable object?
        // - Unity automatic updates per object, possibly optimizing performance
        //   - see if there is a performance difference between the two approaches
        
        //Grain grain = grainObject.AddComponent<Grain>();
        // Grain grain = grainObject.GetComponent<Grain>();
        // grain.Initialize(
        //     new Vector3(gf.features[x_Feature], gf.features[y_Feature], gf.features[z_Feature]),
        //     new Vector3(gf.features[scale_Feature], gf.features[scale_Feature], gf.features[scale_Feature]),
        //     gf,
        //     $"grain_{grains.Count}"
        // );
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

        //float buffSize = 1 / (float)sampleRate * 256;

        audioSource.clip = Microphone.Start(null, true, 1, sampleRate);

        while(!(Microphone.GetPosition(null) > 0))
        {
            //float[] micSamples = audioSource.clip.GetData()
        }
    }

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
}
