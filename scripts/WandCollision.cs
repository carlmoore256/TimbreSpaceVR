using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WandCollision : MonoBehaviour
{
    //GameObject thisWand;
    OVRInput.Axis1D activeTrig;
    OVRInput.Button trigPress;
    OVRInput.Controller activeController;

    public VibrationManager vm;
    public ConstellationManager cm;
    public RuntimeManager rm;
    //public TextMeshPro constHUD;
    //public GameObject constHUD;
    public WandHUD HUD;
    //Coroutine HUDupdate;

    public float playDuration;
    bool isLeftController;
    public bool staticGrain;
    int grainCount;
    int controllerNum;


    private static bool YButton(bool status)
    {
        return status;
    }

    private void OnEnable()
    {
        GameObject thisWand = gameObject;
        if(thisWand.tag == "Wand_L")
        {
            activeTrig = OVRInput.Axis1D.PrimaryIndexTrigger;
            //added because rift controllers are acting strange
            trigPress = OVRInput.Button.PrimaryIndexTrigger;
            activeController = OVRInput.Controller.LTrackedRemote;
            isLeftController = true;
            controllerNum = 0;
        } else if(thisWand.tag == "Wand_R")
        {
            activeTrig = OVRInput.Axis1D.SecondaryIndexTrigger;
            //added because rift controllers are acting strange
            trigPress = OVRInput.Button.SecondaryIndexTrigger;
            activeController = OVRInput.Controller.RTrackedRemote;
            isLeftController = false;
            controllerNum = 1;
        }
    }


    private void OnCollisionEnter(Collision collision)
    {

        if (OVRInput.Get(trigPress) && collision.gameObject.tag == "grain")
        {
            collision.gameObject.GetComponent<GrainAudio>().Playback(OVRInput.Get(activeTrig) / Mathf.Pow(grainCount, 0.16f), 0, false);
            StartCoroutine(GrainCompression());
            //vm = new VibrationManager();
            vm.TriggerVibration(10, 4, (int)(OVRInput.Get(activeTrig) * 255), controllerNum);
        }

        if (staticGrain)
        {
            //If grains are static, add to a list of grains within the wand's radius
            if (isLeftController)
            {
                rm.wandPlaybackL.Add(collision.gameObject);
            }

            if(!isLeftController)
            {
                rm.wandPlaybackR.Add(collision.gameObject);
            }
        }

        /*
        //TEMP DEBUG
        if (Input.GetKey("x") && collision.gameObject.tag == "grain")
        {
            cm.AddGrains(collision.gameObject);
        }
        */  

        //Temporary constellation code
        if (isLeftController && OVRInput.Get(OVRInput.Button.Four) && collision.gameObject.tag == "grain")
        {
            cm.AddGrains(collision.gameObject);
            vm.TriggerVibration(10, 3, 63, controllerNum);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (staticGrain)
        {
            if (isLeftController)
            {
                rm.wandPlaybackL.Remove(collision.gameObject);
            }

            if(!isLeftController)
            {
                rm.wandPlaybackR.Remove(collision.gameObject);
            }
        }
    }


    IEnumerator GrainCompression()
    {
        grainCount++;
        yield return new WaitForSeconds(playDuration);
        grainCount--;
    }

}
