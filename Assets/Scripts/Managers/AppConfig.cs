using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Unity accessible configuration for the application
[CreateAssetMenu(fileName = "TSVR/AppConfig", menuName = "App Config (Singleton)")]
public class AppConfig : SingletonScriptableObject<AudioManager>
{
    public GameObject wandPlay;
    public GameObject wandDelete;
    public GameObject wandConstellation;
    public GameObject menuTool;
    public GameObject grainSpawner;


    public GameObject grainModel;
    public GameObject grainPrefab;
    // public Dictionary<TsvrToolType, GameObject> tools = new Dictionary<TsvrToolType, GameObject> {
    //     { TsvrToolType.PlayWand, playWandTool },
    //     { TsvrToolType.DeleteWand, deleteWandTool },
    //     { TsvrToolType.Menu, menuTool }
    // };

    public GameObject GetToolPrefab(TsvrToolType toolType) {
        switch (toolType) {
            case TsvrToolType.PlayWand:
                return wandPlay;
            case TsvrToolType.DeleteWand:
                return wandDelete;
            case TsvrToolType.ConstellationWand:
                return wandConstellation;
            case TsvrToolType.Menu:
                return menuTool;
            case TsvrToolType.GrainSpawner:
                return grainSpawner;
            default:
                return null;
        }
    }
}
