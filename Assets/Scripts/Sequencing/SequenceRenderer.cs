using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Manages rendering trails between sequence items
/// </summary>
public class SequenceRenderer : MonoBehaviour {
    public int MaxLines = 10;
    public float bezierSmoothness = 10f;
    public ManagedLineRenderer LineRenderer { get; private set; }
    private Sequence _sequence;
    private Action _lineUpdate;
    private List<Vector3> _positions = new List<Vector3>();

    private List<SequenceItem> _recentSequenceItems = new List<SequenceItem>();

    private void OnEnable() {
        LineRenderer = new ManagedLineRenderer(new LineRendererOptions {
            startColor = new Color(1, 1, 1, 1.0f),
            endColor = new Color(1, 1, 1, 1.0f),
            startWidth = 0.01f,
            endWidth = 0.01f
        }, "SequenceRenderer");

        //_lineUpdate = ConnectSequenceablesBezier;
        // _lineUpdate = ConnectRecentSequenceables;
        _lineUpdate = ConnectAllSequenceables;
    }

    public void SetSequence(Sequence sequence) {
        _sequence = sequence;
        _sequence.OnSequenceAdvance += OnSequenceAdvance;
    }


    private void OnSequenceAdvance(SequenceItem sequenceItem) {
        if (sequenceItem.Sequenceable is IInteractableSequenceable)
        {
            _recentSequenceItems.Add(sequenceItem);
            
            // if (_recentSequenceItems.Count > MaxLines) {
            //     Debug.Log("LENGTH OF RECENT SEQUENCE ITEMS " + _recentSequenceItems.Count);
            //     _recentSequenceItems.RemoveAt(0);
            // }
            while (_recentSequenceItems.Count > MaxLines) {
                // Debug.Log("Removing old sequence item " + sequenceItem.RelativePlayTime);
                _recentSequenceItems.RemoveAt(0);
            }

        }
    }


    void Update() {
        if (_sequence == null) return;
        if (_sequence.Count < 2) {
            if (LineRenderer.Enabled) {
                LineRenderer.Enabled = false;
            }
            return;
        } else {
            if (!LineRenderer.Enabled) {
                LineRenderer.Enabled = true;
            }
        }

        _lineUpdate?.Invoke();
    }

    int _renderIndex = 0;

    private void ConnectAllSequenceables() {
        _positions.Clear();
        foreach (var sequenceItem in _sequence)
        {
            if (sequenceItem.Sequenceable is IInteractableSequenceable) {
                _positions.Add(((IInteractableSequenceable)sequenceItem.Sequenceable).Position);
            }
        }
        // VectorHelpers.GenerateBezierPoints(positions, 10);
        LineRenderer.SetPositions(_positions);
    }

    private void UpdatePositionsFromRecent()
    {
        _positions.Clear();
        foreach (var sequenceItem in _recentSequenceItems)
        {
            if (sequenceItem.Sequenceable is IInteractableSequenceable)
            {
                _positions.Add(((IInteractableSequenceable)sequenceItem.Sequenceable).Position);
            }
        }
    }

    /// <summary>
    /// Draw a line between the sequence items that have already played
    /// </summary>
    private void ConnectRecentSequenceablesBezier() {
        UpdatePositionsFromRecent();
        LineRenderer.SetPositions(VectorHelpers.GenerateBezierPoints(_positions, bezierSmoothness));
    }

    /// <summary>
    /// Draw a line between the most recently played sequenceables
    /// </summary>
    private void ConnectRecentSequenceables() {
        _positions.Clear();
        UpdatePositionsFromRecent();
        LineRenderer.SetPositions(_positions);
    }
}