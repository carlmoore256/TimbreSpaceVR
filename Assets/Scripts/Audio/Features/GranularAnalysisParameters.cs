using NWaves.Windows;

public struct GranularAnalysisParameters {
    public AudioFeature feature;
    public WindowTypes windowType;
    public int windowSize;
    public int hopSize;
}