using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

// possible control schemes:
// - Controller actions (this script) handles all control events, and does not pass
// down any input action references. It has complete control over the logic
// - Controller actions provides other scripts with all the input action references
// and lets them handle subscribing and unsubscribing on their own
// - Controller actions is a static class that everything below it references,
// but lower classes need to ask controller actions to do subscribing (pass in the delegate)


/// <summary>
/// Pair of InputActionReference that allows for a handler to bind to button and value events
/// </summary>
public struct InputActionValuePair {
    public InputActionReference button;
    public InputActionReference value;
}

/// <summary>
// Access to controller input actions, per-hand
/// </summary>
public class ControllerActions : MonoBehaviour
{
    public InputActionReference grab;
    public InputActionReference grabValue;
    public InputActionReference trigger;
    public InputActionReference triggerValue;
    public InputActionReference uiSelect;
    public InputActionReference toolOption;
    public InputActionReference toolCycle;
    public InputActionReference twistLock;
    public InputActionReference twistLockCombo;
    public InputActionReference cycleTool;
    public InputActionReference toolAxis2D;
    public InputActionReference rotationAction;

    public ControllerHand Hand { get; protected set; }

    public InputActionValuePair TriggerValue => new InputActionValuePair { button = trigger, value = triggerValue };
    public InputActionValuePair GrabValue => new InputActionValuePair { button = grab, value = grabValue };

    // private Dictionary<Action<InputAction.CallbackContext>, (string id, Action<InputAction.CallbackContext> cb)> boundActions = new Dictionary<Action<InputAction.CallbackContext>, (string, Action<InputAction.CallbackContext>)>();

    private void OnEnable() {
        if (transform.name.Contains("Left") || transform.name.Contains("left")) {
            Hand = ControllerHand.Left;
        } else if(transform.name.Contains("Right") || transform.name.Contains("right")) {
            Hand = ControllerHand.Right;
        }

        grab.action.Enable();
        grabValue.action.Enable();
        trigger.action.Enable();
        triggerValue.action.Enable();
    }

    private void OnDisable() {
        grab.action.Disable();
        grabValue.action.Disable();
        trigger.action.Disable();
        triggerValue.action.Disable();
    }

    public void AddListener(InputActionReference inputAction, Action<InputAction.CallbackContext> callback)
    {
        inputAction.action.started += callback;
        inputAction.action.performed += callback;
        inputAction.action.canceled += callback;
    }

    public void AddListener(InputActionReference inputAction, Action<InputAction.CallbackContext> callback, InputActionPhase phase)
    {
        switch (phase) {
            case InputActionPhase.Started:
                inputAction.action.started += callback;
                break;
            case InputActionPhase.Performed:
                inputAction.action.performed += callback;
                break;
            case InputActionPhase.Canceled:
                inputAction.action.canceled += callback;
                break;
        }
    }

    public void RemoveListener(InputActionReference inputAction, Action<InputAction.CallbackContext> callback)
    {
        inputAction.action.started -= callback;
        inputAction.action.performed -= callback;
        inputAction.action.canceled -= callback;
    }

    public void RemoveListener(InputActionReference inputAction, Action<InputAction.CallbackContext> callback, InputActionPhase phase)
    {
        switch (phase) {
            case InputActionPhase.Started:
                inputAction.action.started -= callback;
                break;
            case InputActionPhase.Performed:
                inputAction.action.performed -= callback;
                break;
            case InputActionPhase.Canceled:
                inputAction.action.canceled -= callback;
                break;
        }
    }
}

/// <summary>
/// Generic listener handler for registering observers
/// </summary>
public class ActionListeners<T> {
        private List<Action<T>> listeners;
        public ActionListeners() => listeners = new List<Action<T>>();
        public void AddListener(Action<T> _listener) => listeners.Add(_listener);
        public void RemoveListener(Action<T> _listener) => listeners.Remove(_listener);
        public void RemoveAll() => listeners.Clear();
        public void NotifyListeners(T _value) => listeners.ForEach(listener => listener?.Invoke(_value));
}
