using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;
using System;
using NWaves.Signals;
using System.Threading.Tasks;
using UnityEngine;



/// <summary>
/// Container for grains in a GrainCloud, handling transforms and inspect interactions
/// </summary>
public class BoundingBoxRenderer : MonoBehaviour
{
    public GameObject boundingBox;

    # region Public Methods

    public void SetColor(Color color, float duration = 1f, float delay = 0f) {
        Color currentColor = boundingBox.GetComponent<Renderer>().material.color;
        if (Color.Equals(currentColor, color)) return;
        // _coroutineManager.TimedAction("bbox-opacity", 
        //     (progress) => {
        //         Color currentColor = boundingBox.GetComponent<Renderer>().material.color;
        //         boundingBox.GetComponent<Renderer>().material.color = Color.Lerp(currentColor, color, progress);
        //     },
        //     onComplete : () => {
        //         boundingBox.GetComponent<Renderer>().material.color = color;
        //     },
        //     duration : duration,
        //     delay : delay
        // );
    }

    # endregion
}

















// public Color colorPlayable = new Color(255, 255, 255, 0.0f);
// public Color colorInspecting = new Color(255, 255, 255, 1f);
// public Color colorPositioning = new Color(255, 255, 255, 1f);

// public GrainModelState State { get; set; } = GrainModelState.Unplaced;

// public delegate void OnRepositionStart();
// public event OnRepositionStart OnRepositionStartEvent;

// public delegate void OnRepositionEnd();
// public event OnRepositionEnd OnRepositionEndEvent;

// private bool _isInspecting = false;


// # region Lifecycle
// private void OnEnable() {
    // coroutineManager = new TransformCoroutineManager(this, () => {
    //     Debug.Log("GrainModel ON COROUTINE START");
    //     // TsvrApplication.DebugLogger.Log("Sending Spring Toggle broadcast message -> OFF", "[GrainModel]");
    //     // BroadcastMessage("ToggleReposition", true, SendMessageOptions.DontRequireReceiver);
    //     BroadcastMessage("ChangePositioningState", Grain.GrainState.Repositioning, SendMessageOptions.DontRequireReceiver);
    //     // OnRepositionStartEvent?.Invoke();
    // }, () => {
    //     Debug.Log("GrainModel ON COROUTINE END");
    //     DebugLogger.Log("Sending Spring Toggle broadcast message -> ON", "[GrainModel]");
    //     if (HasBeenPlaced) {
    //         BroadcastMessage("ChangePositioningState", Grain.GrainState.Idle, SendMessageOptions.DontRequireReceiver);
    //         // BroadcastMessage("ToggleReposition", false, SendMessageOptions.DontRequireReceiver);
    //         // OnRepositionEndEvent?.Invoke();
    //     }
    // });
// }

// # endregion


// public void Inspect() {
//     boundingBox.GetComponent<Renderer>().material.color = colorInspecting;
//     // _isInspecting = true;
//     _coroutineManager.TimedAction("inspect-timeout", null, Time.deltaTime * 2f, null, () => {
//         // _isInspecting = false;
//         SetColor(colorPlayable, 0.2f);
//     });
// }

// private void ChangeState(GrainModelState newState) {
//     var currentState = State;
//     switch (newState) {
//         case GrainModelState.Positioning:
//             SetColor(colorPositioning, 0.3f);
//             break;
//         case GrainModelState.Playable:
//             _coroutineManager.Freeze();
//             SetColor(colorPlayable, 0.3f, 1f);
//             break;
//     }
//     State = newState;
// }
    
// # region Debugging
// public void OnSnap()
// {
//     throw new NotImplementedException();
// }

// public void OnUnsnap()
// {
//     throw new NotImplementedException();
// }

// public void OnSnapVolumeEnter(Collider other)
// {
//     throw new NotImplementedException();
// }

// #endregion

// public void Place() {
//     ChangeState(GrainModelState.Playable);
//     // OnRepositionEndEvent?.Invoke();
//     // BroadcastMessage("ToggleReposition", false, SendMessageOptions.DontRequireReceiver);
// }

// public void Reposition(Vector3 position, Quaternion rotation, Vector3 scale, float duration = 0.5f) {
//     _coroutineManager.MoveTo(position, duration);
//     _coroutineManager.RotateTo(rotation, duration);
//     _coroutineManager.ScaleTo(scale, duration);
// }
    
// public void Reposition(Vector3 position, float duration = 0.5f) {
//     _coroutineManager.MoveTo(position, duration);
// }

// public void Rotate(Quaternion rotation, float duration = 0.5f) {
//     _coroutineManager.RotateTo(rotation, duration);
// }

// public void RotateAngle(Vector3 angles, float duration = 0.5f) {
//     _coroutineManager.RotateTo(Quaternion.Euler(angles), duration);
// }

// // public void AddRotation()

// public void Scale(Vector3 scale, float duration = 0.5f) {
//     _coroutineManager.ScaleTo(scale, duration);
// }