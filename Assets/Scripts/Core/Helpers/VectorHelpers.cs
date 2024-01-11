using UnityEngine;
using System.Collections.Generic;
using System;

public static class VectorHelpers {

    public static Vector3 Scale(this Vector3 vector, Vector3 scale) {
        return new Vector3(vector.x * scale.x, vector.y * scale.y, vector.z * scale.z);
    }

    public static Vector3 ReplaceNaN(this Vector3 vector, Vector3 replacement) {
        return new Vector3(
            float.IsNaN(vector.x) ? replacement.x : vector.x,
            float.IsNaN(vector.y) ? replacement.y : vector.y,
            float.IsNaN(vector.z) ? replacement.z : vector.z
        );
    }
    
    private static Vector3 BezierPoint(List<Vector3> controlPoints, float t)
    {
        if (controlPoints.Count == 1)
            return controlPoints[0];

        List<Vector3> newControlPoints = new List<Vector3>(controlPoints.Count - 1);
        for (int i = 0; i < controlPoints.Count - 1; i++)
        {
            newControlPoints.Add(Vector3.Lerp(controlPoints[i], controlPoints[i + 1], t));
        }

        return BezierPoint(newControlPoints, t);
    }

    public static List<Vector3> GenerateBezierPoints(IEnumerable<Vector3> controlPoints, float smoothness)
    {
        if (smoothness <= 0)
            throw new ArgumentException("Smoothness must be greater than 0.");

        List<Vector3> bezierPoints = new List<Vector3>();
        int segments = Mathf.Max(1, Mathf.FloorToInt(smoothness));
        float step = 1.0f / segments;

        var points = new List<Vector3>(controlPoints);

        if (points.Count == 2)
        {
            for (float t = 0; t <= 1; t += step)
            {
                Vector3 point = Vector3.Lerp(points[0], points[1], t);
                bezierPoints.Add(point);
            }
        }
        else if (points.Count > 2)
        {
            for (float t = 0; t <= 1; t += step)
            {
                bezierPoints.Add(BezierPoint(points, t));
            }
        }

        return bezierPoints;
    }
}