using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Threading.Tasks;
using System.Net;

public enum AppDataFileType
{
    Audio,
    Image,
    Video,
    Text,
    Other
}

public enum AppDataCategory
{
    User,
    Cache,
    Downloads,
    Logs,
    None
}

public static class AppData
{
    private static readonly string appDataRoot;

    private static readonly Dictionary<AppDataFileType, string> fileExtensions = new Dictionary<AppDataFileType, string>()
    {
        { AppDataFileType.Audio, ".wav" },
        { AppDataFileType.Image, ".png" },
        { AppDataFileType.Video, ".mp4" },
        { AppDataFileType.Text, ".txt" },
        { AppDataFileType.Other, ".dat" },
    };

    private static readonly Dictionary<AppDataCategory, string> categoryPaths;

    static AppData()
    {
        // Create an app data directory under persistentDataPath
        appDataRoot = Path.Combine(Application.persistentDataPath);
        categoryPaths = new Dictionary<AppDataCategory, string>();

        foreach (AppDataCategory location in Enum.GetValues(typeof(AppDataCategory)))
        {
            if (location == AppDataCategory.None)
            {
                categoryPaths[location] = Application.persistentDataPath;
                continue;
            }
            string locationName = location.ToString();
            string directoryPath = Path.Combine(Application.persistentDataPath, locationName);
            categoryPaths[location] = directoryPath;
            Directory.CreateDirectory(directoryPath);
            Debug.Log($"Created directory for {locationName}: {directoryPath}");
        }
    }

    // , bool downloadIfNot = false
    public static bool Exists(string folder, string filename, AppDataCategory category = AppDataCategory.None)
    {
        string filePath = GetAppDataSubFilepath(folder, filename, category);
        return File.Exists(filePath);
    }

    public static string GetAppDataSubFilepath(string folder, string filename, AppDataCategory category = AppDataCategory.None, bool createDirectory = true)
    {
        string directoryPath = Path.Combine(categoryPaths[category], folder);
        if (createDirectory)
        {
            CheckMakeDirectory(directoryPath);
        }
        return Path.Combine(directoryPath, filename);
    }

    public static void SaveFileRaw(string relativePath, byte[] fileData, AppDataCategory category = AppDataCategory.None)
    {  
        string filePath = Path.Combine(categoryPaths[category], relativePath);
        File.WriteAllBytes(filePath, fileData);
    }

    public static byte[] ReadFileRaw(string fileName, AppDataCategory category = AppDataCategory.None)
    {
        string filePath = Path.Combine(categoryPaths[category], fileName);
        if (File.Exists(filePath))
        {
            return File.ReadAllBytes(filePath);
        }
        Debug.LogError($"File not found in app data: {fileName}");
        return null;
    }

    public static void SaveFileObject<T>(string relativePath, T data, AppDataCategory category = AppDataCategory.None)
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        using (FileStream fileStream = new FileStream(Path.Combine(categoryPaths[category], relativePath), FileMode.Create))
        {
            binaryFormatter.Serialize(fileStream, data);
        }
    }

    public static T LoadFileObject<T>(string relativePath, AppDataCategory category = AppDataCategory.None)
    {
        if (File.Exists(relativePath))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (FileStream fileStream = new FileStream(relativePath, FileMode.Open))
            {
                T data = (T)binaryFormatter.Deserialize(fileStream);
                return data;
            }
        }

        return default(T);
    }

    public static void CheckMakeDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    public static void SaveFileJson<T>(T data, string folder, string filename, AppDataCategory category = AppDataCategory.None)
    {
        // fileName = GetAppDataPath(fileName, folder, category);
        // string filePath = Path.Combine(categoryPaths[category], folder, filename);
        string filepath = GetAppDataSubFilepath(folder, filename, category, true);
        string jsonData = JsonUtility.ToJson(data);
        Debug.Log("Saving JSON " + filename + " to " + filepath);
        File.WriteAllText(filepath, jsonData);
    }

    public static T LoadFileJson<T>(string folder, string filename, AppDataCategory category = AppDataCategory.None)
    {
        // string filePath = Path.Combine(categoryPaths[category], relativePath);
        // filename = GetAppDataPath(filename, category);
        string filepath = Path.Combine(categoryPaths[category], folder, filename);
        if (File.Exists(filepath))
        {
            string jsonData = File.ReadAllText(filepath);
            T data = JsonUtility.FromJson<T>(jsonData);
            return data;
        }

        return default(T);
    }

    public static async void Download(string url, string folder, string filename, AppDataCategory category, Action<string> callback = null) {
        string filepath = GetAppDataSubFilepath(folder, filename, category, true);
        
        using (var webClient = new WebClient())
        {
            await webClient.DownloadFileTaskAsync(url, filepath);
        }

        callback?.Invoke(filepath);
    }


    public static async Task<string> DownloadOrGetCachedPath(string url, string folder, string filename, AppDataCategory category) {
        string filepath = GetAppDataSubFilepath(folder, filename, category, true);

        if (File.Exists(filepath))
        {
            return filepath;
        }

        using (var webClient = new WebClient())
        {
            await webClient.DownloadFileTaskAsync(url, filepath);
        }

        return filepath;
    }



    public static async Task<T> DownloadOrGetCached<T>(string url, string folder, string filename, AppDataCategory category = AppDataCategory.Downloads)
    {
        string filepath = GetAppDataSubFilepath(folder, filename, category, true);

        if (!File.Exists(filepath))
        {
            using (var webClient = new WebClient())
            {
                await webClient.DownloadFileTaskAsync(url, filepath);
            }
        }

        if (File.Exists(filepath))
        {
            return LoadFileObject<T>(filepath);
        }

        return default(T);
    }

    /// <summary>
    /// Creates a hash of key, and returns a full path to the directory
    /// </summary>
    public static string CreateHashDirectory(string key, AppDataCategory category = AppDataCategory.None) {
        // hash key, and create a directory at the location
        string hash = key.GetHashCode().ToString();
        string directoryPath = Path.Combine(categoryPaths[category], hash);
        // Directory.CreateDirectory(directoryPath);
        CheckMakeDirectory(directoryPath);
        return directoryPath;
    }

    // public static async Task<ResourceData> DownloadOrGetCachedResourceData(ResourceData resourceData, string folder, AppDataCategory category = AppDataCategory.Downloads)
    // {
    //     string filepath = GetAppDataSubFilepath(folder, resourceData.filename, category, true);

    //     if (!File.Exists(filepath))
    //     {
    //         using (var webClient = new WebClient())
    //         {
    //             await webClient.DownloadFileTaskAsync(resourceData.url, filepath);
    //         }
    //     }

    //     if (File.Exists(filepath))
    //     {
    //         resourceData.filepath = filepath;
    //         return resourceData;
    //     }

    //     return null;
    // }
}

