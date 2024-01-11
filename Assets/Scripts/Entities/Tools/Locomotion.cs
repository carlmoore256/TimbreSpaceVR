using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Allows player to move around with joystick, and to teleport with the trigger
// press the main action button to activate free flying mode
public class Locomotion : TsvrTool
{
    public override TsvrToolType ToolType { get => TsvrToolType.Locomotion; }

    public Transform moveableJoystick;
    public Transform antennaTip;
    private Transform cameraRig;

    private Color tipOriginalColor = Color.red;
    private Color tipActivatedColor = Color.green;
    private Coroutine moveTimeout;
    private Quaternion joystickOriginalRotation;
    private Quaternion joystickTargetRotation;

    void OnEnable() {
        ControllerActions.toolAxis2D.action.performed += JoystickMove;
        cameraRig = GameObject.Find("TSVR_CameraRig").transform;
        joystickOriginalRotation = moveableJoystick.localRotation;
    }

    void OnDisable() {
        ControllerActions.toolAxis2D.action.performed -= JoystickMove;
    }

    void JoystickMove(InputAction.CallbackContext ctx) {
        antennaTip.GetComponent<Renderer>().material.color = tipActivatedColor;
        Vector2 input = ctx.ReadValue<Vector2>();
        Vector3 targetDir = moveableJoystick.transform.localPosition + new Vector3(-input.x, -input.y, 2);
        joystickTargetRotation = Quaternion.LookRotation(targetDir);
        cameraRig.Translate(new Vector3(input.x, 0, input.y) * Time.deltaTime * 2);
        if (moveTimeout != null)
            StopCoroutine(moveTimeout);
        moveTimeout = StartCoroutine(MoveTimeout());
    }

    IEnumerator MoveTimeout() {
        yield return new WaitForSeconds(0.5f);
        antennaTip.GetComponent<Renderer>().material.color = tipOriginalColor;
        joystickTargetRotation = joystickOriginalRotation;
    }

    void Update() {
        if (!Quaternion.Equals(moveableJoystick.rotation, joystickTargetRotation)) {
            moveableJoystick.localRotation = Quaternion.Slerp(moveableJoystick.localRotation, joystickTargetRotation, Time.deltaTime * 10f);
            if (Quaternion.Angle(moveableJoystick.rotation, joystickTargetRotation) < 0.1f) 
                moveableJoystick.localRotation = joystickTargetRotation;
        }
    }

}
