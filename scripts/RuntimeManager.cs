using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// changes aspects of the game's runtime properties such as grain phyisics and sorting
/// </summary>

public class RuntimeManager : MonoBehaviour
{
    //public BroadcastMove bm;
    public GameObject grainParent;
    public GameObject L_controller;
    public GameObject R_controller;
    Coroutine scaleActions;
    Coroutine outlineFade;
    Vector3 parentScale;
    float initDistance;
    bool resizeFlag;

    public List<GameObject> wandPlaybackL;
    public List<GameObject> wandPlaybackR;

    GameObject wandL;
    GameObject wandR;
    public bool staticGrain;
    bool staticGrainSwitch;

    //corner line parameters
    bool lineFade;
    bool fadeIn;
    bool fadeOut;
    bool fadeFlag;
    public bool displayCornerLines;
    public LineRenderer cornerLines;
    Color lineColorActive;
    Color lineColorInactive;
    int[] cornerTraversal = { 0, 1, 5, 6, 2, 3, 7, 4, 0, 3, 7, 6, 2, 1, 5, 4 };


    int playL;
    int playR;
    public bool randomizedPlayback;

    public Color inactive;

    public WandHUD whud;

    void Start()
    {
        wandL = GetComponent<CallingManager>().WandL;
        wandR = GetComponent<CallingManager>().WandR;
        staticGrainSwitch = staticGrain;
        playL = 0;
        playR = 0;
        lineFade = false;
        cornerLines.positionCount = 16;
        lineColorActive = cornerLines.GetComponent<Renderer>().material.color;
        parentScale = grainParent.GetComponent<Transform>().localScale;
    }

    void Update()
    {


        //Ability to rotate and resize grain model with two hand trigs
        /*
        if(OVRInput.Get(OVRInput.Button.PrimaryHandTrigger) && OVRInput.Get(OVRInput.Button.SecondaryHandTrigger))
        {
            if (resizeFlag)
            {
                parentScale = grainParent.transform.localScale;
                initDistance = Vector3.Distance(L_controller.transform.position, R_controller.transform.position);
                resizeFlag = false;
            }
                
            //timer to re-enable spring joints after selection has ended
            if (!staticGrain)
            {
                if (tempStatic != null)
                    StopCoroutine(tempStatic);
                tempStatic = StartCoroutine(TempStatic());
                SwitchStaticState(true);
            }
                
            //scales grain parent based on controller distance
            float controllerDist = Vector3.Distance(L_controller.transform.position, R_controller.transform.position);
            controllerDist -= initDistance;
            Vector3 newScale = parentScale + new Vector3(controllerDist, controllerDist, controllerDist);
            grainParent.transform.localScale = newScale;

            GetComponent<VibrationManager>().TriggerVibration(10, 3, 63, 0);
            GetComponent<VibrationManager>().TriggerVibration(10, 3, 63, 1);
        }
        */

        //Do static grain stuff
        if (staticGrain)
        {
            float Ltrig = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
            float Rtrig = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger);
            if (Ltrig > 0f)
            {
                int listCount = wandPlaybackL.Count;
                if (randomizedPlayback && listCount > 0)
                {
                    int randomGrain = Random.Range(0, listCount);
                    wandPlaybackL[randomGrain].GetComponent<GrainAudio>().Playback(Ltrig, 0, false);
                }
                else if(listCount > 0)
                {
                    wandPlaybackL[playL % listCount].GetComponent<GrainAudio>().Playback(Ltrig, 0, false);
                    playL++;
                }
            }

            if (Rtrig > 0f)
            {
                int listCount = wandPlaybackR.Count;
                if (randomizedPlayback && listCount > 0)
                {
                    int randomGrain = Random.Range(0, listCount);
                    wandPlaybackR[randomGrain].GetComponent<GrainAudio>().Playback(Rtrig, 0, false);
                }
                else if(listCount > 0)
                {
                    wandPlaybackR[playR % listCount].GetComponent<GrainAudio>().Playback(Rtrig, 0, false);
                    playR++;
                }
            }
        }

        if (displayCornerLines)
        {
            for(int a = 0; a < 16; a++)
            {
                GameObject currentCorner = GetComponent<CallingManager>().cornerPins[cornerTraversal[a]];
                cornerLines.SetPosition(a, currentCorner.GetComponent<Transform>().position);
            }

            if (lineFade)
            {
                if (fadeIn)
                {
                    cornerLines.GetComponent<Renderer>().material.color = Color.Lerp(cornerLines.GetComponent<Renderer>().material.color, lineColorActive, Time.deltaTime * 3f);
                } else if (fadeOut)
                {
                    cornerLines.GetComponent<Renderer>().material.color = Color.Lerp(cornerLines.GetComponent<Renderer>().material.color, new Color(0, 0, 0, 0), Time.deltaTime * 3f);
                }

            }
        } 
    }

    public void ShowCornerLines(bool isOn)
    {
        if (isOn)
        {
            if (outlineFade != null)
            {
                StopCoroutine(outlineFade);
            }
            StartCoroutine(OutlineFade(isOn, 0f));
        }
        else
        {
            if (outlineFade != null)
            {
                StopCoroutine(outlineFade);
            }
            StartCoroutine(OutlineFade(isOn, 0f));
        }
    }

    public void ChangeWorldScale(float scale)
    {
        if (scaleActions != null)
        {
            StopCoroutine(scaleActions);
        }
        scaleActions = StartCoroutine(ScaleActions(staticGrain));

        if (outlineFade != null)
        {
            StopCoroutine(outlineFade);
        }
        outlineFade = StartCoroutine(OutlineFade(false, 1f));
        displayCornerLines = true;
        lineFade = false;
        GetComponent<CallingManager>().cornerParent.SetActive(true);
        //displayCornerLines = true;
        grainParent.transform.localScale = new Vector3(parentScale.x * scale, parentScale.y * scale, parentScale.z * scale);
    }

    //switches static state of grains if not already static; activates displayCornerLines
    IEnumerator ScaleActions(bool isStatic)
    {
        //if its not static, make it static for movement
        if (!isStatic)
        {
            SwitchStaticState(true);
        }

        //after a small window, if the mode is not static, switch them back to physics
        yield return new WaitForSeconds(0.1f);
        if (!isStatic)
        {
            SwitchStaticState(false);
        }
    }

    IEnumerator OutlineFade(bool fadingIn, float delay)
    {
        if (fadingIn) //fade in
        {
            cornerLines.GetComponent<Renderer>().material.color = new Color(0, 0, 0, 0);
            GetComponent<CallingManager>().cornerParent.SetActive(true);

            fadeIn = true;
            fadeOut = false;
            lineFade = true;
            displayCornerLines = true;

            yield return new WaitForSeconds(1f);

            lineFade = false;

        }
        else //fade out
        {
            cornerLines.GetComponent<Renderer>().material.color = lineColorActive;
            GetComponent<CallingManager>().cornerParent.SetActive(true);

            yield return new WaitForSeconds(delay);

            fadeIn = false;
            fadeOut = true;
            lineFade = true;

            yield return new WaitForSeconds(1f);

            lineFade = false;
            displayCornerLines = false;
            GetComponent<CallingManager>().cornerParent.SetActive(false);
        }
    }

    public void HighlightGrains(bool highlighting)
    {

        List<GameObject> allGrains = GetComponent<CallingManager>().allGrains;

        if (highlighting)
        {
            for (int i = 0; i < allGrains.Count; i++)
            {
                allGrains[i].GetComponent<GrainRuntime>().Highlight(false, inactive);
            }
            //this is stupid, change to make more sense in the future
            List<GameObject> activeConstellation = GetComponent<ConstellationManager>().constellations[GetComponent<ConstellationManager>().selectedConst].SequencedGrains;
            for (int j = 0; j < activeConstellation.Count; j++)
            {
                activeConstellation[j].GetComponent<GrainRuntime>().Highlight(true, new Color(0,0,0));
            }

        } else
        {
            for (int i = 0; i < allGrains.Count; i++)
            {
                allGrains[i].GetComponent<GrainRuntime>().Highlight(true, new Color(0, 0, 0));
            }
        }

    }

    //Switches the static state of grains for different wand interactions
    public void SwitchStaticState(bool isStatic)
    {
        //switch between static and non static grain types
        if (isStatic)
        {
            //switches to static wand collision
            wandL.GetComponent<WandCollision>().staticGrain = true;
            wandR.GetComponent<WandCollision>().staticGrain = true;

            ModifySpringJoints(false);

            wandPlaybackL = new List<GameObject>();
            wandPlaybackR = new List<GameObject>();
        }
        else
        {
            //switches to non-static wand collision
            wandL.GetComponent<WandCollision>().staticGrain = false;
            wandR.GetComponent<WandCollision>().staticGrain = false;

            ModifySpringJoints(true);

            wandPlaybackL.Clear();
            wandPlaybackR.Clear();
        }
    }

    //Function disables and re-enables spring joints
    //useful for when moving all grains and joints need to be disabled
    void ModifySpringJoints(bool adding)
    {
        List<GameObject> allGrains = GetComponent<CallingManager>().allGrains;

        //Adds joints
        if (adding)
        {
            float springDamp = GetComponent<CallingManager>().springDamp;
            float springStrength = GetComponent<CallingManager>().springStrength;
            for (int i = 0; i < allGrains.Count; i++)
            {
                allGrains[i].isStatic = false;
                allGrains[i].AddComponent<SpringJoint>();
                allGrains[i].GetComponent<SpringJoint>().damper = springDamp;
                allGrains[i].GetComponent<SpringJoint>().spring = springStrength;
                allGrains[i].GetComponent<Rigidbody>().isKinematic = false;
            }
        }

        //Removes joints
        if(!adding)
        {
            for (int i = 0; i < allGrains.Count; i++)
            {
                Destroy(allGrains[i].GetComponent<SpringJoint>());
                allGrains[i].GetComponent<Rigidbody>().isKinematic = true;
                allGrains[i].isStatic = true;
            }
        }
    }
    //OVRScreenfade - makes screen fade in, use for when someone puts on headset
}
