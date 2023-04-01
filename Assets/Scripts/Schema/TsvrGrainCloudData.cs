using System;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Linq;
using System.Threading.Tasks;

[Serializable]
public class GrainCloudMetadata
{
    public string title;
    public string description;
    public string hash;
    public CreatorData creator;
    public GranularParameters parameters;
    public List<ResourceData> resources;
    public SessionData session;

    public List<ResourceData> QueryResourceData(Func<ResourceData, bool> predicate)
    {
        return resources.Where(predicate).ToList();
    }

    public async Task<ResourceData> GetLocalResourceData(ResourceData.ResourceCategory category)
    {
        Debug.Log("Getting local resource data for category: " + category);
        var package = QueryResourceData((ResourceData resource) => {
            return resource.category == category && resource.location == ResourceData.ResourceDataLocation.Package;
        }).FirstOrDefault();

        if (package != null) {
            Debug.Log("Found package: " + package.uri);
            return package;
        }

        var appdata = QueryResourceData((ResourceData resource) => {
            return resource.category == category && resource.location == ResourceData.ResourceDataLocation.AppData;
        }).FirstOrDefault();

        if (appdata != null) {
            Debug.Log("Found appdata: " + appdata.uri);
            return appdata;
        }

        // if we get here, we may need to download, or at least update the list of Resources to have
        // the correct cache ResourceData, which exists but for some reason isn't in the list here
        Debug.Log("Downloading: " + category);

        var web = QueryResourceData((ResourceData resource) => {
            return resource.category == category && resource.location == ResourceData.ResourceDataLocation.Web;
        }).FirstOrDefault();

        if (web != null) {
            string appDataFilepath = await AppData.DownloadOrGetCachedPath(web.uri, hash, web.hash, AppDataCategory.Downloads);
            var newResource = new ResourceData(
                web.type, web.category, 
                ResourceData.ResourceDataLocation.AppData, 
                appDataFilepath, web.hash, web.bytes);
            resources.Add(newResource);
            return newResource;
        }

        return null;
    }

    // check if any resources don't exist in cache, download if not save back to the json
    public void CacheAllResourceData() {

        // get a set of unique hashes for each resource
        var hashes = resources.Select((ResourceData resource) => {
            return resource.hash;
        }).Distinct();
        
        // check if each hash exists in cache
        foreach (string hash in hashes) {

            // for this hash, see if there is either a package or appdata
            // if not, download

            List<ResourceData> matchingResources = resources.FindAll((ResourceData resource) => {
                    return resource.hash == hash;
            });

            var package = matchingResources.Find(r => r.location == ResourceData.ResourceDataLocation.Package);
            if (package != null) {
                Debug.Log("Package exists: " + package.uri);
                continue;
            }

            var appdata = matchingResources.Find(r => r.location == ResourceData.ResourceDataLocation.AppData);
            if (appdata != null) {
                Debug.Log("AppData exists: " + appdata.uri);
                continue;
            }

            // if we get here, we may need to download, or at least update the list of Resources to have
            // the correct cache ResourceData, which exists but for some reason isn't in the list here
            Debug.Log("Downloading: " + hash);

            var web = matchingResources.Find(r => r.location == ResourceData.ResourceDataLocation.Web);
            if (web != null) {
                if (!AppData.Exists(hash, web.uri, AppDataCategory.Downloads)) {
                    Debug.Log("Downloading: " + web.uri);
                    AppData.Download(web.uri, hash, web.hash, AppDataCategory.Downloads, (string appDataFilepath) => {
                        Debug.Log("Downloaded: " + appDataFilepath);
                        var newResource = new ResourceData(web.type, web.category, ResourceData.ResourceDataLocation.AppData, appDataFilepath, web.hash, web.bytes);
                        resources.Add(newResource);
                    });
                } else {
                    // load from appdata
                    string existingURI = AppData.GetAppDataSubFilepath(hash, web.uri, AppDataCategory.Downloads);
                    var newResource = new ResourceData(web.type, web.category, ResourceData.ResourceDataLocation.AppData, existingURI, web.hash, web.bytes);
                    resources.Add(newResource);
                }
            }
        }
    }
}


[Serializable]
public class SessionData 
{
    public SequenceData[] sequences;
}

[Serializable]
public class SequenceData
{
    public int seqId; // supports multiple sequences
    public int nodeId; // how to index the AudioNode/Grain
    public double time; // time within sequence
    public float gain; // volume of audio at sequence step
}




[Serializable]
public class CreatorData
{
    public string name;
    public string website;
}
