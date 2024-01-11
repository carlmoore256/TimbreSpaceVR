


[System.Serializable]
public class TsvrAudioSample {
    public ResourceData resourceData;
    public string file;
    public string title;
    public float duration;
    public int channels;
    public string resource;


    public int Bytes { get { 
        if (resourceData == null)
            return 0;
        else
            return resourceData.bytes;
    } }


    // public float maxDBFS;
    
    public GranularParameters granularParameterValues;

    public TsvrAudioSample() { }

    public TsvrAudioSample(ResourceData resourceData, string title) {
        this.resourceData = resourceData;
        this.file = resourceData.uri;
        this.resource = resourceData.uri;
        this.title = title;
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