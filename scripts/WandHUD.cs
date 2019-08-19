using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WandHUD : MonoBehaviour
{
    public Transform controller;
    public GameObject centerEye;
    public Vector3 offset;
    TextMeshPro HUDtmp;
    Coroutine HUD_delay;
    bool HUDactive;

    private void Start()
    {
        HUDtmp = GetComponent<TextMeshPro>();
        HUDactive = false;
    }

    private void Update()
    {
        if (HUDactive)
        {
            transform.position = Vector3.Lerp(transform.position, controller.transform.position + offset, Time.deltaTime * 20f);
            transform.LookAt(2 * transform.position - centerEye.transform.position);
        }
    }

    //
    public void UpdateHUD(string text0, string text1, string text2, int id)
    {
        switch (id)
        {
            case 0:
                HUDtmp.text = "Added " + text0 + "\n\n" + "Total Grains: " + text1;
                break;
            case 1:
                HUDtmp.text = text0 + "\n\n" + text1 + "\n\n" + text2;
                break;
        }

        GetComponent<MeshRenderer>().enabled = true;
        HUDactive = true;
        if (HUD_delay != null)
            StopCoroutine(HUD_delay);
        HUD_delay = StartCoroutine(HUD_Delay());
    }

    IEnumerator HUD_Delay()
    {
        yield return new WaitForSeconds(2f);
        GetComponent<MeshRenderer>().enabled = false;
        HUDactive = false;
    }

}
