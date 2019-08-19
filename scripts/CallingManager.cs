using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

public class CallingManager : MonoBehaviour
{
    //Gameobjects to initialize
    public GameObject grain;
    public GameObject OVRCamera;
    public GameObject WandR;
    public GameObject WandL;
    public GameObject grainParent;
    public GameObject corner;
    public GameObject cornerParent;

    public List<string> fLinesNew;

    //Grain Parameters
    public static int grainSize = 16384;
    public double playDuration;
    public float grainScalar = 0.1f;
    public Vector3 grainSpacing = new Vector3(10f, 10f, 10f);

    //Spring Joint Parameters
    public bool Springy;
    public float springDamp = 10.0f;
    public float springDist = 0.01f;
    public float springStrength = 500f;

    //Global Parameters
    public int totalGrains;
    string clipname;
    float[] windowArray;
    public List<GrainData> grainDataList;
    public List<GameObject> allGrains;
    Vector3[] corners;
    public GameObject[] cornerPins;

    //Extra features
    public bool Discovery_Mode = false;
    public bool exponentialScaling = true;
    public bool calc_maxbounds = true;
    public bool cosineWindowing = true;

    //Grain initial positioning
    public int Xaxis = 0;
    public int Yaxis = 1;
    public int Zaxis = 2;

    //Average play area calculation
    public Vector3 avg_location;
    public Vector3 max_bounds = new Vector3(30, 30, 30);
    public float max_boundsAvg = 50;

    //Filename and grainsize entry in inspector
    public static string filename = "LoFi"; //DrumPack2FS16
    [HideInInspector]
    public int arrayIdx = 0;
    [HideInInspector]
    public string[] GrainSizeSelection = new string[] { "2048", "4096", "8192", "16384", "32768" };

    //Algo
    public List<float> algoFeatures;

    //Principal Component Analysis
    //double[] features;

    void Start()
    {
        //grainSize
        
        if (Discovery_Mode)
        {
            Xaxis = Random.Range(0, 36);
            Yaxis = Random.Range(0, 36);
            Zaxis = Random.Range(0, 36);
        }

        GetComponent<UIManagerNew>().grainAxis = new Vector3Int(Xaxis, Yaxis, Zaxis);

        AudioClip activeAudioClip = Resources.Load<AudioClip>("Audio/" + filename);
        grain.GetComponent<AudioSource>().clip = activeAudioClip;

        //grainSize = int.Parse(GrainSizeSelection[arrayIdx]);
        playDuration = (double)grainSize / activeAudioClip.frequency;
        grain.GetComponent<GrainAudio>().playDuration = playDuration;
        WandR.GetComponent<WandCollision>().playDuration = (float)playDuration;
        WandL.GetComponent<WandCollision>().playDuration = (float)playDuration;


        //filename += "_" + GrainSizeSelection[arrayIdx];
        filename += "_" + grainSize;
        //Cosine windowing function ---WARNING--- resource intensive on startup!
        if (cosineWindowing)
            FillWindow(grainSize);

        grainDataList = new List<GrainData>();
        TextAsset file = Resources.Load("Lists/" + filename) as TextAsset;
        
        string fs = file.text;

        string[] fLines = Regex.Split(fs, "\n|\r|\r\n");
        
        //Remove empty lines
        for(int k = 0; k < fLines.Length; k++)
        {
            if (fLines[k].Length > 1)
                fLinesNew.Add(fLines[k]);
        }

        for (int j = 0; j < fLinesNew.Count; j++)
        {
            string valueLine = fLinesNew[j];
            var values = valueLine.Split(' ');
            GrainData gd = new GrainData();
            //originally had grain spacing here, but has since moved to instantiation stage
            gd.position = new Vector3(float.Parse(values[Xaxis]), float.Parse(values[Yaxis]), float.Parse(values[Zaxis]));
            gd.colorSet = new Vector4(float.Parse(values[3]), float.Parse(values[4]), float.Parse(values[5]));
            gd.scale = float.Parse(values[7]) * grainScalar;    

            //assign all grain attributes
            for(int l = 0; l < 36; l++)
            {
                gd.attributes[l] = float.Parse(values[l]);
                //features[l] = float.Parse(values[l]);
            }
            grainDataList.Add(gd);
        }

        int i = 0;
        float maxX = 0;
        float maxY = 0;
        float maxZ = 0;
        float minX = 0;
        float minY = 0;
        float minZ = 0;

        foreach (GrainData grainData in grainDataList)
        {
            GameObject grainInstance = Instantiate(grain, grainParent.transform, true);
            grainInstance.name = "grain" + i;
            grainInstance.GetComponent<GrainParametrization>().grain_attributes = grainData.attributes;
            grainInstance.transform.parent = grainParent.transform;
            //average location and max location
            grainInstance.transform.position = new Vector3(grainData.position.x * grainSpacing.x, grainData.position.y * grainSpacing.y, grainData.position.z * grainSpacing.z);
            grainInstance.GetComponent<GrainRuntime>().currentAttributes = new Vector3Int(Xaxis, Yaxis, Zaxis);
            grainInstance.GetComponent<GrainRuntime>().grainSpacing = grainSpacing;
            avg_location += grainData.position;

            if (exponentialScaling)
            {
                grainInstance.GetComponent<Transform>().localScale = new Vector3(Mathf.Sqrt(grainData.scale), Mathf.Sqrt(grainData.scale), Mathf.Sqrt(grainData.scale)); //sqrt could be put somewhere else to reduce load
            }
            else
            {
                grainInstance.GetComponent<Transform>().localScale = new Vector3(grainData.scale, grainData.scale, grainData.scale);
            }

            //set the grain's sample number and color
            grainInstance.GetComponent<GrainParametrization>().grainSize = grainSize;
            Renderer[] lods = grainInstance.GetComponentsInChildren<Renderer>();
            //Renderer[] lods = lod.renderers;
            for(int j = 0; j < lods.Length; j++)
            {
                lods[j].material.color = grainData.colorSet;
            }
            //grainInstance.GetComponent<GrainRuntime>().lods = lods;
            grainInstance.GetComponent<GrainParametrization>().sampNum = i * grainSize;
            grainInstance.GetComponent<GrainParametrization>().grainNum = i;
            grainInstance.GetComponent<GrainParametrization>().color = grainData.colorSet;
            //grainInstance.GetComponent<Renderer>().material.SetColor("_Color", grainData.colorSet);

            if (Springy == true)
            {
                grainInstance.AddComponent<SpringJoint>();
                grainInstance.GetComponent<SpringJoint>().damper = springDamp;
                //grainInstance.GetComponent<SpringJoint>().maxDistance = springDist;
                grainInstance.GetComponent<SpringJoint>().spring = springStrength;
            }

            if (calc_maxbounds)
            {
                //calculate max
                if (grainData.position.x > maxX)
                {
                    maxX = grainData.position.x;
                }
                if (grainData.position.y > maxY)
                {
                    maxY = grainData.position.y;
                }
                if (grainData.position.z > maxZ)
                {
                    maxZ = grainData.position.z;
                }

                //calculate min
                if (grainData.position.x < minX)
                {
                    minX = grainData.position.x;
                }
                if (grainData.position.y < minY)
                {
                    minY = grainData.position.y;
                }
                if (grainData.position.z < minZ)
                {
                    minZ = grainData.position.z;
                }
            }


            //add all grains to a master list
            allGrains.Add(grainInstance);

            i++;
        }
        avg_location = avg_location / i;
        OVRCamera.transform.position = avg_location;

        if (calc_maxbounds)
        {
            max_bounds = new Vector3(maxX, maxY, maxZ);
            max_boundsAvg = maxX + maxY + maxZ / 3f;
        }

        corners = new Vector3[12];
        corners[0] = new Vector3(minX, minY, minZ);
        corners[1] = new Vector3(maxX, minY, minZ);
        corners[2] = new Vector3(maxX, minY, maxZ);
        corners[3] = new Vector3(minX, minY, maxZ);
        corners[4] = new Vector3(minX, maxY, minZ);
        corners[5] = new Vector3(maxX, maxY, minZ);
        corners[6] = new Vector3(maxX, maxY, maxZ);
        corners[7] = new Vector3(minX, maxY, maxZ);

        cornerPins = new GameObject[8];

        for (int a = 0; a < 8; a++)
        {
            GameObject newCorner = Instantiate(corner, corners[a], Quaternion.identity);
            newCorner.transform.parent = cornerParent.transform;
            newCorner.name = "corner_" + a;
            cornerPins[a] = newCorner;
        }

        grain.SetActive(false);
        //WandR.GetComponent<WandControl>().rangeMultiplier = max_boundsAvg * 0.1f;
        //WandL.GetComponent<WandControl>().rangeMultiplier = max_boundsAvg * 0.1f;
        //grainParent.transform.localScale = new Vector3(initialScale, initialScale, initialScale);
        totalGrains = i;
    }

    void FillWindow(int n)
    {
        windowArray = new float[n];
        int k;
        for (k = 0; k < n; k++)
        {
            windowArray[k] = 1.0f - ((Mathf.Cos((k / (float)n) * 2.0f * (float)Mathf.PI) + 1.0f) * 0.5f);
        }
        return;
    }

    public void LoadScene(string file, int gs)
    {
        for(int b = 0; b < allGrains.Count; b++)
        {
            Destroy(allGrains[b]);
        }
        allGrains.Clear();
        grainDataList.Clear();

        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        filename = file;
        grainSize = gs;
    }

    /*
    void PCA(double[] inputFeatures)
    {
        alglib.pcabuildbasis(inputFeatures, )
        //alglib.pcatruncatedsubspace 
    }
    */

}
