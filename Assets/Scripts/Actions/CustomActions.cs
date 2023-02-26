using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System;

public struct TwistLockOptions {
    public float minAngle;
    public float maxAngle;
    public bool reverse;
    public TwistLockOptions(float minAngle, float maxAngle, bool reverse = false) {
        this.minAngle = minAngle;
        this.maxAngle = maxAngle;
        this.reverse = reverse;
    }
}

public class TwistLockAction {
    public float currentValue = 0f;
    // float lockedValue;
    private float initialAngle = 0f;
    Quaternion initialRotation;
    InputActionReference toggleAction;
    InputAction rotationAction;
    ControllerActions controllerActions;
    float modulationValue = 0f;
    bool isActive = false;
    bool reverse = false;
    private TwistLockOptions options;

    private Action<float> onValueChange;
    private Action<float> onLock;

    public TwistLockAction(
        TwistLockOptions options,
        ControllerActions controllerActions,
        InputActionReference toggleAction,
        Action<float> onValueChange,
        Action<float> onLock
    ) {
        this.options = options;
        this.controllerActions = controllerActions;
        this.toggleAction = toggleAction;
        this.rotationAction = controllerActions.rotationAction.action;

        this.onValueChange = onValueChange;
        this.onLock = onLock;

        controllerActions.AddListener(toggleAction, Started, InputActionPhase.Started);
        controllerActions.AddListener(toggleAction, Canceled, InputActionPhase.Canceled);
        controllerActions.AddListener(toggleAction, Modulate, InputActionPhase.Performed);
        controllerActions.AddListener(controllerActions.rotationAction, OnRotate, InputActionPhase.Performed);
    }

    private void OnRotate(InputAction.CallbackContext ctx) {
        if (isActive) {
            Quaternion rotation = ctx.ReadValue<Quaternion>();
            float angle = QuaternionZRotation(rotation);
            float delta = angle - initialAngle;
            delta /= 360f;
            currentValue = delta;
            onValueChange.Invoke(delta);
        }
    }

    public static float QuaternionZRotation(Quaternion rotation)
    {
        // Convert the quaternion to Euler angles
        Vector3 euler = rotation.eulerAngles;

        // Normalize the angle to be in the range [-180, 180] degrees
        float zRotation = euler.z;
        if (zRotation > 180f) zRotation -= 360f;
        if (zRotation < -180f) zRotation += 360f;

        return zRotation;
    }

    private void Started(InputAction.CallbackContext ctx)
    {
        Quaternion rotation = rotationAction.ReadValue<Quaternion>();
        initialRotation = rotation;
        initialAngle = QuaternionZRotation(rotation);
        isActive = true;
    }

    private void Canceled(InputAction.CallbackContext ctx) {
        isActive = false;
        onLock.Invoke(currentValue);
    }

    // TODO: Implement this if we want to apply extra pressure to the button to modify an action
    private void Modulate(InputAction.CallbackContext ctx)
    {
        if (ctx.ReadValue<float>() < 0.01f) {
            // Debug.Log("TwistLockAction value too low, deactivating");
            Canceled(ctx);
        } else {
            modulationValue = ctx.ReadValue<float>();
        }
    }

    public static float GetRotationAngle(Quaternion initialRotation, Quaternion currentRotation)
    {
        float angle = Quaternion.Angle(initialRotation, currentRotation);
        Vector3 initialForward = initialRotation * Vector3.forward;
        Vector3 currentForward = currentRotation * Vector3.forward;
        Vector3 cross = Vector3.Cross(initialForward, currentForward);
        float dot = Vector3.Dot(Vector3.up, cross);
        float sign = Mathf.Sign(dot);
        Vector3 signedAngle = Quaternion.AngleAxis(angle, Vector3.up * sign) * initialForward;
        return Vector3.SignedAngle(initialForward, signedAngle, Vector3.up);
    }

    private float AngleFrom(Quaternion _initialRotation, Quaternion _currentRotation) {
        float angle = Quaternion.Angle(_initialRotation, _currentRotation);
        float sign = Mathf.Sign(Vector3.Dot(Vector3.up, Vector3.Cross(_initialRotation * Vector3.forward, _currentRotation * Vector3.forward)));
        float normalizedAngle = angle / 180f;
        //  * sign;
        if (reverse) normalizedAngle = -normalizedAngle;
        return normalizedAngle;
    }

    public void UnsubscribeActions() {
        controllerActions.RemoveListener(toggleAction, Started, InputActionPhase.Started);
        controllerActions.RemoveListener(toggleAction, Canceled, InputActionPhase.Canceled);
        controllerActions.RemoveListener(toggleAction, Modulate, InputActionPhase.Performed);
        controllerActions.RemoveListener(controllerActions.rotationAction, OnRotate, InputActionPhase.Performed);
    }

    // functions for haptics and maybe sound
}


// float angle = deltaRotation.eulerAngles.z;
// if (angle > 180) {
//     angle -= 360;
// }
// angle = Mathf.Clamp(angle, 0, 1);


// public TwistLockAction(
//     TwistLockOptions options,
//     InputAction toggleAction,
//     InputAction rotationAction,
//     Action<float> callback
// ) {
//     this.options = options;
//     this.callback = callback;
//     // this.currentValue = options.initValue;
//     this.toggleAction = toggleAction;
//     this.rotationAction = rotationAction;
//     toggleAction.started += Started;
//     toggleAction.canceled += Canceled;
//     toggleAction.performed += Modulate;
//     rotationAction.performed += OnRotate;
// }


// void ListenForRotation(InputAction.CallbackContext ctx) {
//     if (isActive) {
//         float currentAngle = AngleFrom(ctx.ReadValue<Quaternion>());
//         currentValue = lockedValue + (currentAngle * options.incrementAmount);
//         currentValue = Mathf.Clamp(currentValue, options.minValue, options.maxValue);
//         callback.Invoke(currentValue);
//     }
// }



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