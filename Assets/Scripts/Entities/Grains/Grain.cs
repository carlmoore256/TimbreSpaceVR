using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// A node that visually represents an audio grain
/// </summary>
public class Grain : MonoBehaviour, IInteractableSequenceable, IWandInteractable
{
    public Material material;
    public int GrainIndex { get; set; }
    public WindowTime Window { get; set; }

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

    // public enum InteractionType {
    //     Play,
    //     Select,
    //     Delete
    // }

    public event EventHandler<WandInteraction> OnWandInteract;


    // as long as the parent has a guid, we don't need a guid to locate the item under 
    // the parent if the collection is of a fixed size that generates deterministically
    // maybe if guids need to be set manually they can be done so outside of this class
    public void Initialize(WindowTime window) {
        // GrainIndex = grainIndex;
        Window = window;
        Id = Guid.NewGuid();
    }

    # region MonoBehaviours

    private void OnEnable() {
        transform.localScale = Vector3.zero;
        lodRenderer = new LODRenderer(transform.Find("LOD").gameObject, material);
        joint = GetComponent<SpringJoint>();

        if (TsvrApplication.Settings.EnableGrainDebugGizmos) {
            var gizmo = gameObject.GetOrAddComponent<DebugGizmo>();
        }
    }

    void Update() {
        if (isActivated) { // this saves resources as opposed to starting new coroutines
            if (activateEnd > Time.time) {
                activateEnd -= Time.deltaTime;
                float lerp = 1f - ((activateEnd - Time.time) / activatedDuration);
                lodRenderer.ChangeColorCycle(Color.Lerp(lodRenderer.GetColor(), targetColor, Mathf.Pow(lerp, 0.5f)));
                transform.localScale = Vector3.Lerp(transform.localScale, targetTransform.Scale, lerp);
            } else {
                // apply changes to all lods
                lodRenderer.ChangeColor(targetColor);
                transform.localScale = targetTransform.Scale;
                isActivated = false;
            }
        }
    }

    # endregion

    # region ISequenceable

    public Guid Id { get; private set; }
    public Vector3 Position { get => transform.position; }
    public event EventHandler<(double, SequenceableParameters, ScheduleCancellationToken)> OnSchedule;
    public event Action OnSequenceablePlayStart;
    public event Action OnSequenceablePlayEnd;

    public ScheduleCancellationToken Schedule(double time, SequenceableParameters parameters) {
        var token = new ScheduleCancellationToken();
        parameters.Color = targetColor;
        OnSchedule?.Invoke(this, (time, parameters, token));
        return token;
    }

    public void SequenceablePlayStart() {
        PlayActivatedAnimation(Color.red, 1.8f, 10f);
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
    public void DoWandInteraction(WandInteraction wandInteraction) {

        // potentiall here we could decide that certain WandInteractionTypes
        // are handled by the grain itself, rather than its parent the playable collection
        OnWandInteract?.Invoke(this, wandInteraction);
        OnWandInteractionStart(wandInteraction.ActionType);
    }

    public void OnWandInteractionStart(WandInteractionType interactionType)
    {
        Debug.Log("Grain.OnWandInteractionStart");
    }

    public void OnWandInteractionEnd(WandInteractionType interactionType)
    {
        Debug.Log("Grain.OnWandInteractionEnd");
    }


    /// <summary>
    /// Play an activated animation
    /// </summary>
    public void PlayActivatedAnimation(Color color, float radiusMultiplier = 1.2f, float duration = 1f) {
        isActivated = true;
        activateEnd = Time.time + duration;
        lastActivated = Time.timeAsDouble;
        lodRenderer.ChangeColor(color);
        transform.localScale = targetTransform.Scale * radiusMultiplier;
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
        targetTransform.Position = newPosition;
        positionCoroutine = StartCoroutine(PositionCoroutine(targetTransform.Position, durationRePosition));
    }

    public void UpdateScale(float radius) {
        if (radius > TsvrApplication.Settings.GrainMaxRadius)
            radius = TsvrApplication.Settings.GrainMaxRadius;
        else if (radius < TsvrApplication.Settings.GrainMinRadius)
            radius = TsvrApplication.Settings.GrainMinRadius;
        GetComponent<Rigidbody>().mass = radius * 10f;
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);
        targetTransform.Scale = new Vector3(radius, radius, radius);
        scaleCoroutine = StartCoroutine(ScaleCoroutine(targetTransform.Scale, durationReScale));
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
        positionCoroutine = StartCoroutine(PositionCoroutine(targetTransform.Position, duration));
        scaleCoroutine = StartCoroutine(ScaleCoroutine(targetTransform.Scale, duration));
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
        transform.localScale = targetTransform.Scale * radiusMultiplier;
        float time = 0f;        
        // while (time < activateEnd) {
        while (time < duration) {
            lodRenderer.ChangeColor(Color.Lerp(lodRenderer.GetColor(), targetColor, time/duration));
            transform.localScale = Vector3.Lerp(transform.localScale, targetTransform.Scale, time/duration);
            time += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);
        lodRenderer.ChangeColor(targetColor);
        transform.localScale = targetTransform.Scale;
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