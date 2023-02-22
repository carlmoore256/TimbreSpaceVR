using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System;

public struct TwistLockOptions {
    public float minAngle;
    public float maxAngle;
    public float minValue;
    public float maxValue;
    public float initValue;
    public float incrementAmount;
    public bool reverse;

    public TwistLockOptions(
        float minAngle, float maxAngle, 
        float minValue, float maxValue, 
        float initValue,
        float incrementAmount,
        bool reverse = false
    ) {
        this.minAngle = minAngle;
        this.maxAngle = maxAngle;
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.initValue = initValue;
        this.incrementAmount = incrementAmount;
        this.reverse = reverse;
    }
}

public class TwistLockAction {
    public float currentValue;
    float lockedValue;
    Quaternion initialRotation;

    // callbacks are subscribed & unsubscribed to functions in this class
    InputAction toggleAction;
    InputAction rotationAction;
    InputAction.CallbackContext rotationCallback;
    ActionBasedController controller;
    ControllerActions controllerActions;
    // UnityEvent<float> listener;
    Action<float> callback;
    float modulationValue = 0f;
    bool isActive = false;
    bool reverse = false;

    // InputAction.CallbackContext scalarCallback; <- implement this later

    private TwistLockOptions options;

    public TwistLockAction(
        TwistLockOptions options,
        InputAction toggleAction,
        InputAction rotationAction,
        Action<float> callback
    ) {
        this.options = options;
        this.callback = callback;
        this.currentValue = options.initValue;
        this.toggleAction = toggleAction;
        this.rotationAction = rotationAction;
        toggleAction.started += Started;
        toggleAction.canceled += Canceled;
        toggleAction.performed += Modulate;
        rotationAction.performed += ListenForRotation;
    }

    void ListenForRotation(InputAction.CallbackContext ctx) {
        // if (callback == null) return;
        if (isActive) {
            float currentAngle = AngleFrom(ctx.ReadValue<Quaternion>());
            currentValue = lockedValue + (currentAngle * options.incrementAmount);
            currentValue = Mathf.Clamp(currentValue, options.minValue, options.maxValue);
            callback.Invoke(currentValue);
        }
    }

    public void Started(InputAction.CallbackContext ctx)
    {
        initialRotation = rotationAction.ReadValue<Quaternion>();
        isActive = true;
    }

    public void Canceled(InputAction.CallbackContext ctx)
    {
        // Debug.Log("TwistLockAction canceled!");
        lockedValue = currentValue; // store the locked value 
        isActive = false;
    }

    // TODO: Implement this if we want to apply extra pressure to the button to modify an action
    public void Modulate(InputAction.CallbackContext ctx)
    {
        // Debug.Log("Modulate action called!");
        if (ctx.ReadValue<float>() < 0.01f) {
            // Debug.Log("TwistLockAction value too low, deactivating");
            Canceled(ctx);
        } else {
            modulationValue = ctx.ReadValue<float>();
            // Debug.Log("TwistLockAction value: " + modulationValue);
        }
    }

    public float AngleFrom(Quaternion rotation) {
        Quaternion deltaRotation = rotation * Quaternion.Inverse(initialRotation);
        float angle = deltaRotation.eulerAngles.z;
        if (angle > 180) {
            angle -= 360;
        }
        angle = Mathf.Clamp(angle, options.minAngle, options.maxAngle) / 180f;
        if (reverse) angle = -angle;
        return angle;
    }

    public void UnsubscribeActions() {
        this.toggleAction.started -= Started;
        this.toggleAction.canceled -= Canceled;
        this.toggleAction.performed -= Modulate;
        // if (listener != null) 
        rotationAction.performed -= ListenForRotation;
    }

    // functions for haptics and maybe sound
}

// public TwistLockAction( 
//     float minAngle, float maxAngle, 
//     float minValue, float maxValue, 
//     float initValue,
//     float incrementAmount,
//     InputAction toggleAction,
//     InputAction rotationAction,
//     Action<float> callback,
//     bool reverse = false)
// {
//     this.initialRotation = Quaternion.identity;
//     this.options = new TwistLockOptions();
//     this.options.minAngle = minAngle;
//     this.options.maxAngle = maxAngle;
//     this.options.minValue = minValue;
//     this.options.maxValue = maxValue;
//     this.options.incrementAmount = incrementAmount;
    
//     this.currentValue = initValue;
//     this.lockedValue = initValue;
//     this.reverse = reverse;

//     this.toggleAction = toggleAction;
//     this.rotationAction = rotationAction;
//     this.callback = callback;
    
//     toggleAction.started += Started;
//     toggleAction.canceled += Canceled;
//     toggleAction.performed += Modulate;

//     // listener = new UnityEvent<float>();
//     // listener.AddListener(callback);

//     rotationAction.performed += ListenForRotation;
// }