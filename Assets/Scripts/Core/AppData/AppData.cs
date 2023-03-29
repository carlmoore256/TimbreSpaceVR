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
            string locationName = location.ToString();
            string directoryPath = Path.Combine(Application.persistentDataPath, locationName);
            categoryPaths[location] = directoryPath;
            Directory.CreateDirectory(directoryPath);
            Debug.Log($"Created directory for {locationName}: {directoryPath}");
        }
    }

    public static void SaveFileRaw(AppDataCategory category, string relativePath, byte[] fileData)
    {  
        string filePath = Path.Combine(categoryPaths[category], relativePath);
        File.WriteAllBytes(filePath, fileData);
    }

    public static byte[] ReadFileRaw(AppDataCategory category, string fileName)
    {
        string filePath = Path.Combine(categoryPaths[category], fileName);
        if (File.Exists(filePath))
        {
            return File.ReadAllBytes(filePath);
        }
        Debug.LogError($"File not found in app data: {fileName}");
        return null;
    }

    public static void SaveFileObject<T>(AppDataCategory category, string relativePath, T data)
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        using (FileStream fileStream = new FileStream(Path.Combine(categoryPaths[category], relativePath), FileMode.Create))
        {
            binaryFormatter.Serialize(fileStream, data);
        }
    }

    public static T LoadFileObject<T>(AppDataCategory category, string relativePath)
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

    public static void SaveFileJson<T>(T data, string filePath)
    {
        string jsonData = JsonUtility.ToJson(data);
        File.WriteAllText(filePath, jsonData);
    }

    public static T LoadFileJson<T>(string filePath)
    {
        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            T data = JsonUtility.FromJson<T>(jsonData);
            return data;
        }

        return default(T);
    }
}

