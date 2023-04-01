using System;
using System.Collections.Generic;
// using System.Threading.Tasks;

[Serializable]
public class ResourceData
{
    public enum ResourceDataLocation
    {
        Package,
        AppData,
        Web,
    }

    public enum ResourceCategory
    {
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

    public ResourceData() { }

    public ResourceData(string type, ResourceCategory category, 
                        ResourceDataLocation location, string uri, 
                        string hash, int bytes)
    {
        this.type = type;
        this.category = category;
        this.location = location;
        this.uri = uri;
        this.hash = hash;
        this.bytes = bytes;
    }


    public static ResourceData Query(List<ResourceData> resources, Func<ResourceData, bool> predicate)
    {
        foreach (ResourceData resource in resources)
        {
            if (predicate(resource))
            {
                return resource;
            }
        }
        return null;
    }
    
    // public static async Task<ResourceData> GetLocal(List<ResourceData> resources, ResourceCategory category)
    // {
    //     var package = Query(resources, (ResourceData resource) => {
    //         return resource.category == category && resource.location == ResourceDataLocation.Package;
    //     });

    //     if (package != null)
    //     {
    //         return package;
    //     }

    //     var appData = Query(resources, (ResourceData resource) => {
    //         return resource.category == category && resource.location == ResourceDataLocation.AppData;
    //     });

    //     if (appData != null)
    //     {
    //         return appData;
    //     }

    //     // if not, download to appdata and return a new ResourceData at that location
    //     AppData.DownloadToAppData(resources, category);
    // }

    // returns a new ResourceData with the new location
    // public ResourceData DownloadToAppData()
    // {
    //     if (location != ResourceDataLocation.Web)
    //     {
    //         return null;
    //     }
    // }

    // public static ResourceData FromResourceDataLocation(ResourceDataLocation location, string uri, string hash, int bytes) {
    //     ResourceData resourceData = new ResourceData();
    //     resourceData.location = location;
    //     resourceData.uri = uri;
    //     resourceData.hash = hash;
    //     resourceData.bytes = bytes;
    //     return resourceData;
    // }
}