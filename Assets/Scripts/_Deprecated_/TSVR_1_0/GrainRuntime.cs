using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using TMPro;
#if false
public class GrainRuntime : MonoBehaviour
{
    Color originalColor;
    Vector3 originalScale;
    //Renderer rend;
    public Renderer[] lods;
    public GameObject lodParent;
    public bool grainActivated;
    public bool pinataMode = false;

    bool grainMoving;
    public float playDuration;
    public Vector3Int currentAttributes;
    public Vector3 grainSpacing;
    Vector3 newPos;
    Vector3 scaleLerp = new Vector3(1.3f, 1.3f, 1.3f);
    int counter;
    int lodCount;

    Coroutine scaleLock;
    string grainInfoText;
    public GameObject infoPanel;

    void Start()
    {
        infoPanel = gameObject.transform.GetChild(0).gameObject;
        infoPanel.SetActive(false);
        lods = lodParent.GetComponentsInChildren<Renderer>();
        lodCount = lods.Length;
        
        originalColor = GetComponent<GrainParametrization>().color;
        originalScale = GetComponent<Transform>().localScale;
        grainActivated = false;
    }

    public void ActivateGrain()
    {
        for (int i = 0; i < lodCount; i++)
        {
            lods[i].material.color = Color.red;
        }
        transform.localScale = Vector3.Scale(originalScale, scaleLerp);
        grainActivated = true;
    }
    

    void Update()
    {
        if (grainActivated)
        {
            lods[counter].material.color = Color.Lerp(lods[counter].material.color, originalColor, Time.deltaTime * 4f);
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * 2f);
            if(transform.localScale == originalScale)
                grainActivated = false;

            counter++;
            if (counter > lodCount  -1)
                counter = 0;
        }


        if (grainMoving)
        {

            transform.localPosition = Vector3.Lerp(transform.localPosition, newPos, Time.deltaTime * 5f);
            if(transform.localPosition == newPos)
            {
                grainMoving = false;
                gameObject.AddComponent<SpringJoint>();
                GetComponent<SpringJoint>().connectedBody = GetComponent<Rigidbody>();
            }
        }
    }

    public void GrainScale(float scale)
    {
        transform.localScale = new Vector3(scale * originalScale.x, scale * originalScale.y, scale * originalScale.z);
        if(scaleLock != null)
        {
            StopCoroutine(scaleLock);
        }
        scaleLock = StartCoroutine(ScaleLock());
    }

    IEnumerator ScaleLock()
    {
        yield return new WaitForSeconds(0.1f);
        originalScale = transform.localScale;
    }

    public void Highlight(bool highlighted, Color inactive)
    {
        if (highlighted)
        {
            for(int i = 0; i < lods.Length; i++)
            {
                lods[i].material.color = originalColor;
            }
            
        } else
        {
            for (int i = 0; i < lods.Length; i++)
            {
                lods[i].material.color = inactive;
            }
        }
    }

    void RepositionGrain(Vector3Int newAttr)
    {
        float[] attributes = GetComponent<GrainParametrization>().grain_attributes;
        newPos = Vector3.Scale(grainSpacing, new Vector3(attributes[newAttr.x], attributes[newAttr.y], attributes[newAttr.z]));
        
        Destroy(GetComponent<SpringJoint>());
        grainMoving = true;
        //add cool lines showing each grain movement

    }

    //Grain info


    //bool initInfo = false;
    public void ShowGrainInfo(bool isOn)
    {
        //if(!initInfo)

        if (isOn)
        {
            //infoPanel.GetComponent<TextMeshPro>().text = grainInfoText;
            infoPanel.GetComponent<TextMeshPro>().text = gameObject.name;
            infoPanel.SetActive(true);
        }
        else
        {
            infoPanel.SetActive(false);
        }

    }

    private string GenerateInfo(float[] attributes)
    {
        /*
        StringBuilder sb = new StringBuilder();
        var grainAttr = GetComponent<GrainParametrization>().grain_attributes;
        foreach (float currentAttribute in grainAttr)
        {
            sb.IsNumber
            sb.AppendLine(currentAttribute.ToString().Substring(0, 4));
        }

        grainInfoText = sb.ToString();
        */      
        return grainInfoText;
           
    }
}
 #endif