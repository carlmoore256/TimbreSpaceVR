using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

// possible control schemes:
// - Controller actions (this script) handles all control events, and does not pass
// down any input action references. It has complete control over the logic
// - Controller actions provides other scripts with all the input action references
// and lets them handle subscribing and unsubscribing on their own
// - Controller actions is a static class that everything below it references,
// but lower classes need to ask controller actions to do subscribing (pass in the delegate)


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
    public ControllerHand Hand { get; protected set; }

    
    private void OnEnable() {
        if (transform.name.Contains("Left")) {
            Hand = ControllerHand.Left;
        } else if(transform.name.Contains("Right")) {
            Hand = ControllerHand.Right;
        }
        // twistLockAction.action.Enable();
        // twistLockModifiedAction.action.Enable();

        trigger.action.Enable();
        triggerValue.action.Enable();
    }

    private void OnDisable() {
        
        trigger.action.Disable();
        triggerValue.action.Disable();
        // twistLockAction.action.Disable();
        // twistLockModifiedAction.action.Disable();
    }

    private void Start() {

        // make a null condition

        // twistLockModifiedAction.action.started += ActionStarted;
        // twistLockModifiedAction.action.performed += ActionPerformed;
        // twistLockModifiedAction.action.canceled += ActionCanceled;
        // twistLock.action.started += ActionStarted;
        // twistLock.action.performed += ActionPerformed;
        // twistLock.action.canceled += ActionCanceled;

        // trigger.action.started += ActionStarted;
        // trigger.action.performed += ActionPerformed;
        // trigger.action.canceled += ActionCanceled;
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