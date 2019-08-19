using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// if isPlaying = true, call to play in constellation manager will call this class. Line renderer will also go through and render each active constellation
/// </summary>

public class Constellation : MonoBehaviour
{
    public List<GameObject> SequencedGrains;
    public LineRenderer constLine;
    public Color lineColor;
    Coroutine lineUpdate;

    //public int masterIndex;
    public int sequencerIndex;
    public int timeSig = 1;

    public bool isPlaying;

    //Toggles constellation on/off, and changes line colors
    public void TogglePlay(bool status)
    {
        if(SequencedGrains.Count > 0)
        {
            isPlaying = status;
            if (status)
            {
                sequencerIndex = 0;
                constLine.material.color = lineColor;
                LineUpdate();
            }
            else
            {
                constLine.material.color = new Color(0.5f, 0.5f, 0.5f, 0.1f);
                LineUpdate();
            }
        }

    }

    public void ConstellationUpdate(double playTime, int masterIndex)
    {
        //if playing, and time signature matches 0, play grain
        if (isPlaying && masterIndex % timeSig == 0)
        {
            GameObject grainPlaying = SequencedGrains[sequencerIndex % SequencedGrains.Count];
            grainPlaying.GetComponent<GrainAudio>().Playback(0.8f, playTime, true);
            sequencerIndex++;

        }
        masterIndex++;
    }

    public void LineUpdate()
    {
        if(SequencedGrains.Count > 0)
        {
            int vertexNum = SequencedGrains.Count;
            constLine.positionCount = vertexNum;
            for (int i = 0; i < vertexNum; i++)
            {
                Vector3 grainPos = SequencedGrains[i].GetComponent<Transform>().position;
                constLine.SetPosition(i, grainPos);
            }
        }
    }
}
