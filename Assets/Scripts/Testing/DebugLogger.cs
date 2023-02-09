using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TSVR_DebugLogger", menuName = "TSVR/Debug Logger")]
public class DebugLogger : ScriptableObject
{
    public GameObject XRDebugConsole;
    private bool isEnabled = false;
    void OnEnable() {
        // isEnabled = TsvrApplication.Settings.EnableXRLogger;
        // Debug.Log("SETTING XRDebugConsole to " + isEnabled.ToString(), XRDebugConsole);
        // XRDebugConsole = GameObject.Find("XRDebugConsole");
        // if (isEnabled && XRDebugConsole != null) {
        //     if (XRDebugConsole != null) {
        //         XRDebugConsole.SetActive(true);
        //     } else {
        //         GameObject debug = Instantiate(TsvrApplication.Config.xrDebugConsolePrefab);
        //         debug.name = "XRDebugConsole";
        //         XRDebugConsole.SetActive(true);
        //     }
        // } else {
        //     if (XRDebugConsole != null) {
        //         XRDebugConsole.SetActive(false);
        //     }
        // }
    }

    public void Log(string message, string category="", GameObject context=null) {
        if (!isEnabled) return;

        if (category != "") {
            message = category + ": " + message;
        }

        Debug.Log(message, context);
    }
}
