using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class BeatGenerator
{
    public RhythmClock Clock { get; private set; }

    public BeatGenerator(RhythmClock clock)
    {
        Clock = clock;
    }

    public static string RandomBeatPattern(int beatsPerBar)
    {
        string pattern = "";
        for (int i = 0; i < beatsPerBar; i++)
        {
            pattern += UnityEngine.Random.Range(0, 2) == 0 ? "x" : "-";
        }
        return pattern;
    }

    public List<BeatIndex> BeatPatternFromString(string pattern)
    {
        List<BeatIndex> beatIndexes = new List<BeatIndex>();
        int patternLength = pattern.Length;
        
        int beatsPerBar = Clock.TimeSignature.BeatsPerBar;
        
        NoteValue noteValue = (NoteValue)(beatsPerBar * patternLength / beatsPerBar);

        if (patternLength % beatsPerBar != 0)
        {
            UnityEngine.Debug.LogWarning($"Invalid pattern length: {patternLength}. Trimming to fit the nearest valid length.");
            patternLength = (patternLength / beatsPerBar) * beatsPerBar;
            pattern = pattern.Substring(0, patternLength);
        }

        for (int i = 0; i < patternLength; i++)
        {
            if (pattern[i] == 'x')
            {
                // int bar = i / beatsPerBar;
                // int noteValuePosition = i % beatsPerBar;
                // Debug.Log("bar: " + bar + " noteValuePosition: " + noteValuePosition + " noteValue: " + noteValue);
                beatIndexes.Add(new BeatIndex(0, noteValue, i, Clock.TimeSignature));
            }
        }

        return beatIndexes;
    }

    public List<BeatIndex> RepeatPattern(List<BeatIndex> pattern, int numBars)
    {
        List<BeatIndex> repeatedPattern = new List<BeatIndex>();
        repeatedPattern.AddRange(pattern);

        for (int bar = 1; bar < numBars; bar++)
        {
            foreach (BeatIndex beatIndex in pattern)
            {
                BeatIndex newBeatIndex = new BeatIndex(beatIndex.Bar + bar, beatIndex.NoteValue, beatIndex.RelativeBeatPosition, Clock.TimeSignature);
                repeatedPattern.Add(newBeatIndex);
            }
        }

        return repeatedPattern;
    }

    public List<BeatIndex> GeneratePolyrhythmPattern(int numBars, List<NoteValue> rhythms)
    {
        List<BeatIndex> pattern = new List<BeatIndex>();

        int lcm = LCM(rhythms.Select(rhythm => (int)rhythm).ToArray());
        int beatsPerBar = Clock.TimeSignature.BeatsPerBar * lcm;

        for (int bar = 0; bar < numBars; bar++)
        {
            foreach (NoteValue rhythm in rhythms)
            {
                int numBeats = beatsPerBar / (int)rhythm;
                for (int beat = 0; beat < numBeats; beat++)
                {
                    pattern.Add(new BeatIndex(beat, rhythm, bar, Clock.TimeSignature));
                }
            }
        }

        return pattern;
    }

    public List<BeatIndex> GenerateSyncopatedPattern(int numBars, List<NoteValue> rhythms, int syncopationOffset = 1)
    {
        List<BeatIndex> pattern = new List<BeatIndex>();

        int lcm = LCM(rhythms.Select(rhythm => (int)rhythm).ToArray());
        int beatsPerBar = Clock.TimeSignature.BeatsPerBar * lcm;

        for (int bar = 0; bar < numBars; bar++)
        {
            foreach (NoteValue rhythm in rhythms)
            {
                int numBeats = beatsPerBar / (int)rhythm;
                for (int beat = 0; beat < numBeats; beat++)
                {
                    int syncopatedBeat = (beat + syncopationOffset) % numBeats;
                    pattern.Add(new BeatIndex(bar, rhythm, syncopatedBeat, Clock.TimeSignature));
                }
            }
        }

        return pattern;
    }

    public List<BeatIndex> GenerateAlgorithmicPattern(int numBars)
    {
        List<BeatIndex> pattern = new List<BeatIndex>();
        System.Random random = new System.Random();

        for (int bar = 0; bar < numBars; bar++)
        {
            // Downbeat
            pattern.Add(new BeatIndex(bar, Clock.TimeSignature.BaseNoteValue, 0, Clock.TimeSignature));

            // Generate random note values for the algorithmic pattern
            for (int i = 1; i < Clock.TimeSignature.BeatsPerBar; i++)
            {
                int noteValue = random.Next(1, 4) * (int)Clock.TimeSignature.BaseNoteValue;
                int noteValuePosition = random.Next(1, Clock.TimeSignature.BeatsPerBar);
                pattern.Add(new BeatIndex(bar, (NoteValue)noteValue, noteValuePosition, Clock.TimeSignature));
            }
        }

        return pattern;
    }

    public List<BeatIndex> GenerateKickPattern(int numBars, int option = 1)
    {
        List<BeatIndex> pattern = new List<BeatIndex>();

        for (int bar = 0; bar < numBars; bar++)
        {
            switch (option)
            {
                case 1:
                    // Basic 4/4 kick pattern
                    pattern.Add(new BeatIndex(bar, NoteValue.Quarter, 0, Clock.TimeSignature));
                    pattern.Add(new BeatIndex(bar, NoteValue.Quarter, 2, Clock.TimeSignature));
                    break;
                case 2:
                    // Slightly syncopated kick pattern
                    pattern.Add(new BeatIndex (bar, NoteValue.Quarter, 0, Clock.TimeSignature));
                    pattern.Add(new BeatIndex(bar, NoteValue.Quarter, 2, Clock.TimeSignature));
                    pattern.Add(new BeatIndex(bar, NoteValue.Eighth, 5, Clock.TimeSignature));
                    break;
                    // Add more kick pattern options here as needed
            }
        }

        return pattern;
    }

    public List<BeatIndex> GenerateSnarePattern(int numBars, int option = 1)
    {
        List<BeatIndex> pattern = new List<BeatIndex>();

        for (int bar = 0; bar < numBars; bar++)
        {
            switch (option)
            {
                case 1:
                    // Basic 4/4 snare pattern
                    pattern.Add(new BeatIndex(bar, NoteValue.Quarter, 1, Clock.TimeSignature));
                    pattern.Add(new BeatIndex(bar, NoteValue.Quarter, 3, Clock.TimeSignature));
                    break;
                case 2:
                    // Slightly syncopated snare pattern
                    pattern.Add(new BeatIndex(bar, NoteValue.Quarter, 1, Clock.TimeSignature));
                    pattern.Add(new BeatIndex(bar, NoteValue.Quarter, 3, Clock.TimeSignature));
                    pattern.Add(new BeatIndex(bar, NoteValue.Eighth, 6, Clock.TimeSignature));
                    break;
                    // Add more snare pattern options here as needed
            }
        }

        return pattern;
    }

    public List<BeatIndex> GenerateHiHatPattern(int numBars, int option = 1)
    {
        List<BeatIndex> pattern = new List<BeatIndex>();

        for (int bar = 0; bar < numBars; bar++)
        {
            switch (option)
            {
                case 1:
                    // Basic 4/4 hi-hat pattern (eighth notes)
                    for (int i = 0; i < Clock.TimeSignature.BeatsPerBar * 2; i++)
                    {
                        pattern.Add(new BeatIndex(bar, NoteValue.Eighth, i, Clock.TimeSignature));
                    }
                    break;
                case 2:
                    // 4/4 hi-hat pattern with sixteenth notes on every other beat
                    for (int i = 0; i < Clock.TimeSignature.BeatsPerBar * 4; i++)
                    {
                        if (i % 4 == 0 || i % 4 == 2)
                        {
                            pattern.Add(new BeatIndex(bar, NoteValue.Sixteenth, i, Clock.TimeSignature));                        
                        }
                        else
                        {
                            pattern.Add(new BeatIndex(bar, NoteValue.Eighth, i / 2, Clock.TimeSignature));
                        }
                    }
                    break;
                case 3:
                    // Syncopated 4/4 hi-hat pattern
                    pattern.Add(new BeatIndex(bar, NoteValue.Eighth, 0, Clock.TimeSignature));
                    pattern.Add(new BeatIndex(bar, NoteValue.Sixteenth, 2, Clock.TimeSignature));
                    pattern.Add(new BeatIndex(bar, NoteValue.Sixteenth, 3, Clock.TimeSignature));
                    pattern.Add(new BeatIndex(bar, NoteValue.Eighth, 4, Clock.TimeSignature));
                    pattern.Add(new BeatIndex(bar, NoteValue.Sixteenth, 6, Clock.TimeSignature));
                    pattern.Add(new BeatIndex(bar, NoteValue.Eighth, 8, Clock.TimeSignature));
                    pattern.Add(new BeatIndex(bar, NoteValue.Sixteenth, 10, Clock.TimeSignature));
                    pattern.Add(new BeatIndex(bar, NoteValue.Eighth, 12, Clock.TimeSignature));
                    pattern.Add(new BeatIndex(bar, NoteValue.Sixteenth, 14, Clock.TimeSignature));
                    break;
                    // Add more hi-hat pattern options here as needed
            }
        }

        return pattern;
    }




    private static int LCM(params int[] numbers)
    {
        return numbers.Aggregate(LCM);
    }

    private static int LCM(int a, int b)
    {
        return a * b / GCD(a, b);
    }

    private static int GCD(int a, int b)
    {
        while (b != 0)
        {
            int t = b;
            b = a % b;
            a = t;
        }
        return a;
    }
}