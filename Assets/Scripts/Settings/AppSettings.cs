using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Setting {
    public string Title { get; }
    public dynamic Value { get; set; }
    public dynamic Type { get; set; }
    public dynamic DefaultValue { get; }
    public void Reset() => Value = DefaultValue;

    public Setting(string title, dynamic value, dynamic type, dynamic defaultValue) {
        Title = title;
        Value = value;
        Type = type;
        DefaultValue = defaultValue;
    }
}

// User accessible settings that the player can change
[CreateAssetMenu(fileName = "TSVR/AppSettings", menuName = "App Settings (Singleton)")]
public class AppSettings : SingletonScriptableObject<AppSettings>
{
    public float wandDistIncrement = 10f;
    public float wandSizeIncrement = 5f;
    public float wandMaxDist = 30f;
    public float wandMinDist = 0.5f;
    public float wandMaxRadius = 5f;
    public float wandMinRadius = 0.5f;
    public int wandLineSegments = 10;
    public float wandLineElasticity = 1f;
    public float particleTolerance = 0.01f;

    public int targetFramerate = 60;
    public bool enableElasticWand = false;

    // eventually combine the public values with these settings
    public Setting WandDistIncrement { get => new Setting ("Wand: Distance Multiplier", 10f, typeof(float), 10f); }
    public Setting WandSizeIncrement { get => new Setting ("Wand: Size Multiplier", 5f, typeof(float), 5f); }
    public Setting WandMaxDist { get => new Setting ("Wand: Maximum Distance", 30f, typeof(float), 30f); }
    public Setting WandMaxRadius { get => new Setting ("Wand: Maximum Radius", 5f, typeof(float), 5f); }
    public Setting WandMinRadius { get => new Setting ("Wand: Minimum Radius", 0.5f, typeof(float), 5f); }
    public Setting WandLineSegments { get => new Setting ("Wand: Line Draw Segments", 10, typeof(int), 10); }
    public Setting WandLineElasticity { get => new Setting ("Wand: Line Elasticity", 1f, typeof(float), 1f); }
    public Setting InterfaceSounds { get => new Setting("Interface Sounds Enabled", true, typeof(bool), true); }
    public Setting interfaceSoundsGain { get => new Setting("Gain: Interface Sounds", 0.3f, typeof(float), 0.3f); }

    public Setting ParticleTolerance { get => new Setting("Particle Tolerance", 0.01f, typeof(float), 0.01f); }
}