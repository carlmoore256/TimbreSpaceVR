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
    public InputActionReference toolOptionButton;
    public InputActionReference twistLock;
    public InputActionReference twistLockCombo;
    public InputActionReference cycleTool;
    public InputActionReference toolAxis2D;
    public InputActionReference rotationAction;

    public ControllerHand Hand { get; protected set; }

    public InputActionValuePair TriggerValue => new InputActionValuePair { button = trigger, value = triggerValue };
    public InputActionValuePair GrabValue => new InputActionValuePair { button = grab, value = grabValue };

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

    public void JoystickDirectionAction(InputAction.CallbackContext ctx) {
        Debug.Log("JoystickDirectionAction: " + ctx.action.GetBindingDisplayString() + "Action Performed!");
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


public interface IInputActionHandler {
    public MonoBehaviour Owner { get; set; }
    void Enable();
    void Disable();
    void RemoveAllObservers();
}

/// <summary>
/// Handles 1D float input actions (like trigger and hand grip)
/// </summary>
public class InputActionValueHandler : IInputActionHandler {
    public enum ActionType { Press, Release, Toggle, Value }
    public MonoBehaviour Owner { get; set; }
    InputAction buttonAction;
    InputAction valueAction;
    Dictionary<ActionType, ActionListeners<float>> observers;
    Coroutine valueListener;
    bool isPressed = false;

    public InputActionValueHandler(InputActionReference _button, InputActionReference _value, MonoBehaviour _owner) {
        Owner = _owner;
        buttonAction = _button.action;
        valueAction = _value.action;
        Enable();
    }

    public InputActionValueHandler(InputActionValuePair _pair, MonoBehaviour _owner) {
        Owner = _owner;
        buttonAction = _pair.button.action;
        valueAction = _pair.value.action;
        observers = new Dictionary<ActionType, ActionListeners<float>>() {
            { ActionType.Press, new ActionListeners<float>() },
            { ActionType.Release, new ActionListeners<float>() },
            { ActionType.Toggle, new ActionListeners<float>() },
            { ActionType.Value, new ActionListeners<float>() }
        };
        Enable();
    }

    public void Enable() {
        buttonAction.started += OnPress;
        buttonAction.canceled += OnRelease;
    }

    public void Disable() {
        buttonAction.started -= OnPress;
        buttonAction.canceled -= OnRelease;
        if (valueListener != null) Owner.StopCoroutine(valueListener);
        RemoveAllObservers();
    }

    private void OnPress(InputAction.CallbackContext context) {
        // if (observers.ContainsKey(ActionType.Press)) {
        observers[ActionType.Press].NotifyListeners(1);
        // Debug.Log("OnPress: " + observers[ActionType.Press].ToString());
        OnToggle(1);
        isPressed = true;
        if(valueListener != null) Owner.StopCoroutine(valueListener);
        valueListener = Owner.StartCoroutine(OnValue());
        // }
    }

    private void OnRelease(InputAction.CallbackContext context) {
        // if (observers.ContainsKey(ActionType.Release)) {
        observers[ActionType.Release].NotifyListeners(0);
        // Debug.Log("[-] OnRelease: " + observers[ActionType.Release].ToString());
        OnToggle(0);
        isPressed = false;
        // }
    }

    private void OnToggle(float value) {
        // if (observers.ContainsKey(ActionType.Toggle)) {
        observers[ActionType.Toggle].NotifyListeners(0);
        // }
    }

    private IEnumerator OnValue() {
        while (isPressed) {
            // if (observers.ContainsKey(ActionType.Value)) {
            var value = valueAction.ReadValue<float>();
            observers[ActionType.Value].NotifyListeners(value);
            // }
            yield return null;
        }
    }

    # region Public Methods

    public void AddObserver(ActionType actionType, Action<float> callback) {
        if (!observers.ContainsKey(actionType)) {
            observers.Add(actionType, new ActionListeners<float>());
        }
        observers[actionType].AddListener(callback);
    }

    public void RemoveObserver(ActionType actionType) {
        if (observers.ContainsKey(actionType)) {
            observers[actionType].RemoveAll();
        }
    }

    public void RemoveObserver(Action<float> callback) {
        foreach (var o in observers)
            o.Value.RemoveListener(callback);
    }

    public void RemoveObserver(ActionType actionType, Action<float> callback) {
        if (observers.ContainsKey(actionType)) {
            observers[actionType].RemoveListener(callback);
        }
    }

    public void RemoveAllObservers() {
        foreach(var o in observers)
            o.Value.RemoveAll();
    }

    # endregion
}