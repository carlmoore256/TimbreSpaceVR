using UnityEngine;
using System;
public class DebugGizmo : MonoBehaviour
{
    public float gizmoSize = 0.05f;
    public Color gizmoColor = Color.yellow;

    private bool _isEnabled;

    private void OnEnable()
    {
        _isEnabled = false;
    }

    private void OnDrawGizmos()
    {
        if (!_isEnabled) return;
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, gizmoSize);
    }

    public void ShowFor(float duration)
    {
        _isEnabled = true;
        CoroutineHelpers.DelayedAction(() => {
            _isEnabled = false;
        }, duration, this);
    }
}