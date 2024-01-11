using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class ManagedStaticLineRenderer
{
    public GameObject gameObject;
    private LineRendererOptions options;
    private LineRenderer lineRenderer;

    private Dictionary<int, LineSegment> lineSegments;
    // private List<Vector3> linePoints;
    private int lineCounter;

    public ManagedStaticLineRenderer(LineRendererOptions options, string id = null) {
        this.options = options;
        string objectName = "WorldSpaceLineRenderer";
        if (id != null) {
            objectName += "_" + id;
        }
        var _gameObject = GameObject.Find(objectName);
        if (_gameObject == null) {
            _gameObject = GameObject.Instantiate(TsvrApplication.Config.worldSpaceLinePrefab);
            _gameObject.name = objectName;
        }
        this.gameObject = _gameObject;
        lineRenderer = this.gameObject.GetComponent<LineRenderer>();
        // linePoints = new List<Vector3>();
        lineSegments = new Dictionary<int, LineSegment>();
        lineCounter = 0;
        ApplyOptions(options);
    }

    private void ApplyOptions(LineRendererOptions options) {
        LineRendererOptions.ApplyToRenderer(options, lineRenderer);
    }

    
    public void CreateLine(List<Vector3> points, Color color, float width)
    {
        for (int i = 0; i < points.Count - 2; i++)
        {
            LineSegment segment = new LineSegment
            {
                Start = points[i],
                End = points[i+1],
                Index = lineCounter
            };

            lineSegments.Add(segment.Index, segment);
            lineRenderer.positionCount += 1;
            lineCounter++;
        }

        UpdatePositions();
    }

    public void RemoveLine(int index) {
        lineSegments.Remove(index);
        UpdatePositions();
    }


    public void ClearLines() {
        lineRenderer.positionCount = 0;
        lineSegments.Clear();
    }

    private void UpdatePositions() {
        // sort lineSegments by Index
        var sortedLineSegments = lineSegments.Values.OrderBy(segment => segment.Index);

        Vector3[] positions = new Vector3[lineSegments.Count];
        for (int i = 0; i < sortedLineSegments.Count() - 1; i++) {
            positions[i] = lineSegments[i].Start;
        }
        positions[lineSegments.Count - 1] = lineSegments[lineSegments.Count - 1].End;
        lineRenderer.positionCount = lineSegments.Count;
        lineRenderer.SetPositions(positions);
    }

    private struct LineSegment
    {
        public Vector3 Start;
        public Vector3 End;
        public int Index;
    }
}

