using UnityEngine;

[System.Serializable]
public struct TransformSnapshot {
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;


    public TransformSnapshot(Vector3 position, Quaternion rotation, Vector3 scale) {
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }

    public TransformSnapshot(Transform transform) {
        Position = transform.position;
        Rotation = transform.rotation;
        Scale = transform.localScale;
    }

    public TransformSnapshot(GameObject gameObject) : this(gameObject.transform) { }

    public TransformSnapshot Copy() {
        Vector3 newPosition = new Vector3(Position.x, Position.y, Position.z);
        Quaternion newRotation = new Quaternion(Rotation.x, Rotation.y, Rotation.z, Rotation.w);
        Vector3 newScale = new Vector3(Scale.x, Scale.y, Scale.z);
        return new TransformSnapshot(newPosition, newRotation, newScale);
    }

}