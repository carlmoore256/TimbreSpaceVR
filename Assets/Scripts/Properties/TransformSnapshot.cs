using UnityEngine;

[System.Serializable]
public struct TransformSnapshot {
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public TransformSnapshot Copy() {
        Vector3 newPosition = new Vector3(position.x, position.y, position.z);
        Quaternion newRotation = new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);
        Vector3 newScale = new Vector3(scale.x, scale.y, scale.z);
        return new TransformSnapshot(newPosition, newRotation, newScale);
    }
    public TransformSnapshot(Vector3 position, Quaternion rotation, Vector3 scale) {
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
    }
}