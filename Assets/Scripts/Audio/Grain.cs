using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// rename to GrainMgr
[CreateAssetMenu(fileName = "TSVR_Grain", menuName = "TSVR/Grain")]
public class Grain : ScriptableObject
{
    private GameObject grainObject;
    private Renderer renderer;
    private AudioSource audioSource;
    private GrainFeatures features;
    private SpringJoint springJoint;

    private Color targetColor;
    private Coroutine colorLerp;

    private Vector3 targetPosition;
    private Vector3 targetScale;
    private Quaternion targetRotation;

    public bool isDisplayingInfo = false;


    private void OnEnable() {
        grainObject = Instantiate(TsvrApplication.Config.grainPrefab);
        renderer = grainObject.GetComponent<Renderer>();
    }

    private void OnDisable() {
        if (grainObject != null)
            Destroy(grainObject);
    }

    /// <summary>
    /// Initialize the grain with the given features and model parameters
    /// </summary>
    public void Initialize(GrainFeatures features, GrainModelParameters modelParameters) {
        this.features = features;
        audioSource = grainObject.GetComponent<AudioSource>(); // set the audio buffer for this grain
        audioSource.clip = AudioClip.Create("Grain", features.AudioSamples.Length, 1, features.SampleRate, false);
        audioSource.clip.SetData(features.AudioSamples, 0);

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

        this.targetRotation = new Quaternion(0, 0, 0, 0);
        springJoint = grainObject.GetComponent<SpringJoint>(); 
        springJoint.tolerance = TsvrApplication.Settings.particleTolerance;

        renderer = grainObject.GetComponent<Renderer>();
    }

    public void Update()
    {
        // instead of using target position, just reconfigure the connected anchor for the spring joint instead
        // if (isMoving && !Vector3.Equals(grainObject.transform.position, targetPosition))
        // {
        //     //transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, Time.deltaTime * 5f);
        //     grainObject.transform.localPosition = Vector3.Lerp(grainObject.transform.localPosition, targetPosition, Time.deltaTime * 10f);
        //     if (Vector3.Distance(grainObject.transform.localPosition, targetPosition) < 0.001)
        //         grainObject.transform.position = targetPosition;
        // }

        if (!renderer.material.color.Equals(targetColor)) {
            renderer.material.color = Color.Lerp(renderer.material.color, targetColor, Time.deltaTime * 10f);
        }

        if (isDisplayingInfo && !Quaternion.Equals(grainObject.transform.rotation, targetRotation)) {
            grainObject.transform.rotation = Quaternion.RotateTowards(grainObject.transform.rotation, targetRotation, Time.deltaTime * 5f);
            if (Quaternion.Angle(grainObject.transform.rotation, targetRotation) < 0.001)
                grainObject.transform.rotation = targetRotation;
        }

        if (!Vector3.Equals(grainObject.transform.localScale, targetScale))
        {
            grainObject.transform.localScale = Vector3.Lerp(grainObject.transform.localScale, targetScale, Time.deltaTime * 1f);
            if (Vector3.Distance(grainObject.transform.localScale, targetScale) < 0.001)
                grainObject.transform.localScale = targetScale;
        }
    }

    /// <summary>
    /// Play Grain audio as a one shot event
    /// </summary>
    public void PlayGrain(float gain = 1.0f)
    {
        audioSource.PlayOneShot(audioSource.clip, gain);
        // if (colorLerp != null)
        //     StopCoroutine(colorLerp);
        // colorLerp = StartCoroutine(ColorChange());
    }

    /// <summary> 
    /// Grain can ask for fx setup from static fx manager or the existing grain fx. 
    /// Grain fx can override unless a global fx setting disables this
    /// </summary>
    IEnumerator ColorChange()
    {
        renderer.material.color = Color.red;
        yield return new WaitForSeconds(1.0f);
        renderer.material.color = new Color(features.mfccs[3], features.mfccs[4], features.mfccs[5]);
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
        // targetPosition = new Vector3(
        //     features.Get(fX) * axisScale.x, 
        //     features.Get(fY) * axisScale.y, 
        //     features.Get(fZ) * axisScale.z
        // );
        springJoint.connectedAnchor = targetPosition;
    }

    /// <summary>
    /// Update rotation of Grain given an Audio Feature for each axis 
    /// </summary>
    /// <param name="f">Audio Feature for Grain scale</param>
    /// <param name="scale">scale of each axis, default is 1.0f</param>
    public void UpdateScale(AudioFeature f, float scale=1f)
    {
        float radius = features.Get(f) * scale;
        targetScale = new Vector3(radius, radius, radius);
    }

    /// <summary>
    /// Update color of Grain given an Audio Feature for each axis
    /// </summary>
    /// <param name="hsv">if true, interpret features as HSV values, otherwise RGB</param>
    public void UpdateColor(AudioFeature fR, AudioFeature fG, AudioFeature fB, bool hsv=false)
    {
        if (hsv) targetColor = Color.HSVToRGB(features.Get(fR), features.Get(fG), features.Get(fB));
        else targetColor = new Color(features.Get(fR), features.Get(fG), features.Get(fB));
    }









    // public void Initialize(Vector3 position, Vector3 scale, GrainFeatures gf, string name)
    // {
    //     grainObject.transform.position = position;
    //     anchorPosition = position;
    //     //transform.localScale = scale;
    //     grainObject.transform.localScale = Vector3.zero; // start invisible, lerp up in size
    //     targetPosition = position;
    //     targetScale = scale;
    //     features = gf;
    //     // set the audio buffer for this grain
    //     audioSource = grainObject.GetComponent<AudioSource>();
    //     audioSource.clip = AudioClip.Create(name, gf.AudioSamples.Length, 1, gf.SampleRate, false);
    //     audioSource.clip.SetData(gf.AudioSamples, 0);
    //     // set the color based on defualt features
    //     renderer = grainObject.GetComponent<Renderer>();
    //     // targetColor = new Color(features.mfccs[3], features.mfccs[4], features.mfccs[5]);
    //     UpdateSpringProperties();
    // }

    
    // request new position based on feature indicies [x, y, z]
    // public void UpdatePosition(string x_F, string y_F, string z_F, Vector3 ax_scale)
    // {
    //     targetPosition = new Vector3(
    //         features.features[x_F] * ax_scale.x,
    //         features.features[y_F] * ax_scale.y,
    //         features.features[z_F] * ax_scale.z
    //     );
    // }

    // request new scale based on feature indicies [x, y, z]
    // public void UpdateScale(string x_F, string y_F, string z_F, float scale)
    // {
    //     targetScale = new Vector3(
    //         features.features[x_F] * scale,
    //         features.features[y_F] * scale,
    //         features.features[z_F] * scale
    //     );
    // }

    // request new color based on feature indicies [x, y, z]
    // public void UpdateColor(string r_F, string g_F, string b_F, bool hsv=false)
    // {
    //     if (hsv)
    //     {
    //         targetColor = Color.HSVToRGB(
    //             features.features[r_F],
    //             features.features[g_F],
    //             features.features[b_F]);
    //     } else {
    //         targetColor = new Color(
    //             features.features[r_F],
    //             features.features[g_F],
    //             features.features[b_F]);
    //     }
    // }
}
