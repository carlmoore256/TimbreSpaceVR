using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SequenceMeterDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _currentBar;


    [SerializeField] private TextMeshProUGUI _baseNote;
    [SerializeField] private TextMeshProUGUI _currentBPM;

    private RhythmClock _clock;

    public void SetSequence(Sequence sequence)
    {
        sequence.OnSequenceAdvance += OnSequenceAdvance;
        _clock = sequence.Clock;

        _clock.OnTempoChanged += OnTempoChanged;
    }

    private void OnTempoChanged()
    {
        _currentBPM.text = _clock.BPM.ToString();
    }

    private void OnSequenceAdvance(SequenceItem sequenceItem)
    {
        _currentBar.text = _clock.CurrentBar.ToString();
        // _currentBar.text = sequenceItem.BeatIndex.Bar.ToString();
    }
}
