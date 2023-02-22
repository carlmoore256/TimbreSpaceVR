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
    
    [HideInInspector]
    public ControllerActions controllerActions;
    public TsvrToolType initialTool = TsvrToolType.Menu;

    // easy way to enumerate all tool types
    private TsvrToolType[] toolTypes = (TsvrToolType[])Enum.GetValues(typeof(TsvrToolType));
    private int toolIndex = 0;
    
    // ==================================================================
    // maybe eventually change this so that it asks controller actions to do this
    void OnEnable() {
        controllerActions = GetComponent<ControllerActions>();
        while(toolTypes[toolIndex] != initialTool) toolIndex++;
        ChangeTool(toolTypes[toolIndex]);
        controllerActions.cycleTool.action.started += CycleTool;
    }

    void OnDisable() {
        controllerActions.cycleTool.action.started -= CycleTool;
    }

    // ==================================================================

    public void CycleTool(InputAction.CallbackContext context) {
        toolIndex++;
        while (TsvrApplication.Config.GetToolPrefab(toolTypes[toolIndex % toolTypes.Length]) == null) {
            toolIndex++;
        }
        ChangeTool(toolTypes[toolIndex % toolTypes.Length]);
    }

    public void ChangeTool(TsvrToolType tool) {
        Debug.Log("Changing tool to " + tool.ToString() + "!");
        if (CurrentTool != null && CurrentTool.GetComponent<TsvrTool>().ToolType == tool) {
            Debug.Log("You're already using that tool!");
            return;
        };

        // maybe cache this in a dict within this controller at runtime
        GameObject toolPrefab = TsvrApplication.Config.GetToolPrefab(tool);
        if (toolPrefab == null) {
            Debug.Log("Tool not found!");
            return;
        }
        if (toolPrefab.GetComponent<TsvrTool>() == null) {
            Debug.Log("Tool prefab does not have a TsvrTool component!");
            return;
        }
        if (CurrentTool != null)
            Destroy(CurrentTool);
            CurrentTool = Instantiate(toolPrefab, transform);
    }

    void Update()
    {
        
    }
}
