
public struct WindowTime {
    public double startTime;
    public double endTime;
    public double duration;
    public int numSamples;
    public WindowTime(double startTime, double endTime, int numSamples) {
        this.startTime = startTime;
        this.endTime = endTime;
        this.duration = endTime - startTime;
        this.numSamples = numSamples;
    }

    public (int start, int end, int count) GetSampleRange(int sampleRate) {
        var startSample = (int)(startTime * sampleRate);
        var endSample = (int)(endTime * sampleRate);
        var numSamples = endSample - startSample;
        return (startSample, endSample, numSamples);
    }
}
