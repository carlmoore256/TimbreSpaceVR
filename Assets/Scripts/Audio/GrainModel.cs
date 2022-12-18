using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class GrainModel : MonoBehaviour
{

    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private float pos_speed = 15f;
    private float rot_speed = 35f;

    public int m_FrameSize = 4096;
    public int m_Hop = 512;

    // grain position feature representations
    public string x_Feature = "mfcc_0";
    public string y_Feature = "mfcc_1";
    public string z_Feature = "mfcc_2";
    public Vector3 axisScale = Vector3.one;
    // grain scale feature representations
    public string scale_Feature = "rms";
    public float scale_Scalar = 2f;
    // grain color feature representations
    public string r_Feature = "mfcc_3";
    public string g_Feature = "mfcc_4";
    public string b_Feature = "mfcc_5";
    public bool hsv_Feature = false;

    public float seqPlayRate = 1;

    private List<Grain> modelGrains;

    public GameObject grainPf;

    Thread T_procAudio = null;
    Thread T_micAudio = null;

    public bool updateFeatures;

    GrainFeatures[] m_AllGrainFeatures;

    private float opTime;

    private SpringJoint springJoint;

    public void Initialize(GameObject grainPf, Vector3 spawnPos, string audioPath=null)
    {
        this.grainPf = grainPf;
        transform.position = spawnPos;
        targetPosition = spawnPos;
        targetRotation = transform.rotation;
        modelGrains = new List<Grain>();

        if (audioPath != null)
            LoadGrains(audioPath, m_FrameSize, m_Hop);

        // StartCoroutine(PlayGrainsSequentially());
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(!Vector3.Equals(transform.position, targetPosition))
        {
            Debug.Log("Position not equal, moving grain model");
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * pos_speed);
        }

        if(!Quaternion.Equals(transform.rotation, targetRotation))
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * rot_speed);
        }

        if(updateFeatures)
        {
            Debug.Log("Updating Features!");
            updateFeatures = false;
            UpdateFeaturePositions(x_Feature, y_Feature, z_Feature, axisScale);
            UpdateFeatureScales(scale_Feature, scale_Scalar);
            UpdateFeatureColors(r_Feature, g_Feature, b_Feature, hsv_Feature);
        }
    }

    IEnumerator PlayGrainsSequentially()
    {
        while(true)
        {
            int listLen = modelGrains.Count;
            for (int i = 0; i < listLen; i++)
            {
                Grain grain = modelGrains[i];
                grain.PlayGrain();
                print($"playing grain {grain}");
                yield return new WaitForSeconds(seqPlayRate);
            }
            yield return new WaitForSeconds(seqPlayRate);
        }

    }

    // threaded audio processing and coroutines to spawn grains
    void LoadGrains(string audioPath, int frameSize, int hop)
    {
        // asynchronously load grains
        T_procAudio = new Thread(() => ProcessAudioFeatures(audioPath, frameSize, hop));
        T_procAudio.Start();

        StartCoroutine(SpawnGrainsAsync());
        //SpawnGrainsFromFeatures(GrainFeatures[] grainFeatures) // << make this a coroutine which waits for new grainFeatures
    }

    //// asynchronously spawn grains
    IEnumerator SpawnGrainsAsync()
    {
        while (m_AllGrainFeatures == null)
        {
            yield return new WaitForEndOfFrame();
        }

        opTime = Time.realtimeSinceStartup;

        int index = 0;
        foreach (GrainFeatures gf in m_AllGrainFeatures)
        {
            index++;
            if (gf == null)
                continue;
            SpawnGrain(gf);

            if (Time.realtimeSinceStartup > opTime + 60.0f)
            {
                yield return null;
                opTime = Time.realtimeSinceStartup;
            } else
            {
                yield return new WaitForSeconds(0.001f);
            }
        }
    }

    //moves grain model to a location, and rotates to look at a point
    public void MoveLookAt(Vector3 lookAtPos, Vector3 _targetPosition)
    {
        targetRotation = Quaternion.LookRotation(lookAtPos);
        targetPosition = _targetPosition;
    }

    void ProcessAudioFeatures(string audioPath, int frameSize, int hop)
    {
        AudioFeatures audioFeatures = new AudioFeatures();
        print($"Computing features for {audioPath}");
        m_AllGrainFeatures = audioFeatures.GenerateAudioFeatures(audioPath, frameSize, hop, 26);
        print($"Completed audio feature analysis for {audioPath}");
    }

    void SpawnGrain(GrainFeatures gf)
    {
        GameObject grainObject = Instantiate(grainPf, transform);
        //Grain grain = grainObject.AddComponent<Grain>();
        Grain grain = grainObject.GetComponent<Grain>();
        grain.Initialize(
            new Vector3(gf.featureDict[x_Feature], gf.featureDict[y_Feature], gf.featureDict[z_Feature]),
            new Vector3(gf.featureDict[scale_Feature], gf.featureDict[scale_Feature], gf.featureDict[scale_Feature]),
            gf,
            $"grain_{modelGrains.Count}"
        );

        modelGrains.Add(grain);
    }

    void StartAudioInput()
    {
        T_micAudio = new Thread(() => AudioInputThread());
        T_micAudio.Start();
    }

    void AudioInputThread()
    {
        AudioSource audioSource = GetComponent<AudioSource>();

        int sampleRate = AudioSettings.GetConfiguration().sampleRate;

        //float buffSize = 1 / (float)sampleRate * 256;

        audioSource.clip = Microphone.Start(null, true, 1, sampleRate);

        while(!(Microphone.GetPosition(null) > 0))
        {
            //float[] micSamples = audioSource.clip.GetData()
        }
    }

    // change the positions of all grains to new feature ordering
    void UpdateFeaturePositions(string x_F, string y_F, string z_F, Vector3 ax_scale)
    {
        foreach(Grain grain in modelGrains)
            grain.UpdatePosition(x_F, y_F, z_F, ax_scale);
    }

    void UpdateFeatureScales(string sc_F, float scale)
    {
        foreach (Grain grain in modelGrains)
            grain.UpdateScale(sc_F, sc_F, sc_F, scale);
    }

    void UpdateFeatureColors(string r_F, string g_F, string b_F, bool hsv=false)
    {
        foreach (Grain grain in modelGrains)
            grain.UpdateColor(r_F, g_F, b_F, hsv);
    }
}
