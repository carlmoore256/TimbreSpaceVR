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
public class GrainCloud : MonoBehaviour, IPositionedSequenceable 
{
    public GrainModel GrainModel { get; private set; } // the model that this cloud is linked to which handles positioning, scaling, and so on
    public GranularBuffer GranularBuffer { get; private set; }
    public GranularParameterHandler ParameterHandler { get; private set; }
    public List<Grain> Grains { get; private set; }
    public List<Sequence> Sequences { get {
        if (_sequences == null) {
            _sequences = new List<Sequence>();
        }
        return _sequences;
    } }

    
    public Action<Grain> OnGrainAdded;
    public Action OnCloudReset;


    private BufferPlaybackHandler _playbackHandler;
    private List<Sequence> _sequences;
    private List<Grain> _selectedGrains;


    # region MonoBehaviours

    private void OnDisable() {
        GranularBuffer?.Stop();
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
        GranularBuffer = new GranularBuffer(audioBuffer);

        _playbackHandler = gameObject.GetOrAddComponent<BufferPlaybackHandler>();
        _playbackHandler.SetAudioBuffer(audioBuffer);
        
        GrainModel = gameObject.GetOrAddComponent<GrainModel>();
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
        var indices = _selectedGrains.Select(g => g.GrainIndex).ToArray();
        var newAudioBuffer = GranularBuffer.GetCroppedBuffer(indices);
        return GrainCloudSpawner.Spawn(newAudioBuffer, ParameterHandler.GetParameters());
    }

    # endregion



    # region ISequencable
    
    public Guid Id { get => ParameterHandler.Id; }

    public Vector3 Position { get => transform.position; }

    public event EventHandler<(double, SequenceableParameters)> OnSchedule;
    public event Action OnSequenceablePlayStart;
    public event Action OnSequenceablePlayEnd;

    public void Schedule(double time, SequenceableParameters parameters) {
        Debug.Log("Scheduling GrainCloud as Sequenceable, Num Sequences: " + Sequences.Count);
        // play through the subsequence set for this cloud
        if (Sequences.Count > 0) {
            foreach(Sequence sequence in Sequences) {
                sequence.Schedule(time, parameters);
            }
        }
    }

    public void SequenceablePlayStart() {
        // make some animation to show entire cloud is playing
        Debug.Log("Playing GrainCloud as Sequenceable");
        OnSequenceablePlayStart?.Invoke();
    }

    public void SequenceablePlayEnd() {
        Debug.Log("Finished playing GrainCloud as Sequenceable");
        OnSequenceablePlayEnd?.Invoke();
    }

    # endregion



    # region Sequence
    
    private Sequence CreateEmptySequence() {
        Sequence sequence = new Sequence();
        SequenceRenderer sequenceRenderer = gameObject.AddComponent<SequenceRenderer>();
        return sequence;
    } 

    
    public Sequence CreateLinearSequence(float bpm = 120) {
        Sequence sequence = new Sequence();
        SequenceRenderer renderer = gameObject.AddComponent<SequenceRenderer>();
        renderer.SetSequence(sequence);
        sequence.AddSequenceableRange(Grains);
        sequence.SetBPM(bpm);
        return sequence;
    }


    public void RemoveSequence(Sequence sequence) {
        // Sequences.Remove(sequence);
        // gameObject.RemoveComponent<SequenceRenderer>();
    }

    public Sequence FromGrainIndexes(int[] grainIndexes) {
        Sequence sequence = new Sequence();
        SequenceRenderer renderer = gameObject.AddComponent<SequenceRenderer>();
        renderer.SetSequence(sequence);
        // sequence.AddObserver(sequenceRenderer);
        // sequence.Add(Grains.Where(g => grainIndexes.Contains(g.GrainIndex)), 1f);
        return sequence;
    }

    # endregion



    # region Grain Lifecycle

    private Grain SpawnGrain() {
        GameObject grainObject = Instantiate(TsvrApplication.Config.grainPrefab, GrainModel.transform);
        return grainObject.GetOrAddComponent<Grain>();
    }

    private void GenerateGrains(int windowSize, int hopSize) {
        ClearGrains();
        var features = ParameterHandler.CurrentFeatures();

        GranularBuffer.ResetAnalysis(windowSize, hopSize, features, () => {
            for (int i = 0; i < GranularBuffer.NumWindows; i++) {
                Grains.Add(CreateGrain(i));
            }
            ParameterHandler.CallUpdate();
            OnCloudReset?.Invoke();
        });
    }

    private Grain CreateGrain(int grainIndex) {
        var grain = SpawnGrain();
        grain.Initialize(grainIndex);
        grain.OnActivate += OnGrainActivated;
        grain.OnSchedule += OnGrainScheduled;
        OnGrainAdded?.Invoke(grain);
        return grain;
    }

    public Grain GetGrain(Guid id) {
        return Grains.Find(g => g.Id == id);
    }

    # endregion



    # region Grain Callbacks

    private void OnGrainActivated(Grain grain, float value, Grain.ActivationAction activationAction) {
        // if the cloud allows the grain to be played, then 
        // tell it to animate, and send granularBuffer a playback event
        if (_selectedGrains.Count > 0 && !_selectedGrains.Contains(grain)) {
            Debug.Log($"Grain {grain.Id} is not selected, not activating");
            return;
        }
        switch (activationAction) {
            case Grain.ActivationAction.Play :
                OnGrainPlay(grain, value);
                break;
            
            case Grain.ActivationAction.Select :
                OnGrainSelect(grain);
                break;

            case Grain.ActivationAction.Delete : 
                OnGrainDelete(grain);
                break;
        }
    }

    private void OnGrainPlay(Grain grain, float value) {
        if (grain.TimeSinceLastPlayed() < TsvrApplication.Settings.GrainPlayCooldown) {
            Debug.Log($"Grain {grain.Id} has been played too recently, not activating");
            return;
        }

        WindowedPlaybackEvent playbackEvent = _playbackHandler.GetPooledPlaybackEvent();
        playbackEvent.Initialize(
            bufferWindow: GranularBuffer.GetWindow(grain.GrainIndex),
            windowType: WindowTypes.Hann,
            gain: value,
            submitterID: grain.Id);

        playbackEvent.RegisterSequenceableEvents(grain);
        // here is how to add events to the playback event:
        // playbackEvent.onPlayStart += grain.OnSequenceablePlayStart;
        // playbackEvent.onPlayEnd += grain.OnSequenceablePlayEnd;

        // -- |> ---
        _playbackHandler.PlayNow(playbackEvent);
    }

    private void OnGrainScheduled(object sender, (double time, SequenceableParameters parameters) schedule) {
        Grain grain = (Grain)sender;

        // Debug.Log($"Scheduling grain {grain.Id} at DSP time " + parameters.scheduleTime);

        WindowedPlaybackEvent playbackEvent = new WindowedPlaybackEvent(
            bufferWindow: GranularBuffer.GetWindow(grain.GrainIndex),
            windowType: ParameterHandler.WindowType,
            // windowType: WindowTypes.Rectangular,
            gain: schedule.parameters.Gain,
            submitterId: grain.Id,
            onPlayStart: grain.SequenceablePlayStart,
            onPlayEnd: grain.SequenceablePlayEnd);

        playbackEvent.RegisterSequenceableEvents(grain);

        // -- |> ---
        _playbackHandler.PlayScheduled(playbackEvent, schedule.time);
    }

    private void OnGrainSelect(Grain grain) {
        if (_selectedGrains.Contains(grain)) {
            _selectedGrains.Remove(grain);
            grain.PlayActivatedAnimation(Color.white, radiusMultiplier: 1f, duration: 1f);
        } else {
            _selectedGrains.Add(grain);
            grain.PlayActivatedAnimation(Color.green, radiusMultiplier: 1.2f, duration: 1f);
        }
    }

    private void OnGrainDelete(Grain grain) {
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
            if (GranularBuffer.GetFeatureValue(features[0], grain.GrainIndex).Equals(float.NaN) ||
                GranularBuffer.GetFeatureValue(features[1], grain.GrainIndex).Equals(float.NaN) ||
                GranularBuffer.GetFeatureValue(features[2], grain.GrainIndex).Equals(float.NaN)) {
                Debug.Log($"NaN feature value for grain {grain.Id}");
            }

            Vector3 newPos = new Vector3(
                GranularBuffer.GetFeatureValue(features[0], grain.GrainIndex) * axisScale[0],
                GranularBuffer.GetFeatureValue(features[1], grain.GrainIndex) * axisScale[1],
                GranularBuffer.GetFeatureValue(features[2], grain.GrainIndex) * axisScale[2]
            );
            grain.UpdatePosition(newPos);
        } 
    }

    private void UpdateGrainColors(AudioFeature[] features, bool useHSV) {
        foreach(Grain grain in Grains) {
            Color newColor;
            if (useHSV) {
                newColor = Color.HSVToRGB(
                    GranularBuffer.GetFeatureValue(features[0], grain.GrainIndex),
                    GranularBuffer.GetFeatureValue(features[1], grain.GrainIndex),
                    GranularBuffer.GetFeatureValue(features[2], grain.GrainIndex)
                );
            } else {
                newColor = new Color(
                    GranularBuffer.GetFeatureValue(features[0], grain.GrainIndex, true, 0f, 1f),
                    GranularBuffer.GetFeatureValue(features[1], grain.GrainIndex, true, 0f, 1f),
                    GranularBuffer.GetFeatureValue(features[2], grain.GrainIndex, true, 0f, 1f)
                );
            }
            grain.UpdateColor(newColor);
        }
    }

    private void UpdateGrainScales(AudioFeature feature, float multiplier, float exponential) {

        foreach(Grain grain in Grains) {
            float newScale = GranularBuffer.GetFeatureValue(feature, grain.GrainIndex, true, 0.1f, 1f);
            newScale = Mathf.Pow(newScale, exponential) * multiplier;
            grain.UpdateScale(newScale);
        }
    }

    # endregion
}