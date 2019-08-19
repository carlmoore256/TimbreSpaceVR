using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class ConstellationManager : MonoBehaviour
{
    //public List<GameObject> activeConstellation;
    //List<GameObject>[] constellations;
    public List<Constellation> constellations;
    public GameObject lineRenderer;
    public GameObject lineParent;
    public WandHUD wandHUD;


    bool playingConst;
    bool toggle;
    public bool initConst;


    public double playbackInterval = 0.03f;
    double nextPlayTime;
    float resetPress;
    int sequencerIndex;
    int lineUpdateRate = 2;
    int lineUpdateCount;
    int constNum;
    int masterIndex;
    public int selectedConst;

    public Color lineColor;

    string saveData;

    public double bpm = 140.0F;
    public int signatureHi = 4;
    public int signatureLo = 4;
    private double nextTick = 0.0F;
    private double sampleRate = 0.0F;

    //public LineRenderer constLine;

    void Start()
    {
        //activeConstellation = new List<GameObject>();
        sequencerIndex = 0;
        lineUpdateCount = 0;
        nextPlayTime = 0;
        toggle = true;
        initConst = true;
        constellations = new List<Constellation>();
        //constellations = new List<GameObject>[4];
        constNum = 0;
        selectedConst = 0;

        double startTick = AudioSettings.dspTime;
        sampleRate = AudioSettings.outputSampleRate;
        nextTick = startTick * sampleRate;
    }


    void Update()
    {

        /*
        if (Input.GetKeyUp("z"))
            NewConstellation();

        if (Input.GetKeyUp("v"))
            SaveConstellation(0);
            */

        if (OVRInput.GetUp(OVRInput.RawButton.X) || Input.GetKeyDown("tab") && constellations[selectedConst].SequencedGrains.Count > 0)
        {
            resetPress = 0;

            if (toggle)
            {
                constellations[selectedConst].TogglePlay(true);
                toggle = false;
                wandHUD.UpdateHUD("Playing constellation " + (selectedConst + 1), constellations[selectedConst].SequencedGrains.Count + " total grains", constellations[selectedConst].timeSig + " time signature", 1);
            }
            else
            {
                constellations[selectedConst].TogglePlay(false);
                toggle = true;
            }
        }

        //Reset press to clear constellation
        if (OVRInput.Get(OVRInput.RawButton.X) && constellations[selectedConst].SequencedGrains.Count > 0)
        {
            resetPress += Time.deltaTime;
            if (resetPress > 0.6f)
            {
                constellations[selectedConst].SequencedGrains.Clear();
                constellations[selectedConst].constLine.positionCount = 0;
                constellations[selectedConst].TogglePlay(false);
                resetPress = 0;
                toggle = true;
                //vm.TriggerVibration(100, 5, 170, 0);
            }
        }
        if (constellations.Count > 0)
        {
            //sends play command to constellation class
            if (AudioSettings.dspTime >= nextPlayTime)
            {
                double samplesPerTick = sampleRate * 60.0F / bpm;
                nextTick += samplesPerTick;
                //double sample = AudioSettings.dspTime * sampleRate;
                nextPlayTime = AudioSettings.dspTime + playbackInterval;
                for (int i = 0; i < constellations.Count; i++)
                {
                    constellations[i].ConstellationUpdate(nextPlayTime, masterIndex);
                }
                masterIndex++;
            }
            //updates lines

            if (lineUpdateCount >= lineUpdateCount % lineUpdateRate)
            {
                for (int i = 0; i < constellations.Count; i++)
                {
                    if(constellations[i].SequencedGrains.Count > 0)
                        constellations[i].LineUpdate();

                }
            }
            lineUpdateCount++;

        }
    }

    //handles adding grains to constellation and updating hud
    public void AddGrains(GameObject newGrain)
    {
        if (initConst)
        {
            NewConstellation();
        }
        constellations[selectedConst].SequencedGrains.Add(newGrain);
        wandHUD.UpdateHUD(newGrain.name, constellations[selectedConst].SequencedGrains.Count.ToString(), "null", 0);
    }

    public void TimeSignature(int ts, int id)
    {
        int newTS = 32 - ts;
        constellations[id].timeSig = newTS;
    }

    public void NewConstellation()
    {
        var newConst = new Constellation();
        newConst.SequencedGrains = new List<GameObject>();
        GameObject newLine = Instantiate(lineRenderer, lineParent.transform, true);
        newConst.constLine = newLine.GetComponent<LineRenderer>();
        //newConst.lineColor = Random.ColorHSV(0f,1f);
        //Color newColor = Random.ColorHSV(0f, 1f);
        Color newColor = Color.HSVToRGB(Random.Range(0f, 1f), 1f, 1f);
        newConst.lineColor = newColor;
        newLine.GetComponent<LineRenderer>().material.color = newColor;
        newConst.constLine.startWidth = 0.005f;
        newConst.constLine.endWidth = 0.005f;
        newConst.timeSig = 8;
        constellations.Add(newConst);
        if (initConst)
        {
            initConst = false;
            selectedConst = 0;
            GetComponent<UIManagerNew>().AddPanel();
        }
        else
        {
            selectedConst++;
        }
    }

    void SaveConstellation(int index)
    {
        string constFile = "./Assets/Resources/Saves/consellation.txt";
        string constellationConents = SaveData(index);
        print("saving constellation");
        File.WriteAllText(constFile, constellationConents);
            /*
        using (var writer = new StreamWriter("Assets/Saves/" + constFile))
        {
            //TextWriter text
            File.Wr
            //StreamWriter.
        }
        */
    }

    //save information about scene, grain size, attribute data, and index in all grains
    private string SaveData(int index)
    {
        StringBuilder sb = new StringBuilder();
        int length = 0;

        for(int i = 0; i < constellations[index].SequencedGrains.Count; i++)
        {
            string line = constellations[index].SequencedGrains[i].GetComponent<GrainParametrization>().grainNum.ToString() + " ";

            sb.Insert(length, line);
            length += line.Length;
        }
        string header = "length_" + constellations[index].SequencedGrains.Count.ToString() + " ";
        sb.Insert(0, header);
        length += header.Length;

        //this will add attributes 
        /*
        for (int i = 0; i < constellations[index].SequencedGrains.Count; i++)
        {
            string line = constellations[index].SequencedGrains[i].GetComponent<GrainParametrization>().grainNum.ToString() + " ";

            sb.Insert(length, line);
            length += line.Length;
        }
        */

        //sb.ToString();
        return sb.ToString();
    }

    void LoadConstellation()
    {
        string loadConst = File.ReadAllText("./Assets/Resources/Saves/consellation.txt");
        //loadConst.
    }
}
