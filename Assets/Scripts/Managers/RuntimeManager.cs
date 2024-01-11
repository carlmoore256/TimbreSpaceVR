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



    public GameObject XRPlayer;
    public GameObject NonXRPlayer;

    public bool useOrthographicCamera = false;


    public Color skyboxColorTop = Color.white;
    public Color skyboxColorMiddle = Color.white;
    public Color skyboxColorBottom = Color.white;

    public Material skyboxMaterial;

    private void Start() {
        // TsvrApplication.Config.skyboxMaterial.SetColor("_TopColor", skyboxColorTop);
        // TsvrApplication.Config.skyboxMaterial.SetColor("_TopColor", skyboxColorMiddle);
        // TsvrApplication.Config.skyboxMaterial.SetColor("_BottomColor", skyboxColorBottom);
        // skyboxMaterial.SetColor("_Tint", skyboxColorBottom);
        Material newSkyboxMaterial = new Material(TsvrApplication.Config.skyboxMaterial);
        newSkyboxMaterial.SetColor("_TopColor", skyboxColorTop);
        newSkyboxMaterial.SetColor("_MiddleColor", skyboxColorMiddle);
        newSkyboxMaterial.SetColor("_BottomColor", skyboxColorBottom);
        RenderSettings.skybox = newSkyboxMaterial;
        DynamicGI.UpdateEnvironment();
        ConfigureXR();
    }

    void ConfigureXR()
    {
        // Check if XR is enabled
        if (XRSettings.enabled && XRSettings.isDeviceActive)
        {
            // If enabled, activate XR player and deactivate non-XR player
            XRPlayer.SetActive(true);
            NonXRPlayer.SetActive(false);
        }
        else
        {
            // If not enabled, activate non-XR player and deactivate XR player
            NonXRPlayer.SetActive(true);
            NonXRPlayer.GetComponent<Camera>().orthographic = useOrthographicCamera;

            XRPlayer.SetActive(false);
        }
    }

    public static LineRenderer SpawnWorldSpaceLine() {
        GameObject lineObject = Instantiate(TsvrApplication.Config.worldSpaceLinePrefab);
        LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
        return lineRenderer;
    }
}