using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class TwistLockAction : MonoBehaviour {
    float minAngle;
    float maxAngle;

    float minValue;
    float maxValue;
    float lockedValue;
    float currentValue;
    float incrementAmount = 0.1f;
    Quaternion initialRotation;

    // callbacks are subscribed & unsubscribed to functions in this class
    InputAction toggleAction;
    InputAction rotationAction;
    InputAction.CallbackContext rotationCallback;
    ActionBasedController controller;
    ControllerActions controllerActions;
    UnityEvent<float> listener;
    float modulationValue = 0f;
    bool active = false;
    bool reverse = false;

    // InputAction.CallbackContext scalarCallback; <- implement this later

    public TwistLockAction( 
        float minAngle, float maxAngle, 
        float minValue, float maxValue, 
        float initValue,
        float incrementAmount,
        InputAction toggleAction,
        InputAction rotationAction,
        UnityAction<float> callback = null,
        bool reverse = false)
    {
        this.initialRotation = Quaternion.identity;
        this.minAngle = minAngle;
        this.maxAngle = maxAngle;
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.incrementAmount = incrementAmount;
        this.toggleAction = toggleAction;
        this.rotationAction = rotationAction;
        this.currentValue = initValue;
        this.lockedValue = initValue;
        this.reverse = reverse;

        toggleAction.started += Started;
        toggleAction.canceled += Canceled;
        toggleAction.performed += Modulate;

        listener = new UnityEvent<float>();
        listener.AddListener(callback);

        if (listener != null) rotationAction.performed += ListenForRotation;
    }

    void ListenForRotation(InputAction.CallbackContext ctx) {
        if (listener == null) return;
        if (active) {
            float currentAngle = AngleFrom(ctx.ReadValue<Quaternion>());
            currentValue = lockedValue + (currentAngle * incrementAmount);
            currentValue = Mathf.Clamp(currentValue, minValue, maxValue);
            listener.Invoke(currentValue);
        }
    }

    public void Started(InputAction.CallbackContext ctx)
    {
        Debug.Log("TwistLockAction started!");
        initialRotation = rotationAction.ReadValue<Quaternion>();
        active = true;
    }

    public void Canceled(InputAction.CallbackContext ctx)
    {
        Debug.Log("TwistLockAction canceled!");
        lockedValue = currentValue; // store the locked value 
        active = false;
    }

    // TODO: Implement this if we want to apply extra pressure to the button to modify an action
    public void Modulate(InputAction.CallbackContext ctx)
    {
        if (ctx.ReadValue<float>() < 0.01f) {
            Debug.Log("TwistLockAction value too low, deactivating");
            Canceled(ctx);
        } else {
            modulationValue = ctx.ReadValue<float>();
            Debug.Log("TwistLockAction value: " + modulationValue);
        }
    }

    public float AngleFrom(Quaternion rotation) {
        Quaternion deltaRotation = rotation * Quaternion.Inverse(initialRotation);
        float angle = deltaRotation.eulerAngles.z;
        if (angle > 180) {
            angle -= 360;
        }
        angle = Mathf.Clamp(angle, minAngle, maxAngle) / 180f;
        if (reverse) angle = -angle;
        return angle;
    }

    public void UnsubscribeActions() {
        this.toggleAction.started -= Started;
        this.toggleAction.canceled -= Canceled;
        this.toggleAction.performed -= Modulate;
        if (listener != null) rotationAction.performed -= ListenForRotation;
    }

    // functions for haptics and maybe sound
}