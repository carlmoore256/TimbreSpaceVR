using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.UI;

public abstract class TsvrTool : MonoBehaviour {
    public Animation animations;

    public TsvrToolType ToolType { get; protected set; }
    // public ControllerHand Hand { get; protected set; }
    // ActionBasedController Controller { get; } // maybe remove this
    public ToolController ToolController { get; set; }
    // GameObject Prefab { get; }
    public ControllerActions ControllerActions { get; set; }

    void Awake() {
        ToolController = transform.parent.GetComponent<ToolController>();
        ControllerActions = transform.parent.GetComponent<ControllerActions>();
    }

    public virtual void Destroy() {
        // UnsubscribeActions();
        // Controller.modelPrefab = null;
    }

    // add methods to play sound when equipped and unequipped
}