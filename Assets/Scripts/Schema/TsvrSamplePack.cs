[System.Serializable]
public class TsvrSample {
    public string file;
    public string title;
    public int bytes;
    public float duration;
    public int channels;
    public float maxDBFS;
}


[System.Serializable]
public class TsvrSamplePackMetadata {
    public string title;
    public string id;
    public string creator;
    public string date;
    public int numSamples;
}


[System.Serializable]
public class TsvrSamplePack {
    public TsvrSamplePackMetadata metadata;
    public TsvrSample[] samples;
}