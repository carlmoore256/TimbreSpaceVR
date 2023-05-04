using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InspectorButton))]
public class InspectorButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        InspectorButton button = (InspectorButton)target;

        if (GUILayout.Button("Press me"))
        {
            button.ButtonAction();
        }
    }
}
