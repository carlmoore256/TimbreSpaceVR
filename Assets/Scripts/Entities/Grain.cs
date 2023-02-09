using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;

public class Grain : MonoBehaviour
{
    public Material material;
    public bool UseHSV { get; set; } = false;
    private LODRenderer lodRenderer;
    private GrainAudioFeatures features;
    private SpringJoint joint;

    private float durationRePosition = 0.5f;
    private float durationReColor = 0.5f;
    private float durationReScale = 0.5f;

    private Vector3 targetPosition;
    private Vector3 targetScale;
    private Quaternion targetRotation;
    private Color targetColor;

    public bool IsDisplayingInfo { get; set; } = false;

    private Action<PlaybackEvent> onGrainActivated;
    private PlaybackEvent playbackEvent;

    private double lastPlayTime = 0d;
    private double playTimeout;

    private bool isActivated = false;


    private float playEnd = 0f;
    private float totalPlayDuration = 1f;

    private void OnEnable() {
        transform.localScale = Vector3.zero;
        lodRenderer = new LODRenderer(transform.Find("LOD").gameObject, material);
        joint = GetComponent<SpringJoint>();
        playTimeout = TsvrApplication.Settings.GrainPlayTimeout.value;
        UseHSV = TsvrApplication.Settings.GrainUseHSV.value;
    }

    /// <summary>
    /// Initialize the grain with the given features and model parameters
    /// </summary>
    public void Initialize(GrainAudioFeatures features, GrainModelParameters modelParameters, Action<PlaybackEvent> onGrainActivated) {
        this.features = features;
        this.onGrainActivated = onGrainActivated;
        playbackEvent = new PlaybackEvent(0f, features.WindowTime, features.Get(AudioFeature.RMS), gameObject.GetInstanceID());

        playTimeout = modelParameters.WindowSize / (double)AudioManager.Instance.SampleRate;
        playTimeout /= 4d;
        joint.tolerance = TsvrApplication.Settings.ParticleTolerance.value;

        transform.localPosition  = new Vector3(
            features.Get(modelParameters.PositionFeatures[0]),
            features.Get(modelParameters.PositionFeatures[1]),
            features.Get(modelParameters.PositionFeatures[2]));

        UpdatePosition(
            modelParameters.PositionFeatures[0], 
            modelParameters.PositionFeatures[1], 
            modelParameters.PositionFeatures[2],
            Vector3.one);

        UpdateColor(
            modelParameters.ColorFeatures[0], 
            modelParameters.ColorFeatures[1], 
            modelParameters.ColorFeatures[2],
            UseHSV);

        UpdateScale(modelParameters.ScaleFeature, modelParameters.ScaleMult, 2f);
    }

    void Update() {
        if (isActivated) { // this saves resources as opposed to starting new coroutines
            if (playEnd > Time.time) {
                playEnd -= Time.deltaTime;
                float lerp = 1f - ((playEnd - Time.time) / totalPlayDuration);
                lodRenderer.ChangeColorCycle(Color.Lerp(lodRenderer.GetColor(), targetColor, Mathf.Pow(lerp, 0.5f)));
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, lerp);
            } else {
                // apply changes to all lods
                lodRenderer.ChangeColor(targetColor);
                transform.localScale = targetScale;
                isActivated = false;
            }
        }
    }
    

    
    /// <summary>
    /// Play Grain audio as a one shot event
    /// </summary>
    public void PlayGrain(float gain = 1.0f)
    {
        if (Time.timeAsDouble - lastPlayTime < playTimeout) return;
        // if (playCoroutine != null) {
        //     playDuration += 1f;
        //     // StopCoroutine(playCoroutine);
        // } else {

        isActivated = true;

        playEnd = Time.time + totalPlayDuration;
        // playCoroutine = StartCoroutine(PlayCoroutine());
        transform.localScale = targetScale * 1.5f;
        lodRenderer.ChangeColor(Color.red);      
        playbackEvent.gain = gain;
        lastPlayTime = Time.timeAsDouble;
        onGrainActivated?.Invoke(playbackEvent);
    }

    /// <summary>
    /// Update Grain position to provided axes of GrainFeatures 
    /// </summary>
    /// <param name="fX">Audio Feature for X axis</param>
    /// <param name="fY">Audio Feature for Y axis</param>
    /// <param name="fZ">Audio Feature for Z axis</param>
    /// <param name="axisScale">scale of each axis, default is 1.0f</param>
    public void UpdatePosition(AudioFeature fX, AudioFeature fY, AudioFeature fZ, Vector3 axisScale)
    {   
        if (axisScale == null) axisScale = Vector3.one;
        Vector3 targetPosition = new Vector3(
            features.Get(fX) * axisScale.x, 
            features.Get(fY) * axisScale.y, 
            features.Get(fZ) * axisScale.z
        );
        if (positionCoroutine != null)
            StopCoroutine(positionCoroutine);
        positionCoroutine = StartCoroutine(PositionCoroutine(targetPosition, durationRePosition));
    }

    /// <summary>
    /// Update rotation of Grain given an Audio Feature for each axis 
    /// </summary>
    /// <param name="f">Audio Feature for Grain scale</param>
    /// <param name="multiplier">Additional multiplier of each axis, default is 1.0f</param>
    public void UpdateScale(AudioFeature f, float multiplier=0.05f, float scaleExp=1f)
    {
        float radius = features.Get(f, positive : true);
        if (scaleExp != 1f) radius = Mathf.Pow(multiplier, scaleExp);
        radius *= multiplier;


        if (radius > TsvrApplication.Settings.GrainMaxRadius.value)
            radius = TsvrApplication.Settings.GrainMaxRadius.value;
        else if (radius < TsvrApplication.Settings.GrainMinRadius.value)
            radius = TsvrApplication.Settings.GrainMinRadius.value;
        // Debug.Log($"Updating Scale | Radius {radius} | Feature {features.Get(f, true)} | Multiplier {multiplier} | Scale Exp {scaleExp} | Min {scaleMin}");

        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);
        targetScale = new Vector3(radius, radius, radius);

        // also change the mass of the rigidbody
        GetComponent<Rigidbody>().mass = radius * 10f;

        scaleCoroutine = StartCoroutine(ScaleCoroutine(targetScale, durationReScale));
    }

    /// <summary>
    /// Update color of Grain given an Audio Feature for each axis
    /// </summary>
    /// <param name="hsv">if true, interpret features as HSV values, otherwise RGB</param>
    public void UpdateColor(AudioFeature fR, AudioFeature fG, AudioFeature fB, bool hsv=false)
    {
        if (hsv) targetColor = Color.HSVToRGB(features.Get(fR, positive : true), features.Get(fG, positive : true), features.Get(fB, positive : true));
        else targetColor = new Color(features.Get(fR, positive : true), features.Get(fG, positive : true), features.Get(fB, positive : true));
        if (colorCoroutine != null)
            StopCoroutine(colorCoroutine);
        colorCoroutine = StartCoroutine(ColorCoroutine(targetColor, durationReColor));
    }

    public void ToggleSpring(bool enable) {
        if (enable) {
            joint.connectedAnchor = transform.position;
            GetComponent<Rigidbody>().isKinematic = false;
        } else {
            GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    /// ==================== COROUTINES ==================== ///

    private Coroutine playCoroutine;
    IEnumerator PlayCoroutine() {
        lodRenderer.ChangeColor(Color.red);
        transform.localScale = targetScale * 1.5f;
        float time = 0f;        
        while (time < playEnd) {
            lodRenderer.ChangeColor(Color.Lerp(lodRenderer.GetColor(), targetColor, time/playEnd));
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, time/playEnd);
            time += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);
        lodRenderer.ChangeColor(targetColor);
        transform.localScale = targetScale;
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
        // GetComponent<Rigidbody>().isKinematic = true; // disable physics
        ToggleSpring(false);
        float time = 0f;        
        while (time < duration) {
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, time/duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = targetPosition;
        // joint.connectedAnchor = transform.position;
        // GetComponent<Rigidbody>().isKinematic = false;
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
}