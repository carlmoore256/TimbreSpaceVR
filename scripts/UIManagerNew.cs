using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using System.IO;



// Show off all the Debug UI components.
public class UIManagerNew : MonoBehaviour
{
    public BroadcastMove bm;
    public Vector3Int grainAxis;
    public GameObject panelPrefab;
    public GameObject canvas;

    bool inMenu;
    private Text[] sliderText = new Text[8];
    List<Text> constSliderText;
    string[] axisLabels;
    string selectedScene;
    int selectedGrainSize;

    int panelNum;
    float constVariance;

    //Add text array for the labels of the sliders for XYZ

    void Start ()
    {
        constVariance = 0.15f;
        selectedGrainSize = 8192;
        panelNum = 2;
        constSliderText = new List<Text>();
        TextAsset labels = Resources.Load("Data/AxisLabels") as TextAsset;
        string labelStr = labels.text;
        string[] axisLabelsLoad = System.Text.RegularExpressions.Regex.Split(labelStr, "\n|\r|\r\n");

        axisLabels = new string[37];
        int i = 0;
        //Remove empty lines
        foreach(string str in axisLabelsLoad)
        {
            if (str.Length > 1)
            {
                axisLabels[i] = str;
                i++;
            }
        }

        DebugUIBuilder.instance.AddLabel("Scene Controls");

        DebugUIBuilder.instance.AddDivider();
        var sliderPrefab = DebugUIBuilder.instance.AddSlider("X-Axis", 0, 36, SliderX, true, 0, 0);
        var textElementsInSlider = sliderPrefab.GetComponentsInChildren<Text>();
        //dumb way of labelling axis
        textElementsInSlider[0].text = "X-Axis";
        Assert.AreEqual(textElementsInSlider.Length, 2, "Slider prefab format requires 2 text components (label + value)");
        sliderText[0] = textElementsInSlider[1];
        Assert.IsNotNull(sliderText[0], "No text component on slider prefab");
        sliderText[0].text = "BFCC1";

        sliderPrefab = DebugUIBuilder.instance.AddSlider("Y-Axis", 0, 36, SliderY, true, 0, 1);
        textElementsInSlider = sliderPrefab.GetComponentsInChildren<Text>();
        textElementsInSlider[0].text = "Y-Axis";
        sliderText[1] = textElementsInSlider[1];
        sliderText[1].text = "BFCC2";

        sliderPrefab = DebugUIBuilder.instance.AddSlider("Z-Axis", 0, 36, SliderZ, true, 0, 2);
        textElementsInSlider = sliderPrefab.GetComponentsInChildren<Text>();
        textElementsInSlider[0].text = "Z-Axis";
        sliderText[2] = textElementsInSlider[1];
        sliderText[2].text = "BFCC3";

        DebugUIBuilder.instance.AddDivider();

        sliderPrefab = DebugUIBuilder.instance.AddSlider("Tempo", 30, 300, SliderGlobalConstSpd, true, 0, 120);
        textElementsInSlider = sliderPrefab.GetComponentsInChildren<Text>();
        textElementsInSlider[0].text = "Tempo";
        sliderText[3] = textElementsInSlider[1];
        sliderText[3].text = "120";

        DebugUIBuilder.instance.AddDivider();

        sliderPrefab = DebugUIBuilder.instance.AddSlider("Model Scale", 0.1f, 3f, ModelScale, false, 0, 1f);
        textElementsInSlider = sliderPrefab.GetComponentsInChildren<Text>();
        textElementsInSlider[0].text = "Model Scale";
        sliderText[4] = textElementsInSlider[1];
        sliderText[4].text = "1.0";

        //slider option for default init val is last 
        sliderPrefab = DebugUIBuilder.instance.AddSlider("Grain Scale", 0.1f, 3f, GrainScale, false, 0, 1f);
        textElementsInSlider = sliderPrefab.GetComponentsInChildren<Text>();
        textElementsInSlider[0].text = "Grain Scale";
        sliderText[5] = textElementsInSlider[1];
        sliderText[5].text = "1.0";

        DebugUIBuilder.instance.AddDivider();

        DebugUIBuilder.instance.AddToggle("Disable Physics", DisablePhys);
        DebugUIBuilder.instance.AddToggle("Display Info", ShowInfo);

        //make this go on const specific panels
        DebugUIBuilder.instance.AddToggle("Highlight Constellation", HighlightConst);

        DebugUIBuilder.instance.AddToggle("Show Bounds", ShowBounds);
        DebugUIBuilder.instance.AddButton("New Constellation", AddPanel, 0);
        DebugUIBuilder.instance.AddButton("Algo Constellation", AlgoSeq, 0);
        DebugUIBuilder.instance.AddDivider(0);
        DebugUIBuilder.instance.AddButton("Variant Constellation", ConstVariantClick, 0);
        sliderPrefab = DebugUIBuilder.instance.AddSlider("Variance", 0f, 1f, ConstVariant, false, 0, 0.1f);
        textElementsInSlider = sliderPrefab.GetComponentsInChildren<Text>();
        textElementsInSlider[0].text = "Variance";
        sliderText[6] = textElementsInSlider[1];
        sliderText[6].text = "0.1";
        DebugUIBuilder.instance.AddDivider(0);
        DebugUIBuilder.instance.AddButton("Open Scene...", OpenScene, 0);


        //Add all names of scenes and add them to the panel
        GetSceneNames();

        DebugUIBuilder.instance.Show();
        inMenu = true;
	}

    /// <summary>
    /// LOAD SCENE FUNCTIONS
    /// </summary>

    public List<string> filenames;

    //Get scene names from 
    void GetSceneNames()
    {
        //stupid way to do this, but this is the way for any resources.load bullshit
        AudioClip[] audioClips = Resources.LoadAll<AudioClip>("Audio/");
        for (int a = 0; a < audioClips.Length; a++)
        {
            filenames.Add(audioClips[a].name);
        }
    }

    //Grain size selector delegate
    public void GrainSizeSelector(float f)
    {
        int gs = 8192;
        switch (f)
        {
            case 0:
                gs = 2048;
                break;
            case 1:
                gs = 4096;
                break;
            case 2:
                gs = 8192;
                break;
            case 3:
                gs = 16384;
                break;
            case 4:
                gs = 32768;
                break;
        }
        sliderText[6].text = gs.ToString() + " samples";
        selectedGrainSize = gs;
    }

    void OpenScene()
    {
        DebugUIBuilder.instance.AddLabel("Load Scene", 1);
        for (int a = 0; a < filenames.Count; a++)
        {
            string addName = filenames[a];
            DebugUIBuilder.instance.AddRadio(addName, "group2", delegate (Toggle t) { SceneSelection(addName, "group2", t); }, 1);
        }
        //Add grain size selection slider
        var sliderPrefab = DebugUIBuilder.instance.AddSlider("Grain Size", 0, 4, GrainSizeSelector, true, 1, 2);
        var textElementsInSlider = sliderPrefab.GetComponentsInChildren<Text>();
        textElementsInSlider[0].text = "Grain Size";
        sliderText[6] = textElementsInSlider[1];
        sliderText[6].text = "8192 samples";
        DebugUIBuilder.instance.AddDivider(1);
        DebugUIBuilder.instance.AddButton("Load Scene", LoadScene, DebugUIBuilder.DEBUG_PANE_RIGHT);
        DebugUIBuilder.instance.Show();
    }

    //selects active scene to load
    public void SceneSelection(string radioLabel, string group, Toggle t)
    {
        selectedScene = radioLabel;
    }

    //Main scene loader
    void LoadScene()
    {
        GetComponent<CallingManager>().LoadScene(selectedScene, selectedGrainSize);
    }

    /// <summary>
    /// Scene Controls
    /// </summary>
    public void ConstVariant(float f)
    {
        sliderText[6].text = f.ToString();
        constVariance = f;
    }

    public void ConstVariantClick()
    {
        GetComponent<AlgoPlay>().VariantConstellation(constVariance);
    }

    public void DisablePhys(Toggle t)
    {
        GetComponent<RuntimeManager>().SwitchStaticState(t.isOn);
        GetComponent<RuntimeManager>().staticGrain = t.isOn;
    }

    public void HighlightConst(Toggle t)
    {
        GetComponent<RuntimeManager>().HighlightGrains(t.isOn);
    }

    public void ShowBounds(Toggle t)
    {

        GetComponent<RuntimeManager>().ShowCornerLines(t.isOn);
    }

    public void ShowInfo(Toggle t)
    {
        bm.BroadcastGrainInfo(t.isOn);
    }

    public void AlgoSeq()
    {
        GetComponent<AlgoPlay>().GenerateSequence(constVariance);
    }


    //HighlightGrains(bool highlighting)


    public void SliderX(float f)
    {
        sliderText[0].text = axisLabels[(int)f];
        grainAxis.x = (int)f;
        bm.BroadcastMovement(grainAxis);
    }

    public void SliderY(float f)
    {
        sliderText[1].text = axisLabels[(int)f];
        grainAxis.y = (int)f;
        bm.BroadcastMovement(grainAxis);
    }

    public void SliderZ(float f)
    {
        sliderText[2].text = axisLabels[(int)f];
        grainAxis.z = (int)f;
        bm.BroadcastMovement(grainAxis);
    }

    //make this slider update values of all sliders simultaneously, so they all have the same tempo
    public void SliderGlobalConstSpd(float f)
    {
        sliderText[3].text = f.ToString() + " BPM";
        //double bpm = 0.6 / f;
        GetComponent<ConstellationManager>().bpm = (double)f;
    }


    public void ModelScale (float f)
    {
        //f = Mathf.Pow(f / 1024 + 0.4f, 2);
        sliderText[4].text = f.ToString();
        GetComponent<RuntimeManager>().ChangeWorldScale(f);
    }

    public void GrainScale(float f)
    {
        //f = Mathf.Pow(f / 1024 + 0.4f, 2);
        sliderText[5].text = f.ToString();
        bm.BroadcastGrainScale(f);
    }

    //REMOVE
    void Update()
    {
        if(OVRInput.GetDown(OVRInput.Button.Start))
        {
            if (inMenu) DebugUIBuilder.instance.Hide();
            else DebugUIBuilder.instance.Show();
            inMenu = !inMenu;
        }


        if (Input.GetKeyDown("1"))
        {
            GetComponent<ConstellationManager>().NewConstellation();
        }


        if (Input.GetKeyDown("2"))
        {
            GetComponent<AlgoPlay>().GenerateSequence(0.15f);
        }


        if (Input.GetKeyDown("3"))
        {
            GetComponent<AlgoPlay>().VariantConstellation(0.15f);
        }
    }



    //adds new panels when new const is requested
    public void AddPanel()
    {
        int thisPanelNum = panelNum - 1;
        GameObject newPanel = Instantiate(panelPrefab, canvas.GetComponent<Transform>(), true);
        DebugUIBuilder.instance.AddLabel("Constellation " + thisPanelNum.ToString(), panelNum);

        //adds proper delegates for sliders

        switch (thisPanelNum)
        {

            case 1:
                var sliderPrefab0 = DebugUIBuilder.instance.AddSlider("constellation_1_spd", 1, 32, SliderConstSpd0, true, panelNum, 8);
                DebugUIBuilder.instance.AddButton("Select", SelectConst0, panelNum);
                var textElementsInSlider0 = sliderPrefab0.GetComponentsInChildren<Text>();
                textElementsInSlider0[0].text = "Tempo";
                constSliderText.Add(textElementsInSlider0[1]);
                constSliderText[0].text = "1/4";

                break;
            case 2:
                var sliderPrefab1 = DebugUIBuilder.instance.AddSlider("constellation_2_spd", 1, 32, SliderConstSpd1, true, panelNum, 8);
                DebugUIBuilder.instance.AddButton("Select", SelectConst1, panelNum);
                var textElementsInSlider1 = sliderPrefab1.GetComponentsInChildren<Text>();
                textElementsInSlider1[0].text = "Tempo";
                constSliderText.Add(textElementsInSlider1[1]);
                constSliderText[1].text = "1/4";
                GetComponent<ConstellationManager>().NewConstellation();
                break;
            case 3:
                var sliderPrefab2 = DebugUIBuilder.instance.AddSlider("constellation_3_spd", 1, 32, SliderConstSpd2, true, panelNum, 8);
                DebugUIBuilder.instance.AddButton("Select", SelectConst2, panelNum);
                var textElementsInSlider2 = sliderPrefab2.GetComponentsInChildren<Text>();
                textElementsInSlider2[0].text = "Tempo";
                constSliderText.Add(textElementsInSlider2[1]);
                constSliderText[2].text = "1/4";
                GetComponent<ConstellationManager>().NewConstellation();
                break;
            case 4:
                var sliderPrefab3 = DebugUIBuilder.instance.AddSlider("constellation_4_spd", 1, 32, SliderConstSpd3, true, panelNum, 8);
                DebugUIBuilder.instance.AddButton("Select", SelectConst3, panelNum);
                var textElementsInSlider3 = sliderPrefab3.GetComponentsInChildren<Text>();
                textElementsInSlider3[0].text = "Tempo";
                constSliderText.Add(textElementsInSlider3[1]);
                constSliderText[3].text = "1/4";
                GetComponent<ConstellationManager>().NewConstellation();
                break;
        }

        DebugUIBuilder.instance.Show();
        panelNum++;
    }

    void SelectConst0()
    {
        GetComponent<ConstellationManager>().selectedConst = 0;
        StartCoroutine(HighlightTimeout(0));
    }

    void SelectConst1()
    {
        GetComponent<ConstellationManager>().selectedConst = 1;
        StartCoroutine(HighlightTimeout(1));
    }

    void SelectConst2()
    {
        GetComponent<ConstellationManager>().selectedConst = 2;
        StartCoroutine(HighlightTimeout(2));
    }

    void SelectConst3()
    {
        GetComponent<ConstellationManager>().selectedConst = 3;
        StartCoroutine(HighlightTimeout(3));
    }

    IEnumerator HighlightTimeout(int id)
    {
        GetComponent<RuntimeManager>().HighlightGrains(true);

        //set color to transparent for visualization
        for(int i = 0; i < GetComponent<ConstellationManager>().constellations.Count; i++)
        {
            if(i != id)
            {
                GetComponent<ConstellationManager>().constellations[i].constLine.material.color = new Color(0.5f, 0.5f, 0.5f, 0.1f);
            }
        }
        yield return new WaitForSeconds(1f);
        GetComponent<RuntimeManager>().HighlightGrains(false);

        //reset color
        for (int i = 0; i < GetComponent<ConstellationManager>().constellations.Count; i++)
        {
            if (i != id)
            {
                GetComponent<ConstellationManager>().constellations[i].constLine.material.color = GetComponent<ConstellationManager>().constellations[i].lineColor;
            }
        }
        yield return null;
    }

    public void SliderConstSpd0(float f)
    {
        constSliderText[0].text = "1/" + f.ToString();
        GetComponent<ConstellationManager>().TimeSignature((int)f, 0);
    }

    public void SliderConstSpd1(float f)
    {
        constSliderText[1].text = "1/" + f.ToString();
        GetComponent<ConstellationManager>().TimeSignature((int)f, 1);
    }

    public void SliderConstSpd2(float f)
    {
        constSliderText[2].text = "1/" + f.ToString();
        GetComponent<ConstellationManager>().TimeSignature((int)f, 2);
    }

    public void SliderConstSpd3(float f)
    {
        constSliderText[3].text = "1/" + f.ToString();
        GetComponent<ConstellationManager>().TimeSignature((int)f, 3);
    }

    /*
    public void SliderConstVol0(float f)
    {
        constSliderText[0].text = f.ToString();
        GetComponent<ConstellationManager>().TimeSignature((int)f, 0);
    }

    public void SliderConstVol1(float f)
    {
        constSliderText[1].text = f.ToString();
        GetComponent<ConstellationManager>().TimeSignature((int)f, 1);
    }

    public void SliderConstVol2(float f)
    {
        constSliderText[2].text = f.ToString();
        GetComponent<ConstellationManager>().TimeSignature((int)f, 2);
    }

    public void SliderConstVol3(float f)
    {
        constSliderText[3].text = f.ToString();
        GetComponent<ConstellationManager>().TimeSignature((int)f, 3);
    }
    */
}
