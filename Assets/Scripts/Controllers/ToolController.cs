using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using System;

public enum ControllerHand {
    Left,
    Right
}

// ============================================================
// Controller class for handling switching between tools
// and changing hand properties. Can also be used for multitool
// functionality. Is a monobehavior and is attached to the 
// XR Rig hand.
// ============================================================
public class ToolController : MonoBehaviour
{
    public GameObject CurrentTool { get; protected set;  }
    public ControllerActions controllerActions;

    // easy way to enumerate all tool types
    private TsvrToolType[] toolTypes = (TsvrToolType[])Enum.GetValues(typeof(TsvrToolType));
    public int toolIndex = 0;

    void Start() {
        // controllerActions = GetComponent<ControllerActions>();
        // ChangeTool(toolTypes[toolIndex]);
    }
    
    // ==================================================================
    // maybe eventually change this so that it asks controller actions to do this
    void OnEnable() {
        controllerActions = GetComponent<ControllerActions>();
        ChangeTool(toolTypes[toolIndex]);
        controllerActions.cycleTool.action.started += CycleTool;
    }

    void OnDisable() {
        controllerActions.cycleTool.action.started -= CycleTool;
    }

    // ==================================================================

    void CycleTool(InputAction.CallbackContext context) {
        toolIndex++;
        while (TsvrApplication.Config.GetToolPrefab(toolTypes[toolIndex % toolTypes.Length]) == null) {
            toolIndex++;
        }
        ChangeTool(toolTypes[toolIndex % toolTypes.Length]);
    }

    void ChangeTool(TsvrToolType tool) {
        Debug.Log("Changing tool to " + tool.ToString() + "!");
        if (CurrentTool != null && CurrentTool.GetComponent<TsvrTool>().ToolType == tool) {
            Debug.Log("You're already using that tool!");
            return;
        };

        if (TsvrApplication.Config.GetToolPrefab(tool) == null) {
            Debug.Log("Tool not found!");
            return;
        }
        if (CurrentTool != null)
            Destroy(CurrentTool);
        CurrentTool = Instantiate(TsvrApplication.Config.GetToolPrefab(tool), transform);
        /*
        switch(tool) {
            case TsvrToolType.PlayWand:
                Destroy(CurrentTool);
                CurrentTool = Instantiate(TsvrApplication.Config.wandPlay, transform);
                // CurrentTool = Instantiate(Resources.Load("Prefabs/Tools/PlayWand"), transform) as GameObject;
                // CurrentTool = new Wand(xrInputActions);
                break;
            case TsvrToolType.DeleteWand:
                Destroy(CurrentTool);
                CurrentTool = Instantiate(TsvrApplication.Config.wandDelete, transform);
                break;
            case TsvrToolType.Menu:
                Destroy(CurrentTool);
                CurrentTool = Instantiate(TsvrApplication.Config.menuTool, transform);
                break;
            case TsvrToolType.Teleport:
                Debug.Log("Tool not implemented yet!");
                break;
            case TsvrToolType.Grab:
                Debug.Log("Tool not implemented yet!");
                break;
            default:
                Debug.Log("Tool not found!");
                break;
        }*/
    }

    void Update()
    {
        
    }
}
