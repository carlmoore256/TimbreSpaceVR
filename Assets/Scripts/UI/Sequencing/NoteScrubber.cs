using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class NoteScrubber : MonoBehaviour
{
    private enum NoteSequenceDisplayMode
    {
        AutoScroll,
        Edit
    }
    public GameObject noteSliderPrefab;

    public NoteValue Quantization = NoteValue.Sixteenth;
    private NoteSequenceDisplayMode _mode = NoteSequenceDisplayMode.AutoScroll;
    [SerializeField] private RectTransform _trackPanel;
    [SerializeField] private RectTransform _infoPanel;
    // private List<Slider> _notes = new List<Slider>();
    private Sequence _sequence;
    private RhythmClock _clock;
    private int _currentBar = 1;
    private int _currentBarRange = 1;

    private Dictionary<SequenceItem, Slider> _noteMap = new Dictionary<SequenceItem, Slider>();

    // each slider will represent a note within a bar

    private BeatIndex _startBounds;
    private BeatIndex _endBounds;


    private SequenceMeterDisplay _meterDisplay;
        
    // private void Update()
    // {
    //     if (Input.GetKey)
    // }

    public void SetSequence(Sequence sequence)
    {
        _sequence = sequence;
        _clock = _sequence.Clock;
        _sequence.OnSequenceAdvance += OnSequenceAdvance;
        _startBounds = new BeatIndex(0, NoteValue.Quarter, 0, _clock.TimeSignature);
        _endBounds = new BeatIndex(1, NoteValue.Quarter, 0, _clock.TimeSignature);

        // try this approach of object composition, by supposing the objects will
        // have the attached components, which is more designer friendly
        _meterDisplay = gameObject.GetComponentInChildren<SequenceMeterDisplay>();
        _meterDisplay.SetSequence(sequence);

        SetDisplayRange(_currentBar, _currentBar + _currentBarRange);
    }

    public void SetDisplayRange(int barStart, int barEnd)
    {
        if (barEnd < barStart || barStart == barEnd)
        {
            throw new Exception("Invalid display range: " + barStart + " - " + barEnd);
        }
        _startBounds.QuantizeToBar(barStart);
        _endBounds.QuantizeToBar(barEnd);

        // now find all notes between these ranges
        var notesInRange = _sequence.SequenceItems.Where(item => item.BeatIndex.Bar >= barStart && item.BeatIndex.Bar <= barEnd);
        // var notesInRange = _sequence.SequenceItemsBetween(_startBounds, _endBounds);

        Debug.Log("Setting display range: " + barStart + " - " + barEnd + " | " + notesInRange.Count() + " notes found");

        ClearNotes();

        _currentBarRange = barEnd - barStart;

        foreach(var note in notesInRange)
        {
            AddNoteSlider(note);
        }

        // ChangeBar(barStart);
    }

    private void AddNoteSlider(SequenceItem sequenceItem)
    {
        var floatPos = sequenceItem.BeatIndex.NormalizedPositionWithinBar();
        var slider = Instantiate(noteSliderPrefab, _trackPanel).GetComponent<Slider>();
        slider.value = floatPos;
        Debug.Log("Adding note with COLOR: " + sequenceItem.Parameters.Color.ToString());
        slider.GetComponentInChildren<Image>().color = sequenceItem.Parameters.Color;

        _noteMap.Add(sequenceItem, slider);
        slider.onValueChanged.AddListener((float value) => OnSliderValueChanged(sequenceItem, value));
    }

    private void OnSliderValueChanged(SequenceItem sequenceItem, float value)
    {
        var newBeatIndex = BeatIndex.Lerp(_startBounds, _endBounds, value, Quantization);
        _sequence.RescheduleItem(sequenceItem, newBeatIndex);
        // _sequence.RunUpdate();
    }

    private void ClearNotes()
    {
        foreach(var note in _noteMap.Values)
        {
            Destroy(note.gameObject);
        }
        _noteMap.Clear();
    }
    
    private void OnSequenceAdvance(SequenceItem sequenceItem)
    {
        Debug.Log("OnSequenceAdvance: " + sequenceItem.Sequenceable.Id + " | " + sequenceItem.BeatIndex?.ToString());
        
        if (TsvrApplication.Settings.EnableGrainDebugGizmos) 
        {
            ((MonoBehaviour)sequenceItem.Sequenceable).gameObject.GetComponent<DebugGizmo>().ShowFor(0.5f);
        }


        // use BeatIndex normalize time
        // if (sequenceItem.BeatIndex.Bar < _startBounds.Bar || sequenceItem.BeatIndex.Bar > _endBounds.Bar)
        // {
        //     // Debug.Log("SequenceItem is out of range: " + sequenceItem.BeatIndex.ToString());
        //     return;
        // }

        // if (sequenceItem.BeatIndex < _startBounds || sequenceItem.BeatIndex > _endBounds)
        // {
        //     Debug.Log("SequenceItem is out of range: " + sequenceItem.BeatIndex.ToString());
        //     return;
        // }

        if (_mode == NoteSequenceDisplayMode.AutoScroll && sequenceItem.BeatIndex.Bar != _currentBar)
        {
            ChangeBar(sequenceItem.BeatIndex.Bar);
        }

        if (_noteMap.ContainsKey(sequenceItem))
        {
            // set the current SequenceItem color as being played
            ActivateSequenceItem(sequenceItem);
        }
    }

    private void ActivateSequenceItem(SequenceItem sequenceItem)
    {
        Image image = _noteMap[sequenceItem].GetComponentInChildren<Image>();
        // CoroutineHelpers.ChangeColor(image, Color.red, sequenceItem.Parameters.Color, 0.5f, this);
        StartCoroutine(AnimateColor(_noteMap[sequenceItem], Color.red, sequenceItem.Parameters.Color, 0.5f));
    }

    private IEnumerator AnimateColor(Slider slider, Color startColor, Color endColor, float duration)
    {
        float time = 0;
        var image = slider.GetComponentInChildren<Image>();
        image.color = startColor;
        while (time < duration)
        {
            if (slider == null)
            {
                yield break;
            }
            time += Time.deltaTime;
            image.color = Color.Lerp(startColor, endColor, time / duration);
            yield return null;
        }
        image.color = endColor;
    }

    /// IDEA - Make a SequencePlayer class that keeps track of things in the sequence like the current bar,
    /// current index, etc. It also has methods to play, pause, etc

    public void ChangeBar(int bar)
    {
        _currentBar = bar;
        ClearNotes();
        var sequenceItems = _sequence.SequenceItems.FindAll(item => item.BeatIndex.Bar == _currentBar);
        Debug.Log("Setting notes for bar " + _currentBar + " with " + sequenceItems.Count + " items");
        sequenceItems.ForEach(item => AddNoteSlider (item));
    }
}