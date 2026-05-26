using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum JoystickDirection { Any, Up, Horizontal }

[Serializable]
public struct InputStep
{
    public InputActionReference actionReference;
    public JoystickDirection requiredDirection;
    public GameObject locomotionComponent;
}