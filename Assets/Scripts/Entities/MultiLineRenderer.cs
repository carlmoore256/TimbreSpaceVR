using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct LineRendererOptions {
    public Color startColor;
    public Color endColor;
    public float startWidth;
    public float endWidth;

    public LineRendererOptions(Color startColor, Color endColor, float startWidth = 0.1f, float endWidth = 0.1f) {
        this.startColor = startColor;
        this.endColor = endColor;
        this.startWidth = startWidth;
        this.endWidth = endWidth;
    }

    public LineRendererOptions(Color color, float width=0.1f) {
        this.startColor = color;
        this.endColor = color;
        this.startWidth = width;
        this.endWidth = width;
    }

    public LineRendererOptions(float width=0.1f) {
        this.startColor = default(Color);
        this.endColor = default(Color);
        this.startWidth = width;
        this.endWidth = width;
    }
}

public class ManagedLineRenderer
{
    public GameObject gameObject;
    private LineRendererOptions options;
    private LineRenderer lineRenderer;

    public ManagedLineRenderer(LineRendererOptions options, GameObject gameObject) {
        this.options = options;
        this.gameObject = gameObject;
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.startColor = options.startColor;
        lineRenderer.endColor = options.endColor;
        lineRenderer.startWidth = options.startWidth;
        lineRenderer.endWidth = options.endWidth;
    }

    private void ApplyOptions() {
        lineRenderer.startColor = options.startColor;
        lineRenderer.endColor = options.endColor;
        lineRenderer.startWidth = options.startWidth;
        lineRenderer.endWidth = options.endWidth;
    }
}

public class MultiLineRenderer : MonoBehaviour
{
    // private List<GameObject> lineObjects = new List<GameObject>();
    private List<ManagedLineRenderer> lines = new List<ManagedLineRenderer>();

    public void AddLine(Vector3 start, Vector3 end, Color color) {
        GameObject lineObject = Instantiate(TsvrApplication.Config.worldSpaceLinePrefab, transform);
        LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        // lineObjects.Add(lineObject);
    }
}
