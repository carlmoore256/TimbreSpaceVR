using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.UI;

// ==============================================================================
// Notes
// ==============================================================================
// each pair of hands will have a default tool pair, for instance, wand and AudioEffect
// the user can switch between tools using the Y and B buttons, for each hand respecively
// this leaves the primary button, grip trigger, and forward trigger on the controller
// free for tool specific actions menu button on the left hand will ALWAYS bring up 
// the standard menu. This menu can't be accessed from the right unless left handedness 
// is selected

// some tools will always be on the left and right hand

// double tap other controller secondary button for default pair, for instance with 
// ConstellationComposer and ConstellationPlayer

// tools can be whole interactive panels - the mixer can be a virtual mixer that you 
// grab the knobs and faders on on some combos, a hand can appear if the opposite hand 
// comes close enough, like using a wrist watch

// long tap to fix tool in its place, defaulting to empty hand/pointer, leaving space 
// for another tool even

// hover over areas with multitool for different actions
// ==============================================================================

public enum TsvrToolType {
    PlayWand,
    DeleteWand,
    ConstellationWand,
    Menu,
    GrainSpawner,
    Teleport,
    Grab,
    VolumeSlector,
    Sequencer,
    ConstellationComposer,
    AudioFile,
    AudioEffect,
    AudioMixer,
    Eraser,
    StatsDisplay,
}

public enum TsvrToolSound {
    Equip,
    Unequip,
    Select,
    Deselect,
    Drop,
    Grab,
}

// Interface for all tools
public interface ITool {
    TsvrToolType ToolType { get; }
    ActionBasedController Controller { get; } // maybe remove this
    GameObject Prefab { get; }
    Transform ToolGameObject { get; protected set; }
    void SubscribeActions();
    void UnsubscribeActions();
    void Spawn(ActionBasedController controller);
    // void PlayToolSound(TsvrToolSound sound);
    void Destroy();
}

