using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Unity accessible configuration for the application
[CreateAssetMenu(fileName = "TSVR/AppConfig", menuName = "App Config (Singleton)")]
public class AppConfig : SingletonScriptableObject<AudioManager>
{
    [Header("Tool Prefabs")]
    public GameObject wandPlay;
    public GameObject wandEdit;
    public GameObject wandConstellation;
    public GameObject menuTool;
    public GameObject grainSpawner;
    public GameObject locomotionTool;

    [Header("Models")]
    public GameObject grainModel;
    public GameObject grainPrefab;
    public GameObject flexibleLinePrefab;
    public GameObject xrDebugConsolePrefab;

    
    public GameObject GetToolPrefab(TsvrToolType toolType) {
        switch (toolType) {
            case TsvrToolType.PlayWand:
                return wandPlay;
            case TsvrToolType.EditWand:
                return wandEdit;
            case TsvrToolType.ConstellationWand:
                return wandConstellation;
            case TsvrToolType.Menu:
                return menuTool;
            case TsvrToolType.GrainSpawner:
                return grainSpawner;
            case TsvrToolType.Locomotion:
                return locomotionTool;
            default:
                return null;
        }
    }
}
