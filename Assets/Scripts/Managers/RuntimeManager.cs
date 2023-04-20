using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;

public class RuntimeManager : MonoBehaviour
{
    // monobehaviour scripts can reference this to get things like settings
    [SerializeField]
    public ScriptableObject masterManager;
    public ToolController LeftHandTool { get; set; }
    public ToolController RightHandTool { get; set; }



    public GameObject xrPlayer;
    public GameObject nonXRPlayer;


    public Color skyboxColorTop = Color.white;
    public Color skyboxColorMiddle = Color.white;
    public Color skyboxColorBottom = Color.white;

    private void Start() {
        TsvrApplication.Config.skyboxMaterial.SetColor("_TopColor", skyboxColorTop);
        TsvrApplication.Config.skyboxMaterial.SetColor("_TopColor", skyboxColorMiddle);
        TsvrApplication.Config.skyboxMaterial.SetColor("_BottomColor", skyboxColorBottom);

        ConfigureXR();
    }

    void ConfigureXR()
    {
        // Check if XR is enabled
        if (XRSettings.enabled && XRSettings.isDeviceActive)
        {
            // If enabled, activate XR player and deactivate non-XR player
            xrPlayer.SetActive(true);
            nonXRPlayer.SetActive(false);
        }
        else
        {
            // If not enabled, activate non-XR player and deactivate XR player
            nonXRPlayer.SetActive(true);
            xrPlayer.SetActive(false);
        }
    }

    public static LineRenderer SpawnWorldSpaceLine() {
        GameObject lineObject = Instantiate(TsvrApplication.Config.worldSpaceLinePrefab);
        LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
        return lineRenderer;
    }
}