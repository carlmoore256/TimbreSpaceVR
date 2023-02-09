using UnityEngine;

public class LODRenderer {
    public LODGroup lodGroup;
    public Renderer[] renderers;
    private int lodCounter = 0;
    public LODRenderer(GameObject gameObject, Material material = null) {
        lodGroup = gameObject.GetComponent<LODGroup>();
        renderers = gameObject.GetComponentsInChildren<Renderer>();
        if (material != null) {
            SetMaterial(material);
        }
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
    public void ChangeColor(Color color, int index) {
        renderers[index].material.color = color;
    }
    // change color of a single LOD on a cycle
    public void ChangeColorCycle(Color color) {
        ChangeColor(color, lodCounter);
        lodCounter = (lodCounter + 1) % renderers.Length;
    }
    public Color GetColor() {
        return renderers[0].material.color;
    }

    public void SetMaterial(Material material) {
        foreach (Renderer renderer in renderers) {
            renderer.material = material;
        }
    }
}