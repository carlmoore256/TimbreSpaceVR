using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

namespace TimbreSpaceVR.Tests {

    public class TestSequencerUI : MonoBehaviour
    {
        public NoteScrubber noteSequenceDisplay;
        private TestMusicalSequencing testMusicalSequencing;

        InspectorButton inspectorButton;

        [Range(1, 10)]
        [SerializeField]
        private int _barRange = 4;

        public int BarRange { 
            get => _barRange;
            set
            {
                if (_barRange != value)
                {
                    _barRange = value;
                    OnBarRangeChanged(value);
                }
        } }

        private void OnEnable()
        {
            inspectorButton = GetComponent<InspectorButton>();
            testMusicalSequencing = GetComponent<TestMusicalSequencing>();
            inspectorButton.OnButtonPressed += () => {
                Debug.Log("Button pressed! Restarting Sequence...");
                testMusicalSequencing.Sequence.Restart();
            };
        }



        private void OnBarRangeChanged(int value)
        {
            Debug.Log("Bar range changed to " + value);
            // noteSequenceDisplay.SetDisplayRange(0, value);
        }

    }


}