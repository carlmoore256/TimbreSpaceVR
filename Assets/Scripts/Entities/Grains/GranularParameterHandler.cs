using System;
using UnityEngine;

/// <summary>
/// State manager for GrainModel parameters, invoking callbacks when parameters are changed
/// </summary>
public class GranularParameterHandler {

    public GranularParameterHandler(
            GranularParameters values,
            Action<AudioFeature[], float[]> onPositionParameterUpdate, 
            Action<AudioFeature[], bool> onColorParameterUpdate,
            Action<AudioFeature, float, float> onScaleParameterUpdate,
            Action<int, int> onWindowUpdate) { 
        this.values = values;
        this.onFeaturePositionUpdate = onPositionParameterUpdate;
        this.onFeatureColorUpdate = onColorParameterUpdate;
        this.onFeatureScaleUpdate = onScaleParameterUpdate;
        this.onWindowUpdate = onWindowUpdate;
    }

    public GranularParameterHandler(GranularParameters values) {
        this.values = values;
    }



    private GranularParameters values;

    public Action<AudioFeature[], float[]> onFeaturePositionUpdate;
    public Action<AudioFeature[], bool> onFeatureColorUpdate;
    public Action<AudioFeature, float, float> onFeatureScaleUpdate;
    public Action<int, int> onWindowUpdate; // window size, hop size

    public AudioFeature[] PositionFeatures { get => new AudioFeature[3] { values.xFeature, values.yFeature, values.zFeature }; }
    public AudioFeature[] ColorFeatures { get => new AudioFeature[3] { values.rFeature, values.gFeature, values.bFeature }; }

    public AudioFeature XFeature { get => values.xFeature; set {
        values.xFeature = value;
        onFeaturePositionUpdate?.Invoke(PositionFeatures, values.posAxisScale);
    } }

    public AudioFeature YFeature { get => values.yFeature; set {
        values.yFeature = value;
        onFeaturePositionUpdate?.Invoke(PositionFeatures, values.posAxisScale);
    } }

    public AudioFeature ZFeature { get => values.zFeature; set {
        values.zFeature = value;
        onFeaturePositionUpdate?.Invoke(PositionFeatures, values.posAxisScale);
    } }

    public AudioFeature[] XYZFeatures { get => new AudioFeature[3] { values.xFeature, values.yFeature, values.zFeature }; }

    public AudioFeature RFeature { get => values.rFeature; set {
        values.rFeature = value;
        onFeatureColorUpdate?.Invoke(ColorFeatures, values.useHSV);
    } }

    public AudioFeature GFeature { get => values.gFeature; set {
        values.gFeature = value;
        onFeatureColorUpdate?.Invoke(ColorFeatures, values.useHSV);
    } }

    public AudioFeature BFeature { get => values.bFeature; set {
        values.bFeature = value;
        onFeatureColorUpdate?.Invoke(ColorFeatures, values.useHSV);
    } }

    public AudioFeature[] RGBFeatures { get => new AudioFeature[3] { values.rFeature, values.gFeature, values.bFeature }; }

    public AudioFeature ScaleFeature { get => values.scaleFeature; set {
        values.scaleFeature = value;
        onFeatureScaleUpdate?.Invoke(value, values.scaleMult, values.scaleExp);
    } }

    public int WindowSize { get => values.windowSize; set { 
        values.windowSize=value;
        onWindowUpdate?.Invoke(value, values.hopSize); 
    } }
    
    public int HopSize { get => values.hopSize; set { 
        values.hopSize=value;
        onWindowUpdate?.Invoke(values.windowSize, value); 
    } }

    public float ScaleMult { get => values.scaleMult; set {
        values.scaleMult = value;
        onFeatureScaleUpdate?.Invoke(values.scaleFeature, value, values.scaleExp);
    } }
    public float ScaleExp { get => values.scaleExp; set {
        values.scaleExp = value;
        onFeatureScaleUpdate?.Invoke(values.scaleFeature, values.scaleMult, value);
    } }

    public bool UseHSV { get => values.useHSV; set {
        values.useHSV = value;
        onFeatureColorUpdate?.Invoke(ColorFeatures, value);
    } }

    public float[] PosAxisScale { get => values.posAxisScale; set {
        values.posAxisScale = value;
        onFeaturePositionUpdate?.Invoke(PositionFeatures, value);
    } }

    /** Set window size by the number of hops per window */
    public void SetWindowByHops(int hopCount) {
        HopSize = (int)(WindowSize /(float)hopCount);
    }

    public AudioFeature[] CurrentFeatures() {
        return new AudioFeature[7] {
            values.xFeature, values.yFeature, values.zFeature,
            values.rFeature, values.gFeature, values.bFeature,
            values.scaleFeature
        };
    }

    public void CallUpdate() {
        onFeaturePositionUpdate?.Invoke(PositionFeatures, values.posAxisScale);
        onFeatureColorUpdate?.Invoke(ColorFeatures, values.useHSV);
        onFeatureScaleUpdate?.Invoke(values.scaleFeature, values.scaleMult, values.scaleExp);
    }
}

