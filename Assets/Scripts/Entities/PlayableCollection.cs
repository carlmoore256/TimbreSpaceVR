using UnityEngine;
using System;
using System.Collections.Generic;

public interface IPlayableObserver
{
    void OnNotify(IPlayable playable);
}

// for things like Grains, or SynthNodes, FXNodes, ETC
// will these always be part of a playable collection?
public interface IPlayable
{
    void Play(); // what if this returns a ScheduledEvent?

    // public List<Insert> Inserts { get; set; }


    // ScheduleCancellationToken Schedule(double scheduleTime, SequenceableParameters parameters); // HOW CAN WE 
    // promote cooperation between IPlayable and ISequenceable? Which one should exist?
}


/// <summary>
/// A collection of Playables that can have sequences | I DON'T THINK WE'LL NEED THIS
/// </summary>
public interface IPlayableCollection
{

    // GrainCloud will inherit from this, so the Grains will become Playables
    // PERFECT, because then we can remove GrainCloud from the ISequenceable behavior,
    // because we actually need to minimize it to a playable

    List<IPlayable> Playables { get; set; }
    
    List<Sequence> Sequences { get; set; }
    
    IPlayable Minimize(); // this will return a playable that simply plays the audio
}