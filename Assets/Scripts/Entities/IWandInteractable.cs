using System;
using UnityEngine;

public enum WandInteractionType
{
    Play,
    Select,
    Delete
}

public class WandInteraction
{
    public WandInteractionType ActionType { get; set; }
    public float Value { get; set; }
    public Vector3 Position { get; set; }    
    public Action OnStart { get; set; }
    public Action OnEnd { get; set; }
}

public interface IWandInteractable
{
    void DoWandInteraction(WandInteraction interaction);
    event EventHandler<WandInteraction> OnWandInteract;
}