using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Editor accessible configuration for the application
[CreateAssetMenu(fileName = "TSVR/AppConfig", menuName = "App Config (Singleton)")]
public class AppConfig : SingletonScriptableObject<AudioManager>
{
    public List<GameObject> toolPrefabs;

    [Header("Models")]
    public GameObject grainModel;
    public GameObject grainPrefab;
    public GameObject flexibleLinePrefab;
    public GameObject worldSpaceLinePrefab;
    public GameObject xrDebugConsolePrefab;

    [Header("Materials")]
    public Material skyboxMaterial;
    
    public GameObject GetToolPrefab(TsvrToolType toolType) {
        return toolPrefabs.FirstOrDefault(x => x.GetComponent<TsvrTool>().ToolType == toolType);

    }
}