using System;
using UnityEngine;

/// <summary>
/// State manager for GrainModel parameters, invoking callbacks when parameters are changed
/// </summary>
public class GranularParameterHandler {

    public GranularParameterHandler(
            GranularParameters parameters,
            Action<AudioFeature[], float[]> onPositionParameterUpdate, 
            Action<AudioFeature[], bool> onColorParameterUpdate,
            Action<AudioFeature, float, float> onScaleParameterUpdate,
            Action<int, int> onWindowUpdate) { 
        this.parameters = parameters;
        this.onFeaturePositionUpdate = onPositionParameterUpdate;
        this.onFeatureColorUpdate = onColorParameterUpdate;
        this.onFeatureScaleUpdate = onScaleParameterUpdate;
        this.onWindowUpdate = onWindowUpdate;
    }

    public GranularParameterHandler(GranularParameters parameters) {
        this.parameters = parameters;
    }



    private GranularParameters parameters;

    public Action<AudioFeature[], float[]> onFeaturePositionUpdate;
    public Action<AudioFeature[], bool> onFeatureColorUpdate;
    public Action<AudioFeature, float, float> onFeatureScaleUpdate;
    public Action<int, int> onWindowUpdate; // window size, hop size

    public AudioFeature[] PositionFeatures { get => new AudioFeature[3] { parameters.xFeature, parameters.yFeature, parameters.zFeature }; }
    public AudioFeature[] ColorFeatures { get => new AudioFeature[3] { parameters.rFeature, parameters.gFeature, parameters.bFeature }; }

    public AudioFeature XFeature { get => parameters.xFeature; set {
        parameters.xFeature = value;
        onFeaturePositionUpdate?.Invoke(PositionFeatures, parameters.posAxisScale);
    } }

    public AudioFeature YFeature { get => parameters.yFeature; set {
        parameters.yFeature = value;
        onFeaturePositionUpdate?.Invoke(PositionFeatures, parameters.posAxisScale);
    } }

    public AudioFeature ZFeature { get => parameters.zFeature; set {
        parameters.zFeature = value;
        onFeaturePositionUpdate?.Invoke(PositionFeatures, parameters.posAxisScale);
    } }

    public AudioFeature[] XYZFeatures { get => new AudioFeature[3] { parameters.xFeature, parameters.yFeature, parameters.zFeature }; }

    public AudioFeature RFeature { get => parameters.rFeature; set {
        parameters.rFeature = value;
        onFeatureColorUpdate?.Invoke(ColorFeatures, parameters.useHSV);
    } }

    public AudioFeature GFeature { get => parameters.gFeature; set {
        parameters.gFeature = value;
        onFeatureColorUpdate?.Invoke(ColorFeatures, parameters.useHSV);
    } }

    public AudioFeature BFeature { get => parameters.bFeature; set {
        parameters.bFeature = value;
        onFeatureColorUpdate?.Invoke(ColorFeatures, parameters.useHSV);
    } }

    public AudioFeature[] RGBFeatures { get => new AudioFeature[3] { parameters.rFeature, parameters.gFeature, parameters.bFeature }; }

    public AudioFeature ScaleFeature { get => parameters.scaleFeature; set {
        parameters.scaleFeature = value;
        onFeatureScaleUpdate?.Invoke(value, parameters.scaleMult, parameters.scaleExp);
    } }

    public int WindowSize { get => parameters.windowSize; set { 
        parameters.windowSize=value;
        onWindowUpdate?.Invoke(value, parameters.hopSize); 
    } }
    
    public int HopSize { get => parameters.hopSize; set { 
        parameters.hopSize=value;
        onWindowUpdate?.Invoke(parameters.windowSize, value); 
    } }

    public float ScaleMult { get => parameters.scaleMult; set {
        parameters.scaleMult = value;
        onFeatureScaleUpdate?.Invoke(parameters.scaleFeature, value, parameters.scaleExp);
    } }
    public float ScaleExp { get => parameters.scaleExp; set {
        parameters.scaleExp = value;
        onFeatureScaleUpdate?.Invoke(parameters.scaleFeature, parameters.scaleMult, value);
    } }

    public bool UseHSV { get => parameters.useHSV; set {
        parameters.useHSV = value;
        onFeatureColorUpdate?.Invoke(ColorFeatures, value);
    } }

    public float[] PosAxisScale { get => parameters.posAxisScale; set {
        parameters.posAxisScale = value;
        onFeaturePositionUpdate?.Invoke(PositionFeatures, value);
    } }

    public int ID { get => parameters.ID; set => parameters.ID = value; }

    /** Set window size by the number of hops per window */
    public void SetWindowByHops(int hopCount) {
        HopSize = (int)(WindowSize /(float)hopCount);
    }

    public AudioFeature[] CurrentFeatures() {
        return new AudioFeature[7] {
            parameters.xFeature, parameters.yFeature, parameters.zFeature,
            parameters.rFeature, parameters.gFeature, parameters.bFeature,
            parameters.scaleFeature
        };
    }

    public void CallUpdate() {
        onFeaturePositionUpdate?.Invoke(PositionFeatures, parameters.posAxisScale);
        onFeatureColorUpdate?.Invoke(ColorFeatures, parameters.useHSV);
        onFeatureScaleUpdate?.Invoke(parameters.scaleFeature, parameters.scaleMult, parameters.scaleExp);
    }

    /// <summary>
    /// Returns a copy of GranularParameters
    /// </summary>
    public GranularParameters GetParameters() {
        return parameters.Copy();
    }
}

