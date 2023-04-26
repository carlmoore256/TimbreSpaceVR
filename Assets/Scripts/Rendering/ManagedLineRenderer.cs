using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ManagedLineRenderer
{
    public GameObject gameObject;
    private LineRendererOptions options;
    private LineRenderer lineRenderer;
    private const string objectName = "WorldSpaceLineRenderer";

    private bool _enabled;
    public bool Enabled {
        get => _enabled;
        set => SetEnabled(value);
    }

    public ManagedLineRenderer(LineRendererOptions options, string id = null) {
        this.options = options;
        // if (id != null) {
        //     objectName += "_" + id;
        // }
        // var _gameObject = GameObject.Find(objectName);
        // if (_gameObject == null) {
        //     _gameObject = GameObject.Instantiate(TsvrApplication.Config.worldSpaceLinePrefab);
        //     _gameObject.name = objectName;
        // }
        
        this.gameObject = ObjectHelpers.FindOrCreate(TsvrApplication.Config.worldSpaceLinePrefab, objectName, id);
        lineRenderer = this.gameObject.GetComponent<LineRenderer>();
        ApplyOptions(options);
        SetEnabled(false);
    }

    private void ApplyOptions(LineRendererOptions options) {
        LineRendererOptions.ApplyToRenderer(options, lineRenderer);
    }

    
    public void SetPositions(List<Vector3> points)
    {
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }

    public void Update() {
        // update the appearance
        ApplyOptions(options);
    }

    private void SetEnabled(bool enabled) {
        gameObject.SetActive(enabled);
        _enabled = enabled;
    }


}




// public class MultiLineRenderer : MonoBehaviour
// {
//     // private List<GameObject> lineObjects = new List<GameObject>();
//     private List<ManagedLineRenderer> lines = new List<ManagedLineRenderer>();

//     public void AddLine(Vector3 start, Vector3 end, Color color) {
//         GameObject lineObject = Instantiate(TsvrApplication.Config.worldSpaceLinePrefab, transform);
//         LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
//         lineRenderer.SetPosition(0, start);
//         lineRenderer.SetPosition(1, end);
//         lineRenderer.startColor = color;
//         lineRenderer.endColor = color;
//         // lineObjects.Add(lineObject);
//     }
// }
