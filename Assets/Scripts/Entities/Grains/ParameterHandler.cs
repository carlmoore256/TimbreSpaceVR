using System;
using UnityEngine;

[System.Serializable]
public class GrainCloudParameterValues {
    public AudioFeature xFeature = AudioFeature.MFCC_0;
    public AudioFeature yFeature = AudioFeature.MFCC_1;
    public AudioFeature zFeature = AudioFeature.MFCC_2;
    public AudioFeature rFeature = AudioFeature.MFCC_3;
    public AudioFeature gFeature = AudioFeature.MFCC_4;
    public AudioFeature bFeature = AudioFeature.MFCC_5;
    public AudioFeature scaleFeature = AudioFeature.RMS;
    public int windowSize = 8192;
    public int hopSize = 8192;
    public float scaleMult = 0.01f;
    public float scaleExp = 0.1f;
    public bool useHSV = false;
    public Vector3 posAxisScale = Vector3.one; // scale x,y,z axis
}

/// <summary>
/// State manager for GrainModel parameters, invoking callbacks when parameters are changed
/// </summary>
public class ParameterHandler {

    public ParameterHandler(
            GrainCloudParameterValues values,
            Action<AudioFeature[], Vector3> onPositionParameterUpdate, 
            Action<AudioFeature[], bool> onColorParameterUpdate,
            Action<AudioFeature> onScaleParameterUpdate,
            Action<int, int> onWindowUpdate) { 
        this.values = values;
        this.onFeaturePosUpdate = onPositionParameterUpdate;
        this.onFeatureColUpdate = onColorParameterUpdate;
        this.onFeatureSclUpdate = onScaleParameterUpdate;
        this.onWindowUpdate = onWindowUpdate;
    }

    public ParameterHandler(GrainCloudParameterValues values) {
        this.values = values;
    }



    private GrainCloudParameterValues values;

    public Action<AudioFeature[], Vector3> onFeaturePosUpdate;
    public Action<AudioFeature[], bool> onFeatureColUpdate;
    public Action<AudioFeature> onFeatureSclUpdate;
    public Action<int, int> onWindowUpdate; // window size, hop size

    public AudioFeature[] PositionFeatures { get => new AudioFeature[3] { values.xFeature, values.yFeature, values.zFeature }; }
    public AudioFeature[] ColorFeatures { get => new AudioFeature[3] { values.rFeature, values.gFeature, values.bFeature }; }

    public AudioFeature XFeature { get => values.xFeature; set {
        values.xFeature = value;
        onFeaturePosUpdate?.Invoke(PositionFeatures, values.posAxisScale);
    } }

    public AudioFeature YFeature { get => values.yFeature; set {
        values.yFeature = value;
        onFeaturePosUpdate?.Invoke(PositionFeatures, values.posAxisScale);
    } }

    public AudioFeature ZFeature { get => values.zFeature; set {
        values.zFeature = value;
        onFeaturePosUpdate?.Invoke(PositionFeatures, values.posAxisScale);
    } }

    public AudioFeature RFeature { get => values.rFeature; set {
        values.rFeature = value;
        onFeatureColUpdate?.Invoke(ColorFeatures, values.useHSV);
    } }

    public AudioFeature GFeature { get => values.gFeature; set {
        values.gFeature = value;
        onFeatureColUpdate?.Invoke(ColorFeatures, values.useHSV);
    } }

    public AudioFeature BFeature { get => values.bFeature; set {
        values.bFeature = value;
        onFeatureColUpdate?.Invoke(ColorFeatures, values.useHSV);
    } }

    public AudioFeature ScaleFeature { get => values.scaleFeature; set {
        values.scaleFeature = value;
        onFeatureSclUpdate?.Invoke(value);
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
        onFeatureSclUpdate?.Invoke(values.scaleFeature);
    } }
    public float ScaleExp { get => values.scaleExp; set {
        values.scaleExp = value;
        onFeatureSclUpdate?.Invoke(values.scaleFeature);
    } }

    public bool UseHSV { get => values.useHSV; set {
        values.useHSV = value;
        onFeatureColUpdate?.Invoke(ColorFeatures, value);
    } }

    public Vector3 PosAxisScale { get => values.posAxisScale; set {
        values.posAxisScale = value;
        onFeaturePosUpdate?.Invoke(PositionFeatures, value);
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
}

