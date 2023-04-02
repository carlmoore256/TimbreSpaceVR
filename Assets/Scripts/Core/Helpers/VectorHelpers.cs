using UnityEngine;

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
}