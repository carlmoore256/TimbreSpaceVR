using UnityEngine;
using System;
using System.Reflection;

public struct ValueRange<T> 
{
    public T MinValue;
    public T MaxValue;

    // public ValueRange(T minValue, T maxValue) {
    //     MinValue = minValue;
    //     MaxValue = maxValue;
    // }
}


/// <summary>
/// Generic Setting type for user changeable variables, with associated callbacks on change
/// </summary>
[System.Serializable]
public class Setting<T>
{
    public T value;
    public string Title { get; }
    public T DefaultValue { get; }
    public string Namespace { get; }
    public Action<T> onChange;
    public void Reset() {
        value = DefaultValue;
    }
    public virtual void SetValue(T newValue) {
        value = newValue;
        onChange?.Invoke(newValue);
    }

    public Setting(string title, T defaultValue, string _namespace="General") {
        value = defaultValue;
        Title = title;
        DefaultValue = defaultValue;
        Namespace = _namespace;
        onChange += (T newValue) => {
            Debug.Log("Setting " + Title + " changed to " + newValue);
        };
    }

    public string ToJson() {
        return JsonUtility.ToJson(new {
            value = value,
            Title = Title,
            DefaultValue = DefaultValue,
            Namespace = Namespace
        });
    }

    public void AddListener(Action<T> listener) => onChange += listener;
    public void RemoveListener(Action<T> listener) => onChange -= listener;
    public static implicit operator T(Setting<T> setting) => setting.value;

    [System.Serializable]
    private class JsonSetting<U> {
        public string Type;
        public U Value;
        public string Title;
        public string Namespace;
    }

    public static Setting<T> FromJson(string json) {
        JsonSetting<T> data = JsonUtility.FromJson<JsonSetting<T>>(json);
        Type type = Type.GetType(data.Type);
        var value = (T)Convert.ChangeType(data.Value, type);
        var setting = new Setting<T>(data.Title, value, data.Namespace);
        return setting;
    }
}

public class SettingIntDiscrete : Setting<int>
{
    public int[] DiscreteValues { get; set; }

    public SettingIntDiscrete(
        string title, 
        int defaultValue, 
        int[] discreteValues, 
        string _namespace = "General")
        : base(title, defaultValue, _namespace)
    {
        DiscreteValues = discreteValues;
    }
}

public class SettingFloatRange : Setting<float>
{
    public ValueRange<float> ValueRange { get; set; }
    public float Exponential { get; set; }

    public SettingFloatRange(
        string title, 
        float defaultValue, 
        ValueRange<float> valueRange, 
        float exponential = 1f, 
        string _namespace = "General")
        : base(title, defaultValue, _namespace)
    {
        ValueRange = valueRange;
        Exponential = exponential;
    }

    public override void SetValue(float newValue)
    {
        value = Mathf.Pow(Exponential, newValue);
        onChange?.Invoke(value);
    }
}


/// <summary>
/// User accessible settings that the player can change. Settings will be saved on close, and loaded on open
/// </summary>
[CreateAssetMenu(fileName = "TSVR/AppSettings", menuName = "App Settings")]
public class AppSettings : ScriptableObject
{
    public Setting<float> WandDistIncrement = new Setting<float>("Wand: Distance Increment",  0.5f, "Tools/Wand");
    public Setting<float> WandSizeIncrement = new Setting<float>("Wand: Size Increment", 5f, "Tools/Wand");
    public Setting<float> WandMaxDist = new Setting<float>("Wand: Maximum Distance", 30f, "Tools/Wand");
    public Setting<float> WandMinDist = new Setting<float>("Wand: Maximum Distance", 0.01f, "Tools/Wand");
    public Setting<float> WandMaxRadius = new Setting<float>("Wand: Maximum Radius", 5f, "Tools/Wand");
    public Setting<float> WandMinRadius = new Setting<float>("Wand: Minimum Radius", 0.01f, "Tools/Wand");
    public Setting<int> WandLineSegments = new Setting<int>("Wand: Line Draw Segments", 10, "Tools/Wand");
    public Setting<float> WandLineElasticity = new Setting<float>("Wand: Line Elasticity", 1f, "Tools/Wand");
    public Setting<bool> EnableElasticWand = new Setting<bool>("Enable Elastic Wand", false, "Tools/Wand");
    public Setting<bool> InterfaceSounds = new Setting<bool>("Interface Sounds Enabled", true, "Interface");
    public Setting<float> InterfaceSoundsGain = new Setting<float>("Interface Sounds Gain", 0.3f, "Interface");
    public Setting<float> ParticleTolerance = new Setting<float>("Particle Tolerance", 0.01f, "Grains");
    public Setting<double> GrainPlayTimeout = new Setting<double>("Grain Play Timeout", 0.01f, "Grains");
    public Setting<float> GrainMaxRadius = new Setting<float>("Grain Max Radius", 0.5f, "Grains");
    public Setting<float> GrainMinRadius = new Setting<float>("Grain Max Radius", 0.001f, "Grains");
    public Setting<bool> GrainUseHSV = new Setting<bool>("Use HSV Color", false, "Grains");
    public Setting<bool> DebugLogging = new Setting<bool>("Enable Debug Logs", true, "Developer");
    public Setting<bool> EnableXRLogger = new Setting<bool>("Enable XR Debug Logger", true, "Developer");
    
    public Setting<float> AudioDbThreshold = new Setting<float>("Audio Db Threshold", -30f, "Audio/Analysis");
    public Setting<int> AudioDbThresholdWindow = new Setting<int>("Audio Db Threshold Window Size", 1024, "Audio/Analysis");

    public Setting<int> ModelMaxGrains = new Setting<int>("Max grains", 5000, "Model");


    // public void Load() {
    //     string json = PlayerPrefs.GetString("AppSettings");
    //     if (!string.IsNullOrEmpty(json)) {
    //         var data = JsonUtility.FromJson<AppSettings>(json);
    //         WandDistIncrement.value = data.WandDistIncrement;
    //         WandLineSegments.value = data.WandLineSegments;
    //         EnableDebug.value = data.EnableDebug;
    //     }
    // }
    public void Load()
    {
        // Load the settings from PlayerPrefs
        // if (PlayerPrefs.HasKey("AppSettings"))
        // {
        //     string json = PlayerPrefs.GetString("AppSettings");
        //     JsonUtility.FromJsonOverwrite(json, this);

        //     // Load each setting using reflection
        //     FieldInfo[] fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        //     foreach (FieldInfo field in fields)
        //     {
        //         // Check if the field is a Setting<T>
        //         if (field.FieldType.IsSubclassOf(typeof(Setting<>)))
        //         {
        //             // Load the setting
        //             Setting<object> setting = (Setting<object>)field.GetValue(this);
        //             setting.Load();
        //         }
        //     }
        // }
    }

    public void Save() {
        string json = JsonUtility.ToJson(this);
        PlayerPrefs.SetString("AppSettings", json);
        PlayerPrefs.Save();
    }
}
















// public float _wandDistIncrement = 0.05f;
// public float _wandSizeIncrement = 0.1f;
// public float _wandMaxDist = 10f;
// public float _wandMinDist = 0.1f;
// public float _wandMaxRadius = 5f;
// public float _wandMinRadius = 0.01f;
// public int _wandLineSegments = 10;
// public float _wandLineElasticity = 0.5f;
// public float _wandLineFriction = 0.5f;
// public float _particleTolerance = 0.1f;
// public bool _enableElasticWand = false;
// public double _grainPlayTimeout = 0.1;
// public bool _enableDebug = true;
// public bool _grainUseHSV = false;