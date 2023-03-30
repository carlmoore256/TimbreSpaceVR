using System;
using System.Collections.Generic;
using UnityEngine;
using System.Net;

[Serializable]
public class TsvrGrainCloudData
{
    public string title;
    public string description;
    public string hash;
    public CreatorData creator;
    public GranularParameters parameters;
    public List<ResourceData> resources;
    public SessionData session;
}


[Serializable]
public class SessionData 
{
    public SequenceData[] sequences;
}

[Serializable]
public class SequenceData
{

}




[Serializable]
public class ResourceData
{
    public enum ResourceDataLocation {
        Package,
        AppData,
        Web,
    }

    public enum ResourceCategory {
        Sample,
        Thumbnail,
        Package,
    }

    // Mime.Type 
    public string type;
    public ResourceCategory category;
    public ResourceDataLocation location; // depending on the platform, choose the most ideal location
    public string uri;
    public string hash; // used to distinguish media
    public int bytes;

    public ResourceData() {}

    public ResourceData(string type, ResourceCategory category, ResourceDataLocation location, string uri, string hash, int bytes) {
        this.type = type;
        this.category = category;
        this.location = location;
        this.uri = uri;
        this.hash = hash;
        this.bytes = bytes;
    }

    public static ResourceData FromResourceDataLocation(ResourceDataLocation location, string uri, string hash, int bytes) {
        ResourceData resourceData = new ResourceData();
        resourceData.location = location;
        resourceData.uri = uri;
        resourceData.hash = hash;
        resourceData.bytes = bytes;
        return resourceData;
    }
}

[Serializable]
public class CreatorData
{
    public string name;
    public string website;
}
