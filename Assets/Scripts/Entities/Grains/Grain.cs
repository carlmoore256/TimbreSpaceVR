using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// A node that visually represents an audio grain
/// </summary>
public class Grain : MonoBehaviour, IPositionedSequenceable
{
    public Material material;

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

    public enum GrainState {
        Idle,
        Repositioning
    }

    private GrainState currentState = GrainState.Idle;

    public enum ActivationAction {
        Play,
        Select,
        Delete
    }

    // public delegate void OnSelect(Grain grain, object caller);
    // public event OnSelect OnSelectEvent;

    // public delegate void OnActivate(Grain grain, float value, ActivationAction activationAction);
    // public event OnActivate OnActivateEvent;

    public Action<Grain, float, ActivationAction> OnActivate;

    // public Action<int, double> OnSchedule;


    public void Initialize(int grainID) {
        ID = grainID;
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

    # region ISequenceable

    public int ID { get; private set; }
    public Vector3 Position { get => transform.position; }
    public event EventHandler<SequenceableScheduleParameters> OnSchedule;
    public event Action OnSequenceablePlayStart;
    public event Action OnSequenceablePlayEnd;

    public void Schedule(SequenceableScheduleParameters parameters) {
        OnSchedule?.Invoke(this, parameters);
    }

    public void SequenceablePlayStart() {
        PlayActivatedAnimation(Color.green, 1.8f, 10f);
        OnSequenceablePlayStart?.Invoke();
    }

    public void SequenceablePlayEnd() {
        OnSequenceablePlayEnd?.Invoke();
    }

    # endregion

    # region Public Methods

    /// <summary>
    /// Notifies listeners of attempt to activate, with self, value, and delta time since last activated
    /// </summary>
    public void Activate(float value, ActivationAction activationAction) {
        OnActivate?.Invoke(this, value, activationAction);
    }


    /// <summary>
    /// Play an activated animation
    /// </summary>
    public void PlayActivatedAnimation(Color color, float radiusMultiplier = 1.2f, float duration = 1f) {
        isActivated = true;
        activateEnd = Time.time + duration;
        lastActivated = Time.timeAsDouble;
        lodRenderer.ChangeColor(color);
        transform.localScale = targetTransform.scale * radiusMultiplier;
        activatedDuration = duration;
        if (playCoroutine != null)
            StopCoroutine(playCoroutine);
        playCoroutine = StartCoroutine(PlayCoroutine(color, radiusMultiplier, duration));
    }

    public void Delete() {
        UpdateScale(0f);
        Destroy(gameObject, durationReScale + 0.1f);
    }

    public double TimeSinceLastPlayed() {
        return Time.timeAsDouble - lastActivated;
    }

    public void UpdatePosition(Vector3 newPosition) {
        if (positionCoroutine != null)
            StopCoroutine(positionCoroutine);
        targetTransform.position = newPosition;
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
        targetColor = color;
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

    # endregion

    public void ChangePositioningState(GrainState state) {
        currentState = state;
        switch (state) {
            case GrainState.Idle:
                ToggleSpring(true);
                break;
            case GrainState.Repositioning:
                ToggleSpring(false);
                break;
        }
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

    private void ToggleSpring(bool enable) {
        if (enable) {
            // turn spring on
            // if (isRepositioning) return;
            if (currentState == GrainState.Repositioning) return;
            joint.connectedAnchor = transform.position;
            GetComponent<Rigidbody>().isKinematic = false;
        } else {
            // turn spring off
            GetComponent<Rigidbody>().isKinematic = true;
        }
    }


    # region Coroutines

    private Coroutine playCoroutine;
    private IEnumerator PlayCoroutine(Color color, float radiusMultiplier = 1.2f, float duration = 1f) {
        lodRenderer.ChangeColor(color);
        transform.localScale = targetTransform.scale * radiusMultiplier;
        float time = 0f;        
        // while (time < activateEnd) {
        while (time < duration) {
            lodRenderer.ChangeColor(Color.Lerp(lodRenderer.GetColor(), targetColor, time/duration));
            transform.localScale = Vector3.Lerp(transform.localScale, targetTransform.scale, time/duration);
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
            Vector3 test = Vector3.Lerp(transform.localPosition, targetPosition, time/duration);
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, time/duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = targetPosition;
        // ToggleSpring(true);
        currentState = GrainState.Idle;
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