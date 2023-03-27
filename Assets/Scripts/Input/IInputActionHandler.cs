using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public interface IInputActionHandler {
    public MonoBehaviour Owner { get; set; }
    void Enable();
    void Disable();
    void RemoveAllObservers();
}