using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrainBehavior : MonoBehaviour
{
    public GrainFeatures grainFeatures;

    Renderer renderer;
    //AudioClip audioClip;
    AudioSource audioSource;
    float[] audioFeatures;

    Coroutine colorLerp;

    //public RuntimeMgr rtmgr;

    void Start()
    {
        renderer = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();

        //audioSource.clip = AudioClip.Create("DASDAS", grainFeatures.audioSamples.Length, 1, grainFeatures.sampleRate, false);
    }

    void Update()
    {
        
    }

    public void PlayGrain()
    {
        audioSource.PlayOneShot(audioSource.clip, 1.0f);

        if (colorLerp != null)
            StopCoroutine(colorLerp);
        colorLerp = StartCoroutine(ColorChange());
    }

    IEnumerator ColorChange()
    {
        renderer.material.color = Color.red;
        yield return new WaitForSeconds(1.0f);
        renderer.material.color = new Color(grainFeatures.mfccs[3], grainFeatures.mfccs[4], grainFeatures.mfccs[5]);
    }

    // request new position based on feature indicies [x, y, z]
    void UpdatePosition(int[] featureIdx)
    {

    }
}
