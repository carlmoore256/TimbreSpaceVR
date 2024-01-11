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

    public static void ApplyToRenderer(LineRendererOptions options, LineRenderer lineRenderer) {
        lineRenderer.startColor = options.startColor;
        lineRenderer.endColor = options.endColor;
        lineRenderer.startWidth = options.startWidth;
        lineRenderer.endWidth = options.endWidth;
    }
}