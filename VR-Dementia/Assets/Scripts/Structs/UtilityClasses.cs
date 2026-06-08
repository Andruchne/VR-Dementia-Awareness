using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// Was replaced by locomotion events, but perhaps might be useful still
public enum JoystickDirection { Any, Up, Horizontal }

[Serializable]
public struct InputStep
{
    public InputActionReference actionReference;
    public JoystickDirection requiredDirection;
    public GameObject[] locomotionComponent;
}


// Used to update tasks on menu
[Serializable]
public struct TaskLine
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
}