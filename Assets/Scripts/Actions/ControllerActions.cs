using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public enum ControllerHand {
    Left,
    Right
}

// ============================================================
// Wrapper for controller input actions per-hand
// ============================================================
public class ControllerActions : MonoBehaviour
{
    public InputActionReference grab;
    public InputActionReference grabValue;
    public InputActionReference trigger;
    public InputActionReference triggerValue;
    public InputActionReference uiSelect;
    public InputActionReference toolOptionButton;
    public InputActionReference twistLock;
    public InputActionReference twistLockCombo;
    public InputActionReference cycleTool;
    public InputActionReference toolAxis2D;
    public InputActionReference rotationAction;
    public ControllerHand hand;
    
    private void OnEnable() {
        // twistLockAction.action.Enable();
        // twistLockModifiedAction.action.Enable();
    }

    private void OnDisable() {
        // twistLockAction.action.Disable();
        // twistLockModifiedAction.action.Disable();
    }

    private void Start() {
        if (transform.parent.name.Contains("LeftHand")) {
            hand = ControllerHand.Left;
        } else {
            hand = ControllerHand.Right;
        }
        // hand = transform.parent.name;
        // twistLockModifiedAction.action.started += ActionStarted;
        // twistLockModifiedAction.action.performed += ActionPerformed;
        // twistLockModifiedAction.action.canceled += ActionCanceled;

        // twistLock.action.started += ActionStarted;
        // twistLock.action.performed += ActionPerformed;
        // twistLock.action.canceled += ActionCanceled;
    }


    void ActionStarted(InputAction.CallbackContext ctx) {
        Debug.Log("=> " + ctx.action.GetBindingDisplayString() + "Action Started!");
    }

    void ActionPerformed(InputAction.CallbackContext ctx) {
        Debug.Log("=> " + ctx.action.GetBindingDisplayString() + "Action Performed!");
    }

    void ActionCanceled(InputAction.CallbackContext ctx) {
        Debug.Log("=> " + ctx.action.GetBindingDisplayString() + " Action Canceled!");
    }

    // public ControllerActions(ActionBasedController xrInputActions) {
    //     this.xrInputActions = xrInputActions;
    //     primaryButton = xrInputActions.selectAction.action;
    //     secondaryButton = xrInputActions.activateAction.action;
    //     gripAction = xrInputActions.activateAction.action;
    // }
    
}