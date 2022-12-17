using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.UI;

public abstract class TsvrTool : MonoBehaviour {
    public TsvrToolType ToolType { get; protected set; }
    public ControllerHand Hand { get; protected set; }
    ActionBasedController Controller { get; } // maybe remove this
    GameObject Prefab { get; }

    public virtual void Destroy() {
        // UnsubscribeActions();
        // Controller.modelPrefab = null;
    }
}
