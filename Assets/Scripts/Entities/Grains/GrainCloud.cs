using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine.Pool;
using UnityEngine;
using NWaves.Signals;
using System;
using System.Linq;

/// <summary>
/// A collection of audio grains linked to a granular buffer and parameter handler
/// </summary>
public class GrainCloud : MonoBehaviour, IPositionedSequenceable {

    public GrainModel grainModel; // the model that this cloud is linked to which handles positioning, scaling, and so on
    public GranularBuffer granularBuffer;
    public List<Sequence> Sequences { get {
        if (_sequences == null) {
            _sequences = new List<Sequence>();
        }
        return _sequences;
    } }

    private List<Sequence> _sequences;

    public GranularParameterHandler parameterHandler;
    public List<Grain> Grains { get; protected set; }
    private List<Grain> selectedGrains;


    public delegate void OnGrainAddedHandler(Grain grain);
    public event OnGrainAddedHandler OnGrainAdded;

    public delegate void OnCloudResetHandler();
    public event OnCloudResetHandler OnCloudReset;


    # region MonoBehaviours

    private void OnDisable() {
        granularBuffer?.Stop();
        foreach(Sequence sequence in Sequences) {
            sequence.Stop();
        }
    }


    # endregion

    # region Public Methods
    public void Initialize(DiscreteSignal audioBuffer, GranularParameters parameterValues = null) {
        InitializeWithAudioBuffer(audioBuffer, parameterValues);
    }

    public void Initialize(GranularBuffer granularBuffer, GranularParameters parameterValues = null) {
        InitializeWithGranularBuffer(granularBuffer, parameterValues);
    }

    /// <summary>
    /// Set the GranularParameters, which determine the feature ordering for display
    /// </summary>
    public void SetParameters(GranularParameters parameterValues) {
        UnsubscribeFromParameterHandlerEvents();
        parameterHandler = new GranularParameterHandler(parameterValues);
        SubscribeToParameterHandlerEvents();
        GenerateGrains(parameterValues.windowSize, parameterValues.hopSize);
    }

    /// <summary>
    /// Create a new GrainCloud from the current selection of Grains
    /// </summary>
    public GrainCloud FromCurrentSelection() {
        // create a new grain cloud from the selected grains
        DiscreteSignal newAudioBuffer = granularBuffer.GetCroppedBuffer(
            selectedGrains.Select(g => g.ID).ToArray());
        return GrainCloudSpawner.Spawn(newAudioBuffer, parameterHandler.GetParameters());
    }

    
    # endregion

    # region ISequencable
    
    public int ID { get => parameterHandler.ID; }

    public Vector3 Position { get => transform.position; }
    
    public void Play(float gain) {
        // play through the subsequence set for this cloud
        
        if (Sequences.Count > 0) {
            Sequences[0].Play(gain);
        }
    }

    # endregion

    # region Sequence

    
    public Sequence CreateLinearSequence(float bpm = 120) {
        Sequence sequence = new Sequence();
        SequenceRenderer sequenceRenderer = gameObject.AddComponent<SequenceRenderer>();
        sequence.AddObserver(sequenceRenderer);
        sequence.Add(Grains, 1f);
        sequence.SetBPM(bpm);
        return sequence;
    }

    public void RemoveSequence(Sequence sequence) {
        // Sequences.Remove(sequence);
        // gameObject.RemoveComponent<SequenceRenderer>();
    }

    # endregion
    
    # region Initialization
    
    private void InitializeWithAudioBuffer(DiscreteSignal audioBuffer, GranularParameters parameterValues) {
        granularBuffer = new GranularBuffer(audioBuffer, gameObject.GetOrAddComponent<PolyvoicePlayer>());
        InitializeGrainCloud(parameterValues);
    }

    private void InitializeWithGranularBuffer(GranularBuffer granularBuffer, GranularParameters parameterValues) {
        this.granularBuffer = granularBuffer;
        InitializeGrainCloud(parameterValues);
    }

    private void InitializeGrainCloud(GranularParameters parameterValues) {
        grainModel = gameObject.GetOrAddComponent<GrainModel>();
        selectedGrains = new List<Grain>();
        Grains = new List<Grain>();
        SetParameters(parameterValues ?? new GranularParameters());
    }

    private void SubscribeToParameterHandlerEvents() {
        parameterHandler.onFeaturePositionUpdate += UpdateGrainPositions;
        parameterHandler.onFeatureColorUpdate += UpdateGrainColors;
        parameterHandler.onFeatureScaleUpdate += UpdateGrainScales;
        parameterHandler.onWindowUpdate += GenerateGrains;
    }

    private void UnsubscribeFromParameterHandlerEvents() {
        if (parameterHandler != null) {
            parameterHandler.onFeaturePositionUpdate -= UpdateGrainPositions;
            parameterHandler.onFeatureColorUpdate -= UpdateGrainColors;
            parameterHandler.onFeatureScaleUpdate -= UpdateGrainScales;
            parameterHandler.onWindowUpdate -= GenerateGrains;
        }
    }

    # endregion

    # region Grain Lifecycle

    private void GenerateGrains(int windowSize, int hopSize) {
        ClearGrains();
        var features = parameterHandler.CurrentFeatures();

        granularBuffer.ResetAnalysis(windowSize, hopSize, features, () => {
            for (int i = 0; i < granularBuffer.WindowTimes.Length; i++) {
                Grains.Add(CreateGrain(i));
            }
            parameterHandler.CallUpdate();
            OnCloudReset?.Invoke();
        });
    }

    private Grain CreateGrain(int id) {
        GameObject grainObject = Instantiate(TsvrApplication.Config.grainPrefab, grainModel.transform);
        Grain grain = grainObject.GetComponent<Grain>();
        grain.Initialize(id);
        grain.OnActivateEvent += OnGrainActivated;
        OnGrainAdded?.Invoke(grain);
        return grain;
    }

    # endregion

    # region Grain Callbacks

    private void OnGrainActivated(Grain grain, float value, Grain.ActivationAction activationAction) {
        // if the cloud allows the grain to be played, then 
        // tell it to animate, and send granularBuffer a playback event
        if (selectedGrains.Count > 0 && !selectedGrains.Contains(grain)) {
            Debug.Log($"Grain {grain.ID} is not selected, not activating");
            return;
        }
        switch (activationAction) {
            case Grain.ActivationAction.Play :
                HandleGrainPlay(grain, value);
                break;
            
            case Grain.ActivationAction.Select :
                HandleGrainSelect(grain);
                break;

            case Grain.ActivationAction.Delete : 
                HandleGrainDelete(grain);
                break;
        }
    }

    private void HandleGrainPlay(Grain grain, float value) {
        if (grain.TimeSinceLastPlayed() < TsvrApplication.Settings.GrainPlayCooldown) {
            Debug.Log($"Grain {grain.ID} has been played too recently, not activating");
            return;
        }
        granularBuffer.PlayGrain(grain.ID, value);
        grain.Play(Color.red, radiusMultiplier: 1.2f, duration: 1f);
    }

    private void HandleGrainSelect(Grain grain) {
        if (selectedGrains.Contains(grain)) {
            selectedGrains.Remove(grain);
            grain.Play(Color.white, radiusMultiplier: 1f, duration: 1f);
        } else {
            selectedGrains.Add(grain);
            grain.Play(Color.green, radiusMultiplier: 1.2f, duration: 1f);
        }
    }

    private void HandleGrainDelete(Grain grain) {
        grain.Delete();
    }

    # endregion

    # region Grain State Management
    
    private void ClearGrains() {
        foreach(Grain grain in Grains) {
            Destroy(grain.gameObject);
        }
        Grains.Clear();
    }

    private void UpdateGrainPositions(AudioFeature[] features, float[] axisScale) {
        foreach(Grain grain in Grains) {
            // debug log if any features are NaN
            if (granularBuffer.GetFeatureValue(features[0], grain.ID).Equals(float.NaN) ||
                granularBuffer.GetFeatureValue(features[1], grain.ID).Equals(float.NaN) ||
                granularBuffer.GetFeatureValue(features[2], grain.ID).Equals(float.NaN)) {
                Debug.Log($"NaN feature value for grain {grain.ID}");
            }

            Vector3 newPos = new Vector3(
                granularBuffer.GetFeatureValue(features[0], grain.ID) * axisScale[0],
                granularBuffer.GetFeatureValue(features[1], grain.ID) * axisScale[1],
                granularBuffer.GetFeatureValue(features[2], grain.ID) * axisScale[2]
            );
            grain.UpdatePosition(newPos);
        } 
    }

    private void UpdateGrainColors(AudioFeature[] features, bool useHSV) {
        foreach(Grain grain in Grains) {
            Color newColor;
            if (useHSV) {
                newColor = Color.HSVToRGB(
                    granularBuffer.GetFeatureValue(features[0], grain.ID),
                    granularBuffer.GetFeatureValue(features[1], grain.ID),
                    granularBuffer.GetFeatureValue(features[2], grain.ID)
                );
            } else {
                newColor = new Color(
                    granularBuffer.GetFeatureValue(features[0], grain.ID, true, 0f, 1f),
                    granularBuffer.GetFeatureValue(features[1], grain.ID, true, 0f, 1f),
                    granularBuffer.GetFeatureValue(features[2], grain.ID, true, 0f, 1f)
                );
            }
            grain.UpdateColor(newColor);
        }
    }

    private void UpdateGrainScales(AudioFeature feature, float multiplier, float exponential) {

        foreach(Grain grain in Grains) {
            float newScale = granularBuffer.GetFeatureValue(feature, grain.ID, true, 0.1f, 1f);
            newScale = Mathf.Pow(newScale, exponential) * multiplier;
            grain.UpdateScale(newScale);
        }
    }

    # endregion

    // int grainIndex = 0;
    // void Update() {
    //     if (grains != null && grains.Count > 0) {
    //         grains[grainIndex % grains.Count].Activate(1f, Grain.ActivationAction.Play);
    //         granularBuffer.PlayGrain(grains[grainIndex % grains.Count].GrainID, 1f);
    //         grainIndex++;
    //     }
    // }
}