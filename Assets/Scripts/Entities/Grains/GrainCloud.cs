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

    private GranularBuffer granularBuffer;
    private GranularParameterHandler parameterHandler;
    private List<Grain> grains;
    private List<Grain> selectedGrains;

    public GrainModel GrainModel { get; private set; } // the model that this cloud is linked to
    // it handles positioning, scaling, and so on

    private void Initialize(GranularBuffer granularBuffer, 
                            GranularParameterValues parameterValues) {
        this.granularBuffer = granularBuffer;
        selectedGrains = new List<Grain>();
        SetParameters(parameterValues);
    }

    public void SetParameters(GranularParameterValues parameterValues) {
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
    }

    public void GenerateGrains(int windowSize, int hopSize) {
        ClearGrains();
        var features = parameterHandler.CurrentFeatures();
        granularBuffer.RunBatchAnalysis(features, () => {
            for (int i = 0; i < granularBuffer.WindowTimes.Length; i++) {
                GameObject grainObject = Instantiate(TsvrApplication.Config.grainPrefab, transform);
                Grain grain = grainObject.GetComponent<Grain>();
                grain.Initialize(i);
                grain.OnActivateEvent += OnGrainActivated;
                grains.Add(grain);
            }
        });
    }

    # region Grain Callbacks

    private void OnGrainActivated(Grain grain, float value, object caller) {
        // if the cloud allows the grain to be played, then 
        // tell it to animate, and send granularBuffer a playback event
        if (selectedGrains.Count > 0 && !selectedGrains.Contains(grain)) {
            Debug.Log($"Grain {grain.GrainID} is not selected, not activating");
            return;
        }
        // if type of caller is PlayWand, then play the grain if time has been long enough
        if (caller.GetType() == typeof(TsvrTool)) {
            switch (((TsvrTool)caller).ToolType) {
                case TsvrToolType.PlayWand:
                    if (grain.TimeSinceLastPlayed() < TsvrApplication.Settings.GrainPlayCooldown) {
                        Debug.Log($"Grain {grain.GrainID} has been played too recently, not activating");
                        return;
                    }
                    granularBuffer.PlayGrain(grain.GrainID, value);
                    grain.PlayAnimation(Color.red, radiusMultiplier : 1.2f, duration : 1f);
                    break;

                case TsvrToolType.SelectWand:
                    Debug.Log("Grain activated with select wand");
                    if (selectedGrains.Contains(grain)) {
                        selectedGrains.Remove(grain);
                        grain.PlayAnimation(Color.white, radiusMultiplier : 1f, duration : 1f);
                    } else {
                        selectedGrains.Add(grain);
                        grain.PlayAnimation(Color.green, radiusMultiplier : 1.2f, duration : 1f);
                    }
                    break;
                default:
                    break;
            }
        }
    }

    # endregion

    # region Grain State Management
    
    private void ClearGrains() {
        foreach(Grain grain in grains) {
            Destroy(grain.gameObject);
        }
        grains.Clear();
    }

    private void UpdateGrainPositions(AudioFeature[] features, Vector3 axisScale) {
        foreach(Grain grain in grains) {
            Vector3 newPos = new Vector3(
                granularBuffer.GetFeatureValue(features[0], grain.GrainID),
                granularBuffer.GetFeatureValue(features[1], grain.GrainID),
                granularBuffer.GetFeatureValue(features[2], grain.GrainID)
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
                    granularBuffer.GetFeatureValue(features[0], grain.GrainID),
                    granularBuffer.GetFeatureValue(features[1], grain.GrainID),
                    granularBuffer.GetFeatureValue(features[2], grain.GrainID)
                );
            }
            grain.UpdateColor(newColor);
        }
    }

    private void UpdateGrainScales(AudioFeature feature) {

        foreach(Grain grain in grains) {
            float newScale = granularBuffer.GetFeatureValue(feature, grain.GrainID);
            grain.UpdateScale(newScale);
        }
    }

    # endregion


    

}