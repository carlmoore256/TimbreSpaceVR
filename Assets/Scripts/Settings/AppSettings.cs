using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;


public class Setting {
    public string Title { get; }
    public dynamic Value { get; set; }
    public dynamic Type { get; set; }
    public dynamic DefaultValue { get; }
    public string Category { get; }
    public void Reset() => Value = DefaultValue;

    public Setting(string title, dynamic defaultValue, string category="General") {
        Title = title;
        Value = defaultValue;
        Type = defaultValue.GetType();
        DefaultValue = defaultValue;
        Category = category;
    }
}

// User accessible settings that the player can change
[CreateAssetMenu(fileName = "TSVR/AppSettings", menuName = "App Settings (Singleton)")]
public class AppSettings : SingletonScriptableObject<AppSettings>
{
    public float wandDistIncrement { get { return _wandDistIncrement.Value; } set { _wandDistIncrement.Value = value; } }
    public float wandSizeIncrement { get { return _wandSizeIncrement.Value; } set { _wandSizeIncrement.Value = value; } }
    public float WandMaxDist { get { return _wandMaxDist.Value; } set { _wandMaxDist.Value = value; } }
    public float WandMinDist { get { return _wandMinDist.Value; } set { _wandMinDist.Value = value; } }
    public float WandMaxRadius { get { return _wandMaxRadius.Value; } set { _wandMaxRadius.Value = value; } }
    public float WandMinRadius { get { return _wandMinRadius.Value; } set { _wandMinRadius.Value = value; } }
    public int WandLineSegments { get { return _wandLineSegments.Value; } set { _wandLineSegments.Value = value; } }
    public float WandLineElasticity { get { return _wandLineElasticity.Value; } set { _wandLineElasticity.Value = value; } }
    public float ParticleTolerance { get { return _particleTolerance.Value; } set { _particleTolerance.Value = value; } }
    public bool EnableElasticWand { get { return _enableElasticWand.Value; } set { _enableElasticWand.Value = value; } }
    public double GrainPlayTimeout { get { return _grainPlayTimeout.Value; } set { _grainPlayTimeout.Value = value; } }
    public bool EnableDebug { get { return _enableDebug.Value; } set { _enableDebug.Value = value; } }
    public bool GrainUseHSV { get { return _grainUseHSV.Value; } set { _grainUseHSV.Value = value; } }


    // eventually combine the public values with these settings
    private Setting _wandDistIncrement { get => new Setting ("Wand: Distance Multiplier",  10f); }
    private Setting _wandSizeIncrement { get => new Setting ("Wand: Size Multiplier", 5f); }
    private Setting _wandMaxDist { get => new Setting ("Wand: Maximum Distance", 30f); }
    private Setting _wandMinDist { get => new Setting ("Wand: Maximum Distance", 0.01f); }
    public Setting _wandMaxRadius { get => new Setting ("Wand: Maximum Radius", 5f); }
    private Setting _wandMinRadius { get => new Setting ("Wand: Minimum Radius", 0.01f); }
    private Setting _wandLineSegments { get => new Setting ("Wand: Line Draw Segments", 10); }
    private Setting _wandLineElasticity { get => new Setting ("Wand: Line Elasticity", 1f); }
    private Setting _interfaceSounds { get => new Setting("Interface Sounds Enabled", true); }
    private Setting _interfaceSoundsGain { get => new Setting("Gain: Interface Sounds", 0.3f); }
    private Setting _particleTolerance { get => new Setting("Particle Tolerance", 0.01f); }
    private Setting _enableElasticWand { get => new Setting("Enable Elastic Wand", false); }
    private Setting _grainPlayTimeout { get => new Setting("Grain Play Timeout", 0.1); }

    private Setting _enableDebug { get => new Setting("Enable Debug", true, "Developer"); }

    private Setting _grainUseHSV { get => new Setting("Use HSV Color", false, "Grains"); }
    


    // public Setting GetSetting(string title) {
    //     // var setting = typeof(AppSettings).GetField($"_{title.Replace(" ", "")}");
    //     // Debug.Log("GOT SETTING " + setting);
    //     var setting = GetType().GetField($"_{title.Replace(" ", "")}");
    //     if (setting != null) {
    //         Debug.Log("GOT SETTING!");
    //         return (Setting)setting.GetValue(this);
    //     }
    //     return null;
    // }

    // public Dictionary<string, object> GetAllSettings() {
    //     Dictionary<string, object> settings = new Dictionary<string, object>();
    //     var type = GetType();
        
    //     foreach (PropertyInfo prop in type.GetProperties()) {
    //         Debug.Log("PROP " + prop.Name);
    //         // if (prop.FieldType == typeof(Setting)) {
    //         //     settings.Add(field.Name, field.GetValue(this));
    //         // }
    //     }
    //     return settings;
    // }
}