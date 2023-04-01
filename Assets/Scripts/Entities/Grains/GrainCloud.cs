using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine.Pool;
using UnityEngine;
using NWaves.Signals;
using System;

/// <summary>
/// A collection of audio grains linked to a granular buffer and parameter handler
/// </summary>
public class GrainCloud : MonoBehaviour {

    public GrainModel grainModel; // the model that this cloud is linked to which handles positioning, scaling, and so on

    private GranularBuffer granularBuffer;
    private GranularParameterHandler parameterHandler;
    private List<Grain> grains;
    private List<Grain> selectedGrains;

    private void OnEnable() {

    }

    private void OnDisable() {
        granularBuffer?.Stop();
    }

    // int grainIndex = 0;
    // void Update() {
    //     if (grains != null && grains.Count > 0) {
    //         grains[grainIndex % grains.Count].Activate(1f, Grain.ActivationAction.Play);
    //         granularBuffer.PlayGrain(grains[grainIndex % grains.Count].GrainID, 1f);
    //         grainIndex++;
    //     }
    // }

    // we're gonna need ways to merge, split, and delete clouds
    // and delete individual grains

    public void Initialize(DiscreteSignal audioBuffer, GranularParameters parameterValues = null) {
        Debug.Log("Initializing grain cloud");
        granularBuffer = new GranularBuffer(audioBuffer, gameObject.GetOrAddComponent<PolyvoicePlayer>());
        grainModel = gameObject.GetOrAddComponent<GrainModel>();
        selectedGrains = new List<Grain>();
        grains = new List<Grain>();
        SetParameters(parameterValues == null ? new GranularParameters() : parameterValues);
    }

    public void Initialize(GranularBuffer granularBuffer, GranularParameters parameterValues = null) {
        Debug.Log("Initializing grain cloud");
        this.granularBuffer = granularBuffer;
        grainModel = gameObject.GetOrAddComponent<GrainModel>();
        selectedGrains = new List<Grain>();
        grains = new List<Grain>();
        SetParameters(parameterValues == null ? new GranularParameters() : parameterValues);
    }

    public void SetParameters(GranularParameters parameterValues) {
        if (parameterHandler != null) {
            parameterHandler.onFeaturePositionUpdate -= UpdateGrainPositions;
            parameterHandler.onFeatureColorUpdate -= UpdateGrainColors;
            parameterHandler.onFeatureScaleUpdate -= UpdateGrainScales;
            parameterHandler.onWindowUpdate -= GenerateGrains;
        }
        parameterHandler = new GranularParameterHandler(parameterValues);
        parameterHandler.onFeaturePositionUpdate += UpdateGrainPositions;
        parameterHandler.onFeatureColorUpdate += UpdateGrainColors;
        parameterHandler.onFeatureScaleUpdate += UpdateGrainScales;
        parameterHandler.onWindowUpdate += GenerateGrains;
        GenerateGrains(parameterValues.windowSize, parameterValues.hopSize);
    }

    public void GenerateGrains(int windowSize, int hopSize) {
        ClearGrains();
        var features = parameterHandler.CurrentFeatures();

        granularBuffer.ResetAnalysis(windowSize, hopSize, features, () => {
            for (int i = 0; i < granularBuffer.WindowTimes.Length; i++) {
                grains.Add(CreateGrain(i));
            }
            parameterHandler.CallUpdate();
        });
    }

    private Grain CreateGrain(int id) {
        GameObject grainObject = Instantiate(TsvrApplication.Config.grainPrefab, grainModel.transform);
        Grain grain = grainObject.GetComponent<Grain>();
        grain.Initialize(id);
        grain.OnActivateEvent += OnGrainActivated;
        return grain;
    }

    

    # region Grain Callbacks

    private void OnGrainActivated(Grain grain, float value, Grain.ActivationAction activationAction) {
        // if the cloud allows the grain to be played, then 
        // tell it to animate, and send granularBuffer a playback event
        if (selectedGrains.Count > 0 && !selectedGrains.Contains(grain)) {
            Debug.Log($"Grain {grain.GrainID} is not selected, not activating");
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
            Debug.Log($"Grain {grain.GrainID} has been played too recently, not activating");
            return;
        }
        granularBuffer.PlayGrain(grain.GrainID, value);
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
        foreach(Grain grain in grains) {
            Destroy(grain.gameObject);
        }
        grains.Clear();
    }

    private void UpdateGrainPositions(AudioFeature[] features, float[] axisScale) {
        foreach(Grain grain in grains) {
            Vector3 newPos = new Vector3(
                granularBuffer.GetFeatureValue(features[0], grain.GrainID) * axisScale[0],
                granularBuffer.GetFeatureValue(features[1], grain.GrainID) * axisScale[1],
                granularBuffer.GetFeatureValue(features[2], grain.GrainID) * axisScale[2]
            );
            grain.UpdatePosition(newPos);
        } 
    }

    private void UpdateGrainColors(AudioFeature[] features, bool useHSV) {
        foreach(Grain grain in grains) {
            Color newColor;
            if (useHSV) {
                newColor = Color.HSVToRGB(
                    granularBuffer.GetFeatureValue(features[0], grain.GrainID),
                    granularBuffer.GetFeatureValue(features[1], grain.GrainID),
                    granularBuffer.GetFeatureValue(features[2], grain.GrainID)
                );
            } else {
                newColor = new Color(
                    granularBuffer.GetFeatureValue(features[0], grain.GrainID, true, 0f, 1f),
                    granularBuffer.GetFeatureValue(features[1], grain.GrainID, true, 0f, 1f),
                    granularBuffer.GetFeatureValue(features[2], grain.GrainID, true, 0f, 1f)
                );
            }
            grain.UpdateColor(newColor);
        }
    }

    private void UpdateGrainScales(AudioFeature feature, float multiplier, float exponential) {

        foreach(Grain grain in grains) {
            float newScale = granularBuffer.GetFeatureValue(feature, grain.GrainID, true, 0.1f, 1f);
            newScale = Mathf.Pow(newScale, exponential) * multiplier;
            grain.UpdateScale(newScale);
        }
    }

    # endregion
}