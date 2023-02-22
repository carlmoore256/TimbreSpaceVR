using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

// [System.Serializable]
public class EntityAudio : MonoBehaviour
{
    public AudioSource audioSource;
    public Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
    public void PlaySound(string name, float volume = 1f) {
        if (audioClips.ContainsKey(name)) {
            audioSource.PlayOneShot(audioClips[name], volume);
        } else {
            Debug.LogWarning("Audio clip not found: " + name);
        }
    }
}

[RequireComponent(typeof(EntityAudio))]
public abstract class TsvrTool : MonoBehaviour {
    [HideInInspector] public Animation animations;
    public abstract TsvrToolType ToolType { get; }
    public ToolController ToolController { get; set; }
    public ControllerActions ControllerActions { get; set; }
    protected Dictionary<InputActionValuePair, InputActionValueHandler> inputActionHandlers;
    
    void Awake() {
        animations = GetComponent<Animation>();
        ToolController = transform.parent.GetComponent<ToolController>();
        ControllerActions = transform.parent.GetComponent<ControllerActions>();
        inputActionHandlers = new Dictionary<InputActionValuePair, InputActionValueHandler>();
    }

    public virtual void Equip() {
    }

    public virtual void Unequip() {
    }

    void OnDestroy() {
        foreach(var handler in inputActionHandlers) {
            handler.Value.Disable();
        }
    }

    protected IEnumerator AnimationListener(string name, Action onComplete) {
        while (animations.IsPlaying(name)) {
            yield return null;
        }
        onComplete();
    }

    // add methods to play sound when equipped and unequipped
}
