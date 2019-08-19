using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using TMPro;
using System.Text;

public class GrainParametrization : MonoBehaviour
{
    public LODGroup lodGroup;
    //basic info necessary for grain
    public int sampNum;
    public int grainNum;
    public int grainSize;

    //List containing all attributes
    public float[] grain_attributes = new float[36];

    public Color color;
    //GameObject infoPanel;
    string grainInfoText;

    private void Start()
    {
        //infoPanel = gameObject.transform.GetChild(0).gameObject;
        //infoPanel.SetActive(false);
        //GenerateInfo(grain_attributes);
    }

    private string GenerateInfo(float[] attributes)
    {
        var sb = new StringBuilder();
        foreach(float currentAttribute in grain_attributes)
        {
            sb.AppendLine(currentAttribute.ToString().Substring(0, 4));
        }

        grainInfoText = sb.ToString();
        return grainInfoText;
    }

    /*
    void GrainInfo(bool isActive)
    {
        if (isActive)
        {
            //infoPanel.GetComponent<TextMeshPro>().text = grainInfoText;
            infoPanel.GetComponent<TextMesh>().text = grainNum.ToString();
            infoPanel.SetActive(isActive);
        }
        
    }
    */  

}
