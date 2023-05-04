using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine.Pool;
using UnityEngine;
using NWaves.Signals;
using NWaves.Windows;
using System;
using System.Linq;

/// <summary>
/// A collection of audio grains linked to a granular buffer and parameter handler
/// </summary>
public class GrainCloud : InteractableObject
{
    public GranularBuffer Buffer { get; private set; }
    public GranularParameterHandler ParameterHandler { get; private set; }
    public List<Grain> Grains { get; private set; } = new List<Grain>();
    public List<Sequence> Sequences { get {
        if (_sequences == null) {
            _sequences = new List<Sequence>();
        }
        return _sequences;
    } }

    
    public Action<Grain> OnGrainAdded;
    public Action OnCloudReset;


    [SerializeField] private BoundingBoxRenderer _box; // the model that this cloud is linked to which handles positioning, scaling, and so on
    private BufferPlaybackHandler _playbackHandler;
    private List<Sequence> _sequences;
    private List<Grain> _selectedGrains;

    private InspectableProperties _inspectableProperties = new InspectableProperties {
        Name = "Grain Cloud",
        Properties = new List<InspectableProperty>()
    };


    public override void OnMoveStart()
    {
        base.OnMoveStart();
        foreach(Grain grain in Grains) {
            grain.ChangePositioningState(Grain.GrainState.Repositioning);
        }
        _box.SetColor(Color.yellow);
    }

    public override void OnMoveEnd()
    {
        base.OnMoveEnd();
        foreach(Grain grain in Grains) {
            grain.ChangePositioningState(Grain.GrainState.Idle);
        }
        _box.SetColor(Color.white);
    }

    public override InspectableProperties Inspect()
    {
        return _inspectableProperties;
    }


    # region MonoBehaviours

    private void OnDisable() {
        Buffer?.Stop();
        _playbackHandler?.Stop();
        foreach(Sequence sequence in Sequences) {
            sequence.Stop();
        }
    }

    # endregion



    # region Initialization

    /// <summary>
    /// Initialize with new audioBuffer and optional GranularParameters
    /// </summary>
    public void Initialize(DiscreteSignal audioBuffer, GranularParameters parameterValues=null) {
        Debug.Log("Initializing GrainCloud with audio buffer...");
        Buffer = new GranularBuffer(audioBuffer);

        _playbackHandler = gameObject.GetOrAddComponent<BufferPlaybackHandler>();
        _playbackHandler.SetAudioBuffer(audioBuffer);

        // _box = gameObject.GetComponent<BoundingBoxRenderer>();
        
        _selectedGrains = new List<Grain>();
        Grains = new List<Grain>();

        SetParameters(parameterValues ?? new GranularParameters());
    }

    /// <summary>
    /// Set the GranularParameters, which determine the feature ordering for display
    /// </summary>
    public void SetParameters(GranularParameters parameterValues) {
        UnsubscribeFromParameterHandlerEvents();
        ParameterHandler = new GranularParameterHandler(parameterValues);
        SubscribeToParameterHandlerEvents();
        GenerateGrains(parameterValues.windowSize, parameterValues.hopSize);
    }

    private void SubscribeToParameterHandlerEvents() {
        ParameterHandler.onFeaturePositionUpdate += UpdateGrainPositions;
        ParameterHandler.onFeatureColorUpdate += UpdateGrainColors;
        ParameterHandler.onFeatureScaleUpdate += UpdateGrainScales;
        ParameterHandler.onWindowUpdate += GenerateGrains;
    }

    private void UnsubscribeFromParameterHandlerEvents() {
        if (ParameterHandler == null) return;
        ParameterHandler.onFeaturePositionUpdate -= UpdateGrainPositions;
        ParameterHandler.onFeatureColorUpdate -= UpdateGrainColors;
        ParameterHandler.onFeatureScaleUpdate -= UpdateGrainScales;
        ParameterHandler.onWindowUpdate -= GenerateGrains;
    }

    # endregion



    # region Spawning

    /// <summary>
    /// Create a new GrainCloud from the current selection of Grains
    /// </summary>
    public GrainCloud FromCurrentSelection() {
        var indices = _selectedGrains.Select(g => g.Window).ToArray();
        var newAudioBuffer = Buffer.GetCroppedBuffer(indices);
        return GrainCloudSpawner.Spawn(newAudioBuffer, ParameterHandler.GetParameters());
    }

    # endregion


    # region Sequence
        
    public void AddSequence(Sequence sequence) {
        Sequences.Add(sequence);
        gameObject.AddComponent<SequenceRenderer>().SetSequence(sequence);
    }


    public void RemoveSequence(Sequence sequence) {
        Sequences.Remove(sequence);
        // gameObject.RemoveComponent<SequenceRenderer>();
    }


    # endregion

    # region Grain Lifecycle

    private Grain SpawnGrain() {
        GameObject grainObject = Instantiate(TsvrApplication.Config.grainPrefab, gameObject.transform);
        return grainObject.GetOrAddComponent<Grain>();
    }

    private void GenerateGrains(int windowSize, int hopSize) {
        ClearGrains();
        var features = ParameterHandler.CurrentFeatures();

        Buffer.ResetAnalysis(windowSize, hopSize, features, () => {
            foreach(WindowTime windowTime in Buffer.Windows) {
                Grains.Add(CreateGrain(windowTime));
            }
            ParameterHandler.CallUpdate();
            Debug.Log("Finished generating grains, Num Grains: " + Grains.Count + " Invoking OnCloudReset with " + OnCloudReset.GetInvocationList().Length + " listeners");
            OnCloudReset?.Invoke();
        });
    }

    private Grain CreateGrain(WindowTime window) {
        var grain = SpawnGrain();
        grain.Initialize(window);
        grain.OnWandInteract += OnGrainWandInteraction;
        grain.OnSchedule += OnGrainScheduled;
        OnGrainAdded?.Invoke(grain);
        return grain;
    }

    public Grain GetGrain(Guid id) {
        return Grains.Find(g => g.Id == id);
    }

    # endregion



    # region Grain Callbacks

    private void OnGrainScheduled(
        object sender, 
        (double time, SequenceableParameters parameters, ScheduleCancellationToken token) schedule)
    {
        Grain grain = (Grain)sender;
        WindowedPlaybackEvent playbackEvent = new WindowedPlaybackEvent(
            bufferWindow: grain.Window,
            windowType: ParameterHandler.WindowType,
            // windowType: WindowTypes.Rectangular,
            gain: schedule.parameters.Gain,
            submitterId: grain.Id,
            onPlayStart: grain.SequenceablePlayStart,
            onPlayEnd: grain.SequenceablePlayEnd
        );

        playbackEvent.RegisterSequenceableEvents(grain);

        // -- |> ---
        _playbackHandler.PlayScheduled(playbackEvent, schedule.time, schedule.token);
    }

    private void OnGrainWandInteraction(object caller, WandInteraction interaction) {
        Grain grain = (Grain)caller;
        // if the cloud allows the grain to be played, then 
        // tell it to animate, and send granularBuffer a playback event
        if (_selectedGrains.Count > 0 && !_selectedGrains.Contains(grain)) {
            Debug.Log($"Grain {grain.Id} is not selected, not activating");
            return;
        }
        switch (interaction.ActionType) {
            case WandInteractionType.Play :
                PlayGrain(grain, interaction.Value);
                break;
            
            case WandInteractionType.Select :
                SelectGrain(grain);
                break;

            case WandInteractionType.Delete : 
                DeleteGrain(grain);
                break;
        }
    }

    private void PlayGrain(Grain grain, float value) {
        if (grain.TimeSinceLastPlayed() < TsvrApplication.Settings.GrainPlayCooldown) {
            Debug.Log($"Grain {grain.Id} has been played too recently, not activating");
            return;
        }

        WindowedPlaybackEvent playbackEvent = _playbackHandler.GetPooledPlaybackEvent();
        playbackEvent.Initialize(
            bufferWindow: grain.Window,
            windowType: ParameterHandler.WindowType,
            gain: value,
            submitterID: grain.Id
        );
        
        // -- |> ---
        _playbackHandler.PlayNow(playbackEvent);
        grain.PlayActivatedAnimation(Color.red, radiusMultiplier: 1.2f, duration: 1f);
    }

    private void SelectGrain(Grain grain) {
        if (_selectedGrains.Contains(grain)) {
            _selectedGrains.Remove(grain);
            grain.PlayActivatedAnimation(Color.white, radiusMultiplier: 1f, duration: 1f);
        } else {
            _selectedGrains.Add(grain);
            grain.PlayActivatedAnimation(Color.green, radiusMultiplier: 1.2f, duration: 1f);
        }
    }

    private void DeleteGrain(Grain grain) {
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
            Vector3 newPos = new Vector3(
                Buffer.GetFeatureValue(features[0], grain.Window) * axisScale[0],
                Buffer.GetFeatureValue(features[1], grain.Window) * axisScale[1],
                Buffer.GetFeatureValue(features[2], grain.Window) * axisScale[2]
            );
            grain.UpdatePosition(newPos);
        } 
    }

    private void UpdateGrainColors(AudioFeature[] features, bool useHSV) {
        foreach(Grain grain in Grains) {
            Color newColor;
            if (useHSV) {
                newColor = Color.HSVToRGB(
                    Buffer.GetFeatureValue(features[0], grain.Window),
                    Buffer.GetFeatureValue(features[1], grain.Window),
                    Buffer.GetFeatureValue(features[2], grain.Window)
                );
            } else {
                newColor = new Color(
                    Buffer.GetFeatureValue(features[0], grain.Window, true, 0f, 1f),
                    Buffer.GetFeatureValue(features[1], grain.Window, true, 0f, 1f),
                    Buffer.GetFeatureValue(features[2], grain.Window, true, 0f, 1f)
                );
            }
            grain.UpdateColor(newColor);
        }
    }

    private void UpdateGrainScales(AudioFeature feature, float multiplier, float exponential) {

        foreach(Grain grain in Grains) {
            float newScale = Buffer.GetFeatureValue(feature, grain.Window, true, 0.1f, 1f);
            newScale = Mathf.Pow(newScale, exponential) * multiplier;
            grain.UpdateScale(newScale);
        }
    }

    # endregion
}