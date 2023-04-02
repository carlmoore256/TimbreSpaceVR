[System.Serializable]
public class GranularParameters {
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
    public float[] posAxisScale = { 0f,0f,0f }; // scale x,y,z axis
    public int ID = 0;

    public GranularParameters Copy() {
        GranularParameters copy = new GranularParameters();
        copy.xFeature = xFeature;
        copy.yFeature = yFeature;
        copy.zFeature = zFeature;
        copy.rFeature = rFeature;
        copy.gFeature = gFeature;
        copy.bFeature = bFeature;
        copy.scaleFeature = scaleFeature;
        copy.windowSize = windowSize;
        copy.hopSize = hopSize;
        copy.scaleMult = scaleMult;
        copy.scaleExp = scaleExp;
        copy.useHSV = useHSV;
        copy.posAxisScale = posAxisScale;
        return copy;
    }
}