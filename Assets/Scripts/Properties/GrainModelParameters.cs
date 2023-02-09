using System;

/// <summary>
/// State manager for GrainModel parameters, invoking callbacks when parameters are changed
/// </summary>
public class GrainModelParameters {
    private Action<AudioFeature[]> onFeaturePosUpdate;
    private Action<AudioFeature[]> onFeatureColUpdate;
    private Action<AudioFeature> onFeatureSclUpdate;
    private Action<int, int> onWindowUpdate; // window size, hop size

    private int _windowSize = 8192;
    private int _hopSize = 8192;
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

    public float ScaleMult { get; set;} = 0.005f;

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

    public AudioFeature[] CurrentlySelected() {
        return new AudioFeature[7] {
            _XFeature, _YFeature, _ZFeature,
            _RFeature, _GFeature, _BFeature,
            _ScaleFeature
        };
    }
}

