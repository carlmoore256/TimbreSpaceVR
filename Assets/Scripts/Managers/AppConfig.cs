using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
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
    public GameObject samplePackBrowser;
    public GameObject modelMultitool;

    public List<GameObject> toolPrefabs;

    [Header("Models")]
    public GameObject grainModel;
    public GameObject grainPrefab;
    public GameObject flexibleLinePrefab;
    public GameObject worldSpaceLinePrefab;
    public GameObject xrDebugConsolePrefab;

    
    public GameObject GetToolPrefab(TsvrToolType toolType) {
        foreach(GameObject toolPrefab in toolPrefabs) {
            Debug.Log("toolPrefab.GetComponent<TsvrTool>().ToolType: " + toolPrefab.GetComponent<TsvrTool>().ToolType);
            if (toolPrefab.GetComponent<TsvrTool>().ToolType == toolType) {
                return toolPrefab;
            }
        }
        return toolPrefabs.FirstOrDefault(x => x.GetComponent<TsvrTool>().ToolType == toolType);

    }
}

// switch (toolType) {
//     case TsvrToolType.PlayWand:
//         return wandPlay;
//     case TsvrToolType.EditWand:
//         return wandEdit;
//     case TsvrToolType.ConstellationWand:
//         return wandConstellation;
//     case TsvrToolType.Menu:
//         return menuTool;
//     case TsvrToolType.GrainSpawner:
//         return grainSpawner;
//     case TsvrToolType.Locomotion:
//         return locomotionTool;
//     case TsvrToolType.SamplePackBrowser:
//         return samplePackBrowser;
//     case TsvrToolType.ModelMultitool:
//         return modelMultitool;
//     default:
//         return null;
// }