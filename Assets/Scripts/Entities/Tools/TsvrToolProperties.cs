using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITsvrToolAnimation {
    AnimationClip AnimationClip { get; set; }
    AnimationEvent AnimationEvent { get; set; }
    float AnimationSpeed { get; set; }
    AudioClip AnimationSound { get; set; }
}


public interface ITsvrToolProperties {
    Animation animation { get; set; }
}

[CreateAssetMenu(fileName = "TSVR/ToolProperties", menuName = "Tool Properties")]
public class TsvrToolProperties : MonoBehaviour {

    public List<Transform> _widgets = new List<Transform>();
    public Dictionary<string, Transform> Widgets { get; set; }
    public static Animation[] animations { get; set; }

    public void OnAwake() {
        Widgets = new Dictionary<string, Transform>();
        foreach (Transform widget in _widgets) {
            Widgets.Add(widget.name, widget);
            Debug.Log("Widget: " + widget.name + " added to dictionary.");
        }
    }
}