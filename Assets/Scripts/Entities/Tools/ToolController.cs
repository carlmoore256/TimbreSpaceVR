using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

// ============================================================
// Controller class for handling switching between tools
// and changing hand properties. Can also be used for multitool
// functionality. Is a monobehavior and is attached to the 
// XR Rig hand.
// ============================================================
public class ToolController : MonoBehaviour
{
    public TsvrTool CurrentTool { get; protected set;  }
    private ActionBasedController xrInputActions;

    void Start() {
        xrInputActions = GetComponent<ActionBasedController>();
    }

    void ChangeTool(TsvrToolType tool) {
        if (CurrentTool.ToolType == tool) {
            Debug.Log("You're already using that tool!");
            return;
        };
        CurrentTool?.Destroy();
        // Destroy(currentTool);
        switch(tool) {
            case TsvrToolType.PlayWand:
                // CurrentTool = new Wand(xrInputActions);
                break;
            case TsvrToolType.Menu:
                // CurrentTool = new Menu(xrInputActions);
                break;
            case TsvrToolType.Teleport:
                break;
            case TsvrToolType.Grab:
                break;
            default:
                Debug.Log("Tool not found!");
                break;
        }
    }

    void Update()
    {
        
    }
}
