using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// Manages rendering trails between sequence items
/// </summary>
public class SequenceRenderer : MonoBehaviour, ISequenceObserver {
    public ManagedLineRenderer lineRenderer;
    private IEnumerable<SequenceItem> sequenceItems;


    private void OnEnable() {
        // SequenceManager.Instance.AddObserver(this); <- hmm maybe 
        lineRenderer = new ManagedLineRenderer(new LineRendererOptions {
            startColor = Color.red,
            endColor = Color.white,
            startWidth = 0.01f,
            endWidth = 0.01f
        }, "SequenceRenderer");
    }

    public void OnSequenceUpdated(IEnumerable<SequenceItem> sequenceItems)
    {
        Debug.Log("Called onSequenceUpdated, num items " + sequenceItems.Count());
        this.sequenceItems = sequenceItems;
    }

    void Update() {
        UpdateLineRenderer();
    }

    private void UpdateLineRenderer() {
        if (sequenceItems == null || sequenceItems.Count() < 2) {
            if (lineRenderer.Enabled) {
                lineRenderer.Enabled = false;
            }
            return;
        };

        if (!lineRenderer.Enabled) {
            lineRenderer.Enabled = true;
        }
        // Filter IPositionedSequenceable items from the sequenceItems list
        // var positionedSequenceables = sequenceItems
        //     .Where(item => item.sequenceable is IPositionedSequenceable)
        //     .Select(item => (IPositionedSequenceable)item.sequenceable);

        // Obtain positions of the IPositionedSequenceable objects
        List<Vector3> positions = new List<Vector3>();
        foreach (var sequenceItem in sequenceItems)
        {
            if (sequenceItem.sequenceable is IPositionedSequenceable)
                positions.Add(((IPositionedSequenceable)sequenceItem.sequenceable).Position);
        }

        // VectorHelpers.GenerateBezierPoints(positions, 10);

        // Update the line positions
        lineRenderer.SetPositions(positions);
    }
}