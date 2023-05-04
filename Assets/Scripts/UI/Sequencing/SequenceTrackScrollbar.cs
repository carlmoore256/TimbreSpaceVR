using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class SequenceTrackScrollbar : MonoBehaviour
{
    [SerializeField] private Scrollbar _scrollbar;

    private BeatIndex _startBounds;
    private BeatIndex _endBounds;

    private void OnEnable()
    {
        // _startBounds = new BeatIndex(0, NoteValue.Quarter, 0, _clock.TimeSignature);
        // _endBounds = new BeatIndex(1, NoteValue.Quarter, 0, _clock.TimeSignature);
    }

    public void SetBounds(BeatIndex startBounds, BeatIndex endBounds)
    {
        _startBounds = startBounds;
        _endBounds = endBounds;
    }
}