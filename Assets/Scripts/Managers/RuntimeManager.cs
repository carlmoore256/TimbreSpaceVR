using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


public class RuntimeManager : MonoBehaviour
{
    // monobehaviour scripts can reference this to get things like settings
    [SerializeField]
    public ScriptableObject masterManager;
    public ToolController LeftHandTool { get; set; }
    public ToolController RightHandTool { get; set; }


    public Color skyboxColorTop = Color.white;
    public Color skyboxColorMiddle = Color.white;
    public Color skyboxColorBottom = Color.white;

    private void Start() {
        TsvrApplication.Config.skyboxMaterial.SetColor("_TopColor", skyboxColorTop);
        TsvrApplication.Config.skyboxMaterial.SetColor("_TopColor", skyboxColorMiddle);
        TsvrApplication.Config.skyboxMaterial.SetColor("_BottomColor", skyboxColorBottom);
    }


    public static LineRenderer SpawnWorldSpaceLine() {
        GameObject lineObject = Instantiate(TsvrApplication.Config.worldSpaceLinePrefab);
        LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
        return lineRenderer;
    }
}