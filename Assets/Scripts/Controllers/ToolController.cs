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
        ChangeTool(initialTool);
        controllerActions = GetComponent<ControllerActions>();
        controllerActions.AddListener(controllerActions.cycleTool, CycleTool, InputActionPhase.Started);
        // while(toolTypes[toolIndex] != initialTool) toolIndex++;
        // var toolPrefab = TsvrApplication.Config.GetToolPrefab(initialTool);
        // if (toolPrefab == null) {
        //     Debug.Log("Tool not found!");
        //     return;
        // }
        // ChangeTool(toolPrefab);
        // ChangeTool(toolTypes[toolIndex]);
    }

    void OnDisable() {
        controllerActions.RemoveListener(controllerActions.cycleTool, CycleTool, InputActionPhase.Started);
    }

    // ==================================================================

    public void CycleTool(InputAction.CallbackContext context) {        
        ChangeTool(TsvrApplication.Config.toolPrefabs[(toolIndex + 1) % TsvrApplication.Config.toolPrefabs.Count]);
    }

    /// <summary>
    /// Change the current tool held by the hand. Provide an optional callback that takes in the tool being
    /// switched to, if there is any startup behavior like opening the model multitool with an existing model
    /// </summary>
    public void ChangeTool(TsvrToolType tool, Action<TsvrTool> onToolChanged = null) {
        Debug.Log("Changing tool to " + tool.ToString() + "!");

        if (CurrentTool != null && CurrentTool.GetComponent<TsvrTool>().ToolType == tool) {
            Debug.Log("You're already using that tool!");
            return;
        };

        GameObject toolPrefab = TsvrApplication.Config.GetToolPrefab(tool);
        if (toolPrefab == null) {
            Debug.Log("Tool not found!");
            return;
        }
        if (CurrentTool != null) {
            Destroy(CurrentTool);
        }
        CurrentTool = Instantiate(toolPrefab, transform);
        toolIndex = TsvrApplication.Config.toolPrefabs.FindIndex(tool => tool.GetComponent<TsvrTool>().ToolType == CurrentTool.GetComponent<TsvrTool>().ToolType);
        onToolChanged?.Invoke(CurrentTool.GetComponent<TsvrTool>());
    }

    public void ChangeTool(GameObject toolPrefab, Action<TsvrTool> onToolChanged = null) {
        Debug.Log("Changing tool to " + toolPrefab.name + "!");
        if (CurrentTool != null && CurrentTool.GetComponent<TsvrTool>().ToolType == toolPrefab.GetComponent<TsvrTool>().ToolType) {
            Debug.Log("You're already using that tool!");
            return;
        };
        if (CurrentTool != null) {
            Destroy(CurrentTool);
        }
        CurrentTool = Instantiate(toolPrefab, transform);
        toolIndex = TsvrApplication.Config.toolPrefabs.FindIndex(tool => tool.GetComponent<TsvrTool>().ToolType == CurrentTool.GetComponent<TsvrTool>().ToolType);
        onToolChanged?.Invoke(CurrentTool.GetComponent<TsvrTool>());
    }

    void Update()
    {
        
    }
}
