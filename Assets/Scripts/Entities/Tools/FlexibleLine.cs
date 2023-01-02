using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TSVR/FlexibleLine", menuName = "Flexible Line")]
public class FlexibleLine : ScriptableObject
{
    private GameObject lineObject;
    
    private LineRenderer _lineRenderer;
    // public LineRenderer lineRenderer { get => {
    //     if (_lineRenderer == null) {
    //         _lineRenderer = GetComponent<LineRenderer>();
    //     }
    //     return _lineRenderer;
    // } }
    
    public LineRenderer LineRenderer { get { return _lineRenderer; } }

    // private int NumSegments { get; set {LineRenderer.positionCount}; };
    private int numSegments;
 
    [SerializeField]
    private float lineWidthStart;
   [SerializeField]
    private float lineWidthEnd;

    [SerializeField]
    private Material lineMaterial;


    void OnEnable() {
        lineObject = new GameObject("Wand_Line");
        _lineRenderer = lineObject.AddComponent<LineRenderer>();
        var numSegments = TsvrApplication.Settings.WandLineSegments;
        _lineRenderer.positionCount = numSegments;
        _lineRenderer.startWidth = lineWidthStart;
        _lineRenderer.endWidth = lineWidthEnd;
        _lineRenderer.material = lineMaterial;
        Debug.Log("CALLING ON ENABLE FOR FLEXIBLE LINE");
    }

    void OnDisable() {
        Destroy(lineObject);
    }

    
    void Update() {
        if (LineRenderer == null) return;

    }

    

    private Vector3[] GetFlexLinePositions(Vector3 startCurrent, Vector3 startTarget, Vector3 endCurrent, Vector3 endTarget) {
        Vector3[] positions = new Vector3[numSegments];
        for (int i = 0; i < numSegments; i++) {
            float t = (float)i / (float)(numSegments - 1);
            Vector3 start = Vector3.Lerp(startCurrent, startTarget, 1-t);
            Vector3 end = Vector3.Lerp(endCurrent, endTarget, t);
            positions[i] = Vector3.Lerp(start, end, t);
        }
        return positions;
    }
}
