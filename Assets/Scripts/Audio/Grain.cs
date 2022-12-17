using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// rename to GrainMgr
public class Grain : MonoBehaviour
{
    GrainFeatures features;

    Renderer renderer;
    //AudioClip audioClip;
    AudioSource audioSource;
    float[] audioFeatures;

    Color targetColor;

    Coroutine colorLerp;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Vector3 targetScale;

    public bool displayInfo = false;

    bool grainEnabled = false;

    //public RuntimeMgr rtmgr;

    public void Initialize(
        Vector3 position,
        Vector3 scale,
        GrainFeatures gf,
        string name
        )
    {
        transform.localPosition = position;
        //transform.localScale = scale;
        transform.localScale = Vector3.zero; // start invisible, lerp up in size
        targetPosition = position;
        targetScale = scale;

        features = gf;

        // set the audio buffer for this grain
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = AudioClip.Create(name, gf.audioSamples.Length, 1, gf.sampleRate, false);
        audioSource.clip.SetData(gf.audioSamples, 0);

        // set the color based on defualt features
        renderer = GetComponent<Renderer>();
        targetColor = new Color(gf.mfccs[3], gf.mfccs[4], gf.mfccs[5]);

        grainEnabled = true;
    }

    void Start()
    {
    }

    void Update()
    {
        if(grainEnabled)
        {
            if (!renderer.material.color.Equals(targetColor))
                renderer.material.color = Color.Lerp(renderer.material.color, targetColor, Time.deltaTime * 10f);

            if (!Vector3.Equals(transform.position, targetPosition))
            {
                //transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, Time.deltaTime * 5f);
                transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * 10f);
                if (Vector3.Distance(transform.localPosition, targetPosition) < 0.001)
                    transform.position = targetPosition;
            }

            if (displayInfo && !Quaternion.Equals(transform.rotation, targetRotation))
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * 5f);

            if (!Vector3.Equals(transform.localScale, targetScale))
            {
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 1f);
                if (Vector3.Distance(transform.localScale, targetScale) < 0.001)
                    transform.localScale = targetScale;
            }
        }
    }
    private void Play() {
        audioSource.PlayOneShot(audioSource.clip, 1.0f);

        if (colorLerp != null)
            StopCoroutine(colorLerp);
        colorLerp = StartCoroutine(ColorChange());
    }

    public void PlayGrain()
    {
        Play();
        // audioSource.PlayOneShot(audioSource.clip, 1.0f);
        // if (colorLerp != null)
        //     StopCoroutine(colorLerp);
        // colorLerp = StartCoroutine(ColorChange());
    }
    public void PlayInSequence(double scheduleTime) {

    }

    // grain can ask for fx setup from static fx manager or the existing grain fx. Grain fx can override unless a global
    // fx setting disables this


    IEnumerator ColorChange()
    {
        renderer.material.color = Color.red;
        yield return new WaitForSeconds(1.0f);
        renderer.material.color = new Color(features.mfccs[3], features.mfccs[4], features.mfccs[5]);
    }

    // request new position based on feature indicies [x, y, z]
    public void UpdatePosition(string x_F, string y_F, string z_F, Vector3 ax_scale)
    {
        targetPosition = new Vector3(
            features.featureDict[x_F] * ax_scale.x,
            features.featureDict[y_F] * ax_scale.y,
            features.featureDict[z_F] * ax_scale.z
        );
    }

    // request new scale based on feature indicies [x, y, z]
    public void UpdateScale(string x_F, string y_F, string z_F, float scale)
    {
        targetScale = new Vector3(
            features.featureDict[x_F] * scale,
            features.featureDict[y_F] * scale,
            features.featureDict[z_F] * scale
        );
    }

    // request new color based on feature indicies [x, y, z]
    public void UpdateColor(string r_F, string g_F, string b_F, bool hsv)
    {
        if (hsv)
        {
            targetColor = Color.HSVToRGB(
                features.featureDict[r_F],
                features.featureDict[g_F],
                features.featureDict[b_F]);
        } else {
            targetColor = new Color(
                features.featureDict[r_F],
                features.featureDict[g_F],
                features.featureDict[b_F]);
        }
    }
}
