using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System;

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

    public static void SaveFileJson<T>(T data, string filePath, AppDataCategory category = AppDataCategory.None)
    {
        string jsonData = JsonUtility.ToJson(data);
        File.WriteAllText(filePath, jsonData);
    }

    public static T LoadFileJson<T>(string filePath, AppDataCategory category = AppDataCategory.None)
    {
        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            T data = JsonUtility.FromJson<T>(jsonData);
            return data;
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
        Directory.CreateDirectory(directoryPath);
        return directoryPath;
    }
}

