using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Enums used in different scripts
/// </summary>
public enum TutorialType { Turning, Moving, MenuOpen, Grab, None }
public enum IndicatorHUDs { PressSend, EnterHome, SugarPickup }
public enum JulietteAnimations { OpenDoor, Walk, Sit, IdleStand }

// Was replaced by locomotion events, but perhaps might be useful still
public enum JoystickDirection { Any, Up, Horizontal }


/// <summary>
/// Classes and structs used to keep data within script concise, when needed
/// </summary>
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

[Serializable]
public struct TutorialInstance
{
    public TutorialType tutorialType;
    public GameObject hudInstance;
    public GameObject[] movementComponent;
}

[Serializable]
public struct IndicatorInstance
{
    public IndicatorHUDs indicatorType;
    public GameObject instance;
}

[Serializable]
public struct JulietteAnimationInfo
{
    public JulietteAnimations animationType;
    public GameObject startPosition;
}