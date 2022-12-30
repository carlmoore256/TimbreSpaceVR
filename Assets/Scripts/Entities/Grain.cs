using System.Collections;
using UnityEngine;


public class Grain : MonoBehaviour
{
    private LODRenderer lodRenderer;
    private AudioSource audioSource;
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


    private void OnEnable() {
        transform.localScale = Vector3.zero;
        lodRenderer = new LODRenderer(transform.Find("LOD").gameObject);
        joint = GetComponent<SpringJoint>();
    }

    /// <summary>
    /// Initialize the grain with the given features and model parameters
    /// </summary>
    public void Initialize(GrainAudioFeatures features, GrainModelParameters modelParameters) {
        this.features = features;
        audioSource = GetComponent<AudioSource>(); // set the audio buffer for this grain
        // audioSource.clip = AudioClip.Create("Grain", features.AudioSamples.Length, 1, features.SampleRate, false);
        // audioSource.clip.SetData(features.AudioSamples, 0);
        joint.tolerance = TsvrApplication.Settings.particleTolerance;

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
            modelParameters.ColorFeatures[2]);

        UpdateScale(modelParameters.ScaleFeature);
    }
    
    /// <summary>
    /// Play Grain audio as a one shot event
    /// </summary>
    public void PlayGrain(float gain = 1.0f)
    {
        if (playCoroutine != null)
            StopCoroutine(playCoroutine);
        playCoroutine = StartCoroutine(PlayCoroutine(gain));
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
    /// <param name="scale">scale of each axis, default is 1.0f</param>
    public void UpdateScale(AudioFeature f, float scale=1f)
    {
        float radius = features.Get(f) * scale;
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);
        targetScale = new Vector3(radius, radius, radius);
        scaleCoroutine = StartCoroutine(ScaleCoroutine(targetScale, durationReScale));
    }

    /// <summary>
    /// Update color of Grain given an Audio Feature for each axis
    /// </summary>
    /// <param name="hsv">if true, interpret features as HSV values, otherwise RGB</param>
    public void UpdateColor(AudioFeature fR, AudioFeature fG, AudioFeature fB, bool hsv=false)
    {
        if (hsv) targetColor = Color.HSVToRGB(features.Get(fR), features.Get(fG), features.Get(fB));
        else targetColor = new Color(features.Get(fR), features.Get(fG), features.Get(fB));
        if (colorCoroutine != null)
            StopCoroutine(colorCoroutine);
        colorCoroutine = StartCoroutine(ColorCoroutine(targetColor, durationReColor));
    }

    /// ==================== COROUTINES ==================== ///
    private Coroutine playCoroutine;
    IEnumerator PlayCoroutine(float gain = 1.0f, float duration = 1f) {
        audioSource.PlayOneShot(audioSource.clip, gain);
        lodRenderer.ChangeColor(Color.red);
        transform.localScale = targetScale * 1.5f;
        float time = 0f;        
        while (time < duration) {
            lodRenderer.ChangeColor(Color.Lerp(lodRenderer.GetColor(), targetColor, time/duration));
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, time/duration);
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
        GetComponent<Rigidbody>().isKinematic = true; // disable physics
        float time = 0f;        
        while (time < duration) {
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, time/duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = targetPosition;
        joint.connectedAnchor = transform.position;
        GetComponent<Rigidbody>().isKinematic = false;
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