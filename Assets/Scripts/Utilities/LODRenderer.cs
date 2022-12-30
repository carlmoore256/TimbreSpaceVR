using UnityEngine;

public class LODRenderer {
    public LODGroup lodGroup;
    public Renderer[] renderers;
    public LODRenderer(GameObject gameObject) {
        lodGroup = gameObject.GetComponent<LODGroup>();
        renderers = gameObject.GetComponentsInChildren<Renderer>();
    }
    public void SetLOD(int lod) {
        if (lodGroup != null) {
            lodGroup.ForceLOD(lod);
        }
    }
    public void ChangeColor(Color color) {
        foreach (Renderer renderer in renderers) {
            renderer.material.color = color;
        }
    }
    public Color GetColor() {
        return renderers[0].material.color;
    }
}