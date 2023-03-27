using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// Handles 1D float input actions (like trigger and hand grip)
/// </summary>
public class FloatInputActionHandler : IInputActionHandler {
    public enum ActionType { Press, Release, Toggle, Value }
    public MonoBehaviour Owner { get; set; }
    InputAction buttonAction;
    InputAction valueAction;
    Dictionary<ActionType, ActionListeners<float>> observers;
    Coroutine valueListener;
    bool isPressed = false;

    public FloatInputActionHandler(InputActionReference _button, InputActionReference _value, MonoBehaviour _owner) {
        Owner = _owner;
        buttonAction = _button.action;
        valueAction = _value.action;
        Enable();
    }

    public FloatInputActionHandler(InputActionValuePair _pair, MonoBehaviour _owner) {
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