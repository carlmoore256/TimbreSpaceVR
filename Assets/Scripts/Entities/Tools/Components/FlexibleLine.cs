using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct FlexibleLineSettings {
    public float elasticity;
    public int numSegments;
    public bool isElastic;
    public float startWidth;
    public float endWidth;
    public Material material;

    public void LoadSettings() {
        elasticity = TsvrApplication.Settings.WandLineElasticity;
        numSegments = TsvrApplication.Settings.WandLineSegments;
        isElastic = TsvrApplication.Settings.EnableElasticWand;
    }

    public void ApplyToRenderer(LineRenderer lineRenderer) {
        lineRenderer.positionCount = numSegments;
        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = endWidth;
        lineRenderer.material = material;
    }
}

// [CreateAssetMenu(fileName = "TSVR/FlexibleLine", menuName = "Flexible Line")]
public class FlexibleLine : MonoBehaviour
{
    public FlexibleLineSettings settings = new FlexibleLineSettings();
    private GameObject lineObject;
    private LineRenderer lineRenderer;

    private Transform baseCurrent;
    private Transform endCurrent;
    private Transform endTarget;
    private Vector3[] linePositions;

    public void Initialize(Transform baseCurrent, Transform endCurrent, Transform endTarget) {
        this.baseCurrent = baseCurrent;
        this.endCurrent = endCurrent;
        this.endTarget = endTarget;
    }

    public void SetEndWidth(float size) {
        settings.endWidth = size;
        settings.ApplyToRenderer(lineRenderer);
    }

    void OnEnable() {
        if (baseCurrent == null || endCurrent == null || endTarget == null) {
            Debug.LogError("FlexibleLine: baseCurrent, endAnchor, and endAnchorTarget must be set in inspector");
            return;
        }
        settings.LoadSettings();
        lineObject = Instantiate(TsvrApplication.Config.flexibleLinePrefab);
        lineRenderer = lineObject.GetComponent<LineRenderer>();
        settings.ApplyToRenderer(lineRenderer);

        linePositions = new Vector3[settings.numSegments];
        lineRenderer.SetPositions(CalculateLinePositions());

        ToggleFlex(settings.isElastic);
    }

    void OnDisable() {
        Destroy(lineObject);
    }
   
    void Update() {
        if (lineRenderer == null) return;
        lineRenderer.SetPositions(CalculateLinePositions());
    }

    void ToggleFlex(bool isElastic) {
        if (isElastic) {
            lineObject.transform.parent = null;
            endCurrent.GetComponent<Rigidbody>().isKinematic = false;
            lineRenderer.useWorldSpace = false;
        } else {
            lineObject.transform.parent = transform;
            Destroy(endCurrent.GetComponent<SpringJoint>()); // <= [WARNING] this will cause null reference exception on change
            endCurrent.GetComponent<Rigidbody>().isKinematic = true;
            lineRenderer.useWorldSpace = true;
        }
    }
    
    private Vector3[] CalculateLinePositions() {
        for (int i = 0; i < settings.numSegments; i++) {
            float t = (float)i / (float)(settings.numSegments);
            Vector3 tipTargetPosition = endCurrent.position;
            if (settings.isElastic) // <= give the wand an elastic feel
                tipTargetPosition = Vector3.Lerp(tipTargetPosition, endTarget.position, t * settings.elasticity);
            linePositions[settings.numSegments - i - 1] = Vector3.Lerp(baseCurrent.position, tipTargetPosition, 1 - t);
        }
        return linePositions;
    }
}
















// private Vector3[] GetFlexLinePositions(Vector3 startCurrent, Vector3 startTarget, Vector3 endCurrent, Vector3 endTarget) {
//     Vector3[] positions = new Vector3[numSegments];
//     for (int i = 0; i < numSegments; i++) {
//         float t = (float)i / (float)(numSegments - 1);
//         Vector3 start = Vector3.Lerp(startCurrent, startTarget, 1-t);
//         Vector3 end = Vector3.Lerp(endCurrent, endTarget, t);
//         positions[i] = Vector3.Lerp(start, end, t);
//     }
//     return positions;
// }