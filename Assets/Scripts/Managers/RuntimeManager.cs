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

    public static LineRenderer SpawnWorldSpaceLine() {
        GameObject lineObject = Instantiate(TsvrApplication.Config.flexibleLinePrefab);
        LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
        return lineRenderer;
    }
}