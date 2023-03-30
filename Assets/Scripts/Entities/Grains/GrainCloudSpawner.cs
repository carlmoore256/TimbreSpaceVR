using UnityEngine;
using System.Collections;
using NWaves.Signals;
using System.Threading.Tasks;

public class GrainCloudSpawner : MonoBehaviour {
    
    public static GrainCloudSpawner _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if(_instance == null) {
            if (GameObject.Find("GrainParent") != null)
                _instance = GameObject.Find("GrainParent").GetComponent<GrainCloudSpawner>();
            else
                _instance = new GameObject("GrainParent").AddComponent<GrainCloudSpawner>();
            DontDestroyOnLoad(_instance.gameObject);
        }
    }

    private static GameObject SpawnPrefab(string resourcePath = "Prefabs/GrainCloud") {
        var prefab = Resources.Load<GameObject>(resourcePath);
        return Instantiate(prefab, GameObject.Find("GrainParent").transform);
    }

    # region Spawn Methods

    public static GrainCloud Spawn(TsvrAudioSample sample) {
        GameObject cloudObject = SpawnPrefab();
        GrainCloud cloud = cloudObject.GetOrAddComponent<GrainCloud>();
        AudioIO.LoadAudioFromResources(sample.resource, (DiscreteSignal audioBuffer) => {
            AudioFilters.StripSilenceAsync(audioBuffer, -30, 1024, (DiscreteSignal strippedSignal) => {
                cloud.Initialize(audioBuffer, sample.granularParameterValues);
            });
            // cloud.Initialize(audioBuffer, sample.granularParameterValues);
        });
        return cloud;
    }

    public static GrainCloud Spawn(DiscreteSignal audioBuffer, GranularParameters parameters = null) {
        GameObject cloudObject = SpawnPrefab();
        GrainCloud cloud = cloudObject.GetOrAddComponent<GrainCloud>();
        cloud.Initialize(audioBuffer, parameters);
        return cloud;
    }

    public static GrainCloud Spawn(string resourcePath, GranularParameters parameters = null) {
        GameObject cloudObject = SpawnPrefab();
        GrainCloud cloud = cloudObject.GetOrAddComponent<GrainCloud>();
        AudioIO.LoadAudioFromResources(resourcePath, (DiscreteSignal audioBuffer) => {
            AudioFilters.StripSilenceAsync(audioBuffer, -30, 1024, (DiscreteSignal strippedSignal) => {
                cloud.Initialize(strippedSignal, parameters);
            });
            // cloud.Initialize(audioBuffer, parameters);
        });
        return cloud;
    }

    /// <summary>
    /// Spawns a grain cloud from a web URI pointing to an audio file.
    /// </summary>
    public static GrainCloud SpawnFromURI(string uri) {
        GameObject cloudObject = SpawnPrefab();
        GrainCloud cloud = cloudObject.GetOrAddComponent<GrainCloud>();
        AudioIO.LoadAudioFromURI(uri).ContinueWith(task => {
            DiscreteSignal audioBuffer = task.Result;
            AudioFilters.StripSilenceAsync(audioBuffer, -30, 1024, (DiscreteSignal strippedSignal) => {
                cloud.Initialize(strippedSignal);
            });
            // cloud.Initialize(audioBuffer);
        });
        return cloud;
    }


    public static async Task<GrainCloud> SpawnFromMetadataURI(string uri) {

        
        TsvrGrainCloudData grainCloudData = await JsonDownloader.Download<TsvrGrainCloudData>(uri);
        Debug.Log("tsvrSample: " + grainCloudData);
        if (grainCloudData == null) return null;

        // first check if the resources have already been downloaded

        // tsvrSample.resource = AppDataCategory category = AppDataCategory.None;
        // get the hash path of the resource

        string dir = AppData.CreateHashDirectory(grainCloudData.hash, AppDataCategory.Downloads);

        // save to a new directory in persistentData
        AppData.SaveFileJson<TsvrGrainCloudData>(grainCloudData, grainCloudData.title, AppDataCategory.Downloads);
        return Spawn(grainCloudData);
        // return null;
    }


    # endregion

    public static GrainCloud MergeClouds() {
        return null;
    }
}