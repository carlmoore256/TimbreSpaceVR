using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 #if false
public class FXManager : MonoBehaviour
{
    public bool FX_enabled = false;
    float FX_rotationX;
    float FX_rotationY;
    float FX_rotationZ;
    public AudioLowPassFilter lowPass;
    public AudioEchoFilter echo;
    public AudioReverbFilter verb;
    public AudioChorusFilter chorus;
    //AudioSourceCurveType
    int fx_select = 0;
    public GameObject wand_L;
    public GameObject wandLine_L;
    public WandHUD wandHUD;
    float verbDefaultDecay;
    float verbDefaultGain;

    float qLP;
    float mixDly;
    float mixVerb;
    float mixChorus;
    // Start is called before the first frame update
    void Start()
    {
        double test = alglib.math.randomreal();
        print(test);
        lowPass.enabled = false;
        echo.enabled = false;
        chorus.enabled = false;
        //verb.enabled = false;
        fx_select = 0;

        verbDefaultDecay = verb.decayTime;
        verbDefaultGain = verb.reverbLevel;

        qLP = 1;
        mixDly = 1;
        mixVerb = 1;
        mixChorus = 0.5f;
    }

    void Update()
    {
        if (OVRInput.Get(OVRInput.Button.Two) || Input.GetKey("space"))
        {
            FxRun();

        }
        else
        {
            //FX_enabled = false;
        }
        if (OVRInput.GetUp(OVRInput.Button.Two))
        {
            FxOff();
        }
    }

    void FxRun()
    {
        //FX_enabled = true;
        wand_L.GetComponent<SphereCollider>().enabled = false;
        wand_L.GetComponent<MeshRenderer>().enabled = false;
        wandLine_L.SetActive(false);

        if (OVRInput.GetDown(OVRInput.Button.Four))
        {
            fx_select++;
        }

        //GET OPPOSITE CONTROLLER
        FX_rotationX = (OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch).eulerAngles.x + 180f) % 360;
        FX_rotationY = (OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch).eulerAngles.y + 180f) % 360;
        FX_rotationZ = (OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch).eulerAngles.z + 180f) % 360;

        float thumbstick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;

        //LOW PASS
        if ((fx_select % 4) == 0)
        {
            echo.enabled = false;
            lowPass.enabled = true;
            chorus.enabled = false;
            float currentLPF = lowPass.cutoffFrequency;
            float newLPF = Mathf.Exp((1 - (FX_rotationZ / 360)) * 12) + 80;
            float LPF = Mathf.Lerp(currentLPF, newLPF, Time.deltaTime * 10f);
            if(thumbstick > 0 || thumbstick < 0)
                qLP = ((thumbstick + 1) / 2f) * 3;
            lowPass.lowpassResonanceQ = qLP;
            lowPass.cutoffFrequency = LPF;
            wandHUD.UpdateHUD("FX: Low-Pass", "Cutoff Freq: " + LPF.ToString() + " Hz", "Q: " + qLP.ToString(), 1);
        }
        //ECHO
        if ((fx_select % 4) == 1)
        {
            echo.enabled = true;
            lowPass.enabled = false;
            chorus.enabled = false;
            float delay = (360 - FX_rotationZ) / 360f * 300f;
            echo.delay = delay;
            float decayRatio = FX_rotationX / 360f;
            echo.decayRatio = FX_rotationX / 360f;
            if (thumbstick > 0 || thumbstick < 0)
                mixDly = (thumbstick + 1) / 2f;
            echo.wetMix = mixDly;
            echo.dryMix = 1 - mixDly;
            wandHUD.UpdateHUD("FX: Delay", "time: " + delay + " ms | feedback: " + decayRatio, "Mix: " + mixDly.ToString(), 1);
        }
        //REVERB
        if ((fx_select % 4) == 2)
        {
            echo.enabled = false;
            lowPass.enabled = false;
            chorus.enabled = false;
            verb.decayTime = (1 - FX_rotationZ / 360) * 10f + 3;
            verb.reverbLevel = (OVRInput.Get(OVRInput.RawAxis1D.LHandTrigger) * 400f) - 300;
            if (thumbstick > 0 || thumbstick < 0)
                mixVerb = 0 - (((thumbstick + 1) / 2f) * 1000);
            verb.dryLevel = mixVerb;
            wandHUD.UpdateHUD("FX: Reverb", "decay time: " + verb.decayTime + " | level: " + verb.reverbLevel, "Mix: " + mixVerb.ToString(), 1);
        }
        //CHORUS
        if ((fx_select % 4) == 3)
        {
            echo.enabled = false;
            lowPass.enabled = false;
            chorus.enabled = true;
            verb.decayTime = (1 - FX_rotationZ / 360) * 10f + 3;
            verb.reverbLevel = (OVRInput.Get(OVRInput.RawAxis1D.LHandTrigger) * 400f) - 300;
            if (thumbstick > 0 || thumbstick < 0)
                mixChorus = (thumbstick + 1) / 2f;
            chorus.wetMix1 = mixChorus;
            chorus.wetMix2 = mixChorus;
            chorus.wetMix3 = mixChorus;
            chorus.dryMix = 1 - mixChorus;
            wandHUD.UpdateHUD("FX: Chorus", "decay time: " + verb.decayTime + " | level: " + verb.reverbLevel, "Mix: " + mixChorus.ToString(), 1);
        }
        //Or I could do combinations of L trigger and movement
    }

    void FxOff()
    {
        echo.enabled = false;
        lowPass.enabled = false;
        verb.decayTime = verbDefaultDecay;
        verb.reverbLevel = verbDefaultGain;
        wand_L.GetComponent<SphereCollider>().enabled = true;
        wand_L.GetComponent<MeshRenderer>().enabled = true;
        wandLine_L.SetActive(true);
    }
}
 #endif