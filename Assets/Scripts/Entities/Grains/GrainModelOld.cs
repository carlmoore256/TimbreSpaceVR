using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;
using System;
using NWaves.Signals;
using System.Threading.Tasks;
using UnityEngine;


// public enum GrainModelState {
//     Unplaced,
//     Placed,
//     Positioning,
//     Playable
// }


/// <summary>
/// Manager and container for a collection of playable grains
/// </summary>
public class GrainModelOld : MonoBehaviour
{
    public bool HasBeenPlaced { get; protected set; } = false;

    public List<GrainOld> Grains { get; protected set; } 
    public GrainModelState State { get; set; } = GrainModelState.Unplaced;
    public DiscreteSignal AudioBuffer { get; protected set; }
    public GameObject boundingBox;
    public Color colorPlayable = new Color(255, 255, 255, 0.0f);
    public Color colorInspecting = new Color(255, 255, 255, 1f);
    public Color colorPositioning = new Color(255, 255, 255, 1f);

    private GranularParameterHandler parameterHandler;
    private PolyvoicePlayer grainModelPlayback;
    private AudioFeatureAnalyzer featureExtractor;
    private TransformCoroutineManager coroutineManager;
    private ConcurrentQueue<GrainAudioFeatures> computedFeatures = new ConcurrentQueue<GrainAudioFeatures>();
    private Thread tFeatureExtractor;


    # region Lifecycle
    private void OnEnable() {
        Grains = new List<GrainOld>();

        grainModelPlayback = gameObject.AddComponent<PolyvoicePlayer>();

        coroutineManager = new TransformCoroutineManager(this, () => {
            TsvrApplication.DebugLogger.Log("Sending Spring Toggle broadcast message -> OFF", "[GrainModel]");
            BroadcastMessage("ToggleReposition", true, SendMessageOptions.DontRequireReceiver);
        }, () => {
            TsvrApplication.DebugLogger.Log("Sending Spring Toggle broadcast message -> ON", "[GrainModel]");
            if (HasBeenPlaced)
                BroadcastMessage("ToggleReposition", false, SendMessageOptions.DontRequireReceiver);
        });

        // Determine how grain parameter updates are handled
        this.parameterHandler = new GranularParameterHandler(
            new GranularParameterValues(),
            OnPositionParameterUpdate,
            OnColorParameterUpdate, 
            OnScaleParameterUpdate,
            OnWindowParameterUpdate
        );

        ChangeState(GrainModelState.Unplaced);
    }

    private void OnDisable() {
        grainModelPlayback.StopAllCoroutines();
        grainModelPlayback = null;
        tFeatureExtractor?.Abort();
    }

    # endregion


    # region Event Handlers
    private void OnPositionParameterUpdate(AudioFeature[] posFeatures, Vector3 axisScale) {
        if (this.AudioBuffer == null) return;
        TsvrApplication.DebugLogger.Log("Position Parameters changed " + posFeatures[0].ToString() + " " + posFeatures[1].ToString() + " " + posFeatures[2].ToString(), "[GrainModel]");
        featureExtractor.BatchComputeFeatures(this.AudioBuffer, posFeatures, () => { // make sure we compute any features that need to be computed
            foreach(GrainOld grain in Grains)
                grain.UpdatePosition(posFeatures[0], posFeatures[1], posFeatures[2], axisScale);
        });
    }

    private void OnColorParameterUpdate(AudioFeature[] colFeatures, bool useHSV) {
        if (this.AudioBuffer == null) return;
        TsvrApplication.DebugLogger.Log("Color Parameters changed " + colFeatures[0].ToString() + " " + colFeatures[1].ToString() + " " + colFeatures[2].ToString(), "[GrainModel]");
        featureExtractor.BatchComputeFeatures(this.AudioBuffer, colFeatures, () => {
            foreach(GrainOld grain in Grains)
                grain.UpdateColor(colFeatures[0], colFeatures[1], colFeatures[2], useHSV);
        });
    }

    private void OnScaleParameterUpdate(AudioFeature sclFeature) {
        if (this.AudioBuffer == null) return;
        TsvrApplication.DebugLogger.Log("Scale Parameters changed " + sclFeature.ToString(), "[GrainModel]");
        featureExtractor.BatchComputeFeatures(this.AudioBuffer, new AudioFeature[] {sclFeature}, () => {
            foreach(GrainOld grain in Grains)
                grain.UpdateScale(sclFeature, parameterHandler.ScaleMult, parameterHandler.ScaleExp);
        });
    }

    private void OnWindowParameterUpdate(int windowSize, int hopSize) {
        if (this.AudioBuffer == null) return;
        TsvrApplication.DebugLogger.Log("Window Parameters changed " + windowSize.ToString() + " | Hop Size: " + hopSize.ToString(), "[GrainModel]");
        // window size or hop size has been changed, so all features need to be recalculated
        featureExtractor = new AudioFeatureAnalyzer(this.parameterHandler.WindowSize, this.parameterHandler.HopSize);
        ClearGrains(); // <- instead, consider using pool, and returning to pool

    }

    # endregion


    # region Public Methods

    /// <summary>
    /// Set the grain model's audio samples and trigger audio analysis and grain spawning routines
    /// </summary>
    public void SetAudioBuffer(DiscreteSignal audioBuffer) {
        featureExtractor = new AudioFeatureAnalyzer(this.parameterHandler.WindowSize, this.parameterHandler.HopSize);
        this.AudioBuffer = audioBuffer;
        Debug.Log("=> Setting audio buffer for " + gameObject.name + " with " + audioBuffer.Length + " samples");
        grainModelPlayback.SetAudioBuffer(audioBuffer);
        ClearGrains();
        tFeatureExtractor = new Thread(() => {
            var selectedFeatures = this.parameterHandler.CurrentFeatures();
            featureExtractor.BatchComputeFeatures(
                audioBuffer, 
                selectedFeatures, 
                () => {
                    for(int i = 0; i < featureExtractor.FeatureVectors[selectedFeatures[0]].Length; i++) {
                        computedFeatures.Enqueue(new GrainAudioFeatures(featureExtractor, i));
                    }
                }
            );
            print($"Completed audio feature analysis");
        });
        tFeatureExtractor.Start();
        StartCoroutine(SpawnGrainsFromQueue());
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

    # endregion


    # region Private Methods
    private void ChangeState(GrainModelState newState) {
        var currentState = State;
        switch (newState) {
            case GrainModelState.Positioning:
                SetBBoxColor(colorPositioning, 0.3f);
                break;
            case GrainModelState.Playable:
                coroutineManager.Freeze();
                SetBBoxColor(colorPlayable, 0.3f, 1f);
                break;
        }
        State = newState;
    }

    /// <summary>
    /// Remove all grains from the model
    /// </summary>
    private void ClearGrains() {
         // consider using pool, and returning to pool
        foreach(GrainOld grain in Grains)
            Destroy(grain.gameObject);
        Grains.Clear();
    }

    /// <summary>
    /// Watch a thread safe queue and wait for new grains to enter, spawning them as they are enqued
    /// </summary>
    private IEnumerator SpawnGrainsFromQueue(int batchSize = 16) {
        while (computedFeatures.Count == 0)
            yield return null;
       
        Debug.Log($"Spawning grains, total count: {computedFeatures.Count}");
        while (computedFeatures.Count > 0) {
            for (int i = 0; i < batchSize; i++) {
                if (computedFeatures.TryDequeue(out GrainAudioFeatures gf)) {
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
        GrainOld grain = grainObject.GetComponent<GrainOld>();
        grain.Initialize(features, parameterHandler, grainModelPlayback.Play);
        grain.TriggerPlayAnimation(); // make a nice animation when grain appears
        Grains.Add(grain);
    }

    private void SetBBoxColor(Color color, float duration = 1f, float delay = 0f) {
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

    # endregion


    # region Static Methods

    /// <summary>
    /// Spawn a grain model from a TsvrSamplePack sample. Sample packs are installed in resources
    /// </summary>
    public static GrainModelOld SpawnFromSample(TsvrSamplePackMetadata pack, TsvrSample sample, Vector3 position, Quaternion rotation) {
        GameObject grainModelObject = Instantiate(Resources.Load<GameObject>("Prefabs/GrainModel"), GameObject.Find("GrainParent").transform);
        grainModelObject.transform.position = position;
        grainModelObject.transform.rotation = rotation;
        GrainModelOld grainModel = grainModelObject.GetComponent<GrainModelOld>();
        var sampleFileName = sample.file.Replace(".wav", "");

        AudioIO.LoadAudioFromAssets($"SamplePacks/{pack.id}/{sampleFileName}", (signal) => {
            int sigLen = signal.Length;
            var tcs = new TaskCompletionSource<DiscreteSignal>();
            var stripSilenceTask = Task.Run(() => {
                var strippedSignal = AudioFilters.StripSilence(signal, -30, 1024);
                tcs.SetResult(strippedSignal);
            });
            // Set the audio buffer when the task is completed
            stripSilenceTask.ContinueWith(task => {
                if (task.Status == TaskStatus.RanToCompletion) {
                    var strippedSignal = tcs.Task.Result;
                    Debug.Log($"Filtered out {sigLen - strippedSignal.Length} samples below threshold of {-30}dB");
                    grainModel.SetAudioBuffer(strippedSignal);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        });
        return grainModel;
    }

    /// <summary>
    /// Spawn a grain model directly from a wav or mp3 file
    /// </summary>
    public static GrainModelOld SpawnFromFile(string filePath, Vector3 position, Quaternion rotation) {
        // GameObject grainModelObject = Instantiate(TsvrApplication.Config.grainModel, GameObject.Find("GrainParent").transform);
        GameObject grainModelObject = Instantiate(Resources.Load<GameObject>("Prefabs/GrainModel"), GameObject.Find("GrainParent").transform);
        grainModelObject.transform.position = position;
        grainModelObject.transform.rotation = rotation;
        GrainModelOld grainModel = grainModelObject.GetComponent<GrainModelOld>();
        // AudioIO.LoadAudioFromAssets(filePath, (signal) => {
        //     grainModel.SetAudioBuffer(signal);
        // });
        return grainModel;
    }

    /// <summary>
    /// Spawn a grain model from a directory containing audio files. The size of the grains will be optimized for grain count
    /// </summary>
    public static GrainModelOld SpawnFromFolder(string folderPath, Vector3 position, Quaternion rotation) {
        // GameObject grainModelObject = Instantiate(TsvrApplication.Config.grainModel, GameObject.Find("GrainParent").transform);
        GameObject grainModelObject = Instantiate(Resources.Load<GameObject>("Prefabs/GrainModel"), GameObject.Find("GrainParent").transform);
        grainModelObject.transform.position = position;
        grainModelObject.transform.rotation = rotation;
        GrainModelOld grainModel = grainModelObject.GetComponent<GrainModelOld>();
        // AudioIO.LoadAudioFromAssets(filePath, (signal) => {
        //     grainModel.SetAudioBuffer(signal);
        // });
        return grainModel;
    }

    # endregion

    
    # region Debugging
    public void PlaySequentially() {
        StartCoroutine(PlayGrainsSequentially());
    }
        
    // eventually this will be in its own constellation class
    public float seqPlayRate = 0.1f;

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

    public void OnSnap()
    {
        throw new NotImplementedException();
    }

    public void OnUnsnap()
    {
        throw new NotImplementedException();
    }

    public void OnSnapVolumeEnter(Collider other)
    {
        throw new NotImplementedException();
    }

    #endregion

}
