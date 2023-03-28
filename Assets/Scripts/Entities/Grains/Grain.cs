using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// A node that visually represents an audio grain
/// </summary>
public class Grain : MonoBehaviour
{
    public Material material;
    public int GrainID { get; private set; }

    private LODRenderer lodRenderer;
    private SpringJoint joint;

    private float durationRePosition = 0.5f;
    private float durationReColor = 0.5f;
    private float durationReScale = 0.5f;

    private TransformSnapshot targetTransform;
    private Color targetColor;
    private bool isRepositioning = false;

    private double lastActivated = 0d;
    private float activateEnd = 0f;
    private float activatedDuration = 1f;
    private bool isActivated = false;

    // public delegate void OnSelect(Grain grain, object caller);
    // public event OnSelect OnSelectEvent;

    public delegate void OnActivate(Grain grain, float value, object caller);
    public event OnActivate OnActivateEvent;
    

    public void Initialize(int grainID) {
        GrainID = grainID;
    }

    # region MonoBehaviours

    private void OnEnable() {
        transform.localScale = Vector3.zero;
        lodRenderer = new LODRenderer(transform.Find("LOD").gameObject, material);
        joint = GetComponent<SpringJoint>();
    }

    void Update() {
        if (isActivated) { // this saves resources as opposed to starting new coroutines
            if (activateEnd > Time.time) {
                activateEnd -= Time.deltaTime;
                float lerp = 1f - ((activateEnd - Time.time) / activatedDuration);
                lodRenderer.ChangeColorCycle(Color.Lerp(lodRenderer.GetColor(), targetColor, Mathf.Pow(lerp, 0.5f)));
                transform.localScale = Vector3.Lerp(transform.localScale, targetTransform.scale, lerp);
            } else {
                // apply changes to all lods
                lodRenderer.ChangeColor(targetColor);
                transform.localScale = targetTransform.scale;
                isActivated = false;
            }
        }
    }

    # endregion

    # region Public Methods

    // /// <summary>
    // /// Generic event that can be called by tools and other interface elements
    // /// For instance, if this is invoked by the wand select tool,
    // /// the grain cloud that owns it should be subscribed to OnSelectEvent, and
    // /// should be able to add this grain to the selection
    // /// </summary>
    // public void Select(object caller) {
    //     OnSelectEvent?.Invoke(this, caller);
    // }

    /// <summary>
    /// Notifies listeners of attempt to activate, with self, value, and delta time since last activated
    /// </summary>
    public void Activate(float value, object caller) {
        OnActivateEvent?.Invoke(this, value, caller);
    }


    /// <summary>
    /// Play an activated animation
    /// </summary>
    public void PlayAnimation(Color color, float radiusMultiplier = 1.2f, float duration = 1f) {
        isActivated = true;
        activateEnd = Time.time + duration;
        lastActivated = Time.timeAsDouble;
        lodRenderer.ChangeColor(color);
        transform.localScale = targetTransform.scale * radiusMultiplier;
        activatedDuration = duration;
        if (playCoroutine != null)
            StopCoroutine(playCoroutine);
        playCoroutine = StartCoroutine(PlayCoroutine(color, radiusMultiplier));
    }

    public double TimeSinceLastPlayed() {
        return Time.timeAsDouble - lastActivated;
    }

    public void UpdatePosition(Vector3 newPosition) {
        if (positionCoroutine != null)
            StopCoroutine(positionCoroutine);
        positionCoroutine = StartCoroutine(PositionCoroutine(targetTransform.position, durationRePosition));
    }

    public void UpdateScale(float radius) {
        if (radius > TsvrApplication.Settings.GrainMaxRadius.value)
            radius = TsvrApplication.Settings.GrainMaxRadius.value;
        else if (radius < TsvrApplication.Settings.GrainMinRadius.value)
            radius = TsvrApplication.Settings.GrainMinRadius.value;
        GetComponent<Rigidbody>().mass = radius * 10f;
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);
        targetTransform.scale = new Vector3(radius, radius, radius);
        scaleCoroutine = StartCoroutine(ScaleCoroutine(targetTransform.scale, durationReScale));
    } 

    public void UpdateColor(Color color) {
        if (colorCoroutine != null)
            StopCoroutine(colorCoroutine);
        colorCoroutine = StartCoroutine(ColorCoroutine(targetColor, durationReColor));
    }

    /// <summary>
    /// Interrupt any current animations and reset to target values
    /// </summary>
    public void ResetAnimation(float duration = 0.1f) {
        if (positionCoroutine != null)
            StopCoroutine(positionCoroutine);
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);
        if (colorCoroutine != null)
            StopCoroutine(colorCoroutine);
        positionCoroutine = StartCoroutine(PositionCoroutine(targetTransform.position, duration));
        scaleCoroutine = StartCoroutine(ScaleCoroutine(targetTransform.scale, duration));
        colorCoroutine = StartCoroutine(ColorCoroutine(targetColor, duration));
    }

    public void ToggleReposition(bool enable) {
        if (enable) {
            isRepositioning = true;
            ToggleSpring(false);
        } else {
            isRepositioning = false;
            ToggleSpring(true);
        }
    }

    # endregion



    private void ToggleSpring(bool enable) {
        if (enable) {
            if (isRepositioning) return;
            joint.connectedAnchor = transform.position;
            GetComponent<Rigidbody>().isKinematic = false;
        } else {
            GetComponent<Rigidbody>().isKinematic = true;
        }
    }


    # region Coroutines

    private Coroutine playCoroutine;
    private IEnumerator PlayCoroutine(Color color, float radiusMultiplier = 1.2f, float duration = 1f) {
        lodRenderer.ChangeColor(color);
        transform.localScale = targetTransform.scale * radiusMultiplier;
        float time = 0f;        
        while (time < activateEnd) {
            lodRenderer.ChangeColor(Color.Lerp(lodRenderer.GetColor(), targetColor, time/activateEnd));
            transform.localScale = Vector3.Lerp(transform.localScale, targetTransform.scale, time/activateEnd);
            time += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);
        lodRenderer.ChangeColor(targetColor);
        transform.localScale = targetTransform.scale;
    }

    private Coroutine scaleCoroutine;
    IEnumerator ScaleCoroutine(Vector3 targetScale, float duration = 1f) {
        float time = 0f;
        while (time < duration) {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, time/duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale;
    }

    private Coroutine positionCoroutine;
    IEnumerator PositionCoroutine(Vector3 targetPosition, float duration = 1f) {
        ToggleSpring(false);
        float time = 0f;
        while (time < duration) {
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, time/duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = targetPosition;
        ToggleSpring(true);
    }

    private Coroutine rotateCoroutine;
    IEnumerator RotateCoroutine(Quaternion targetRotation, float duration = 1f) {
        float time = 0f;
        while (time < duration) {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, time/duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.rotation = targetRotation;
    }

    private Coroutine colorCoroutine;
    IEnumerator ColorCoroutine(Color targetColor, float duration = 1f) {
        float time = 0f;        
        while (time < duration) {
            lodRenderer.ChangeColor(Color.Lerp(lodRenderer.GetColor(), targetColor, time/duration));
            time += Time.deltaTime;
            yield return null;
        }
        lodRenderer.ChangeColor(targetColor);
    }

    # endregion
}