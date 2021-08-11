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

    //public RuntimeMgr rtmgr;

    void Start()
    {
        renderer = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        
    }

    // request new position based on feature indicies [x, y, z]
    void UpdatePosition(int[] featureIdx)
    {

    }
}
