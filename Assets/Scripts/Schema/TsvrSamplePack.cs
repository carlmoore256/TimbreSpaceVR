


[System.Serializable]
public class TsvrAudioSample {
    public string file;
    public string title;
    public int bytes;
    public float duration;
    public int channels;
    // public float maxDBFS;
    public string resource;
    public GranularParameters granularParameterValues;

    public static TsvrAudioSample FromResourceData(ResourceData resourceData) {
        if (resourceData.type != "audio") {
            throw new System.Exception("ResourceData is not audio");
        }
        if (resourceData.category != ResourceData.ResourceCategory.Sample) {
            throw new System.Exception("ResourceData is not a sample");
        }
        if (resourceData.location != ResourceData.ResourceDataLocation.Package) {
            throw new System.Exception("ResourceData is not local");
        }
        TsvrAudioSample sample = new TsvrAudioSample();
        sample.file = resourceData.uri;
        sample.title = resourceData.hash;
        sample.bytes = resourceData.bytes;
        sample.resource = resourceData.uri;
        return sample;
    }
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
    public TsvrAudioSample[] samples;
}