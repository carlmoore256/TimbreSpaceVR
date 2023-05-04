using UnityEngine;
using System;
public class InspectorButton : MonoBehaviour
{
    public Action OnButtonPressed;
    public void ButtonAction()
    {
        OnButtonPressed?.Invoke();
    }
}