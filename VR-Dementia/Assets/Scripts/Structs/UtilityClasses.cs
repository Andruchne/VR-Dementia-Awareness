using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public enum TutorialHUDs { Turning, Moving, MenuOpen, Grab }
public enum IndicatorHUDs { PressSend, EnterHome, SugarPickup }

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

[Serializable]
public struct TutorialInstance
{
    public TutorialHUDs tutorialType;
    public GameObject instance;
}

[Serializable]
public struct IndicatorInstance
{
    public IndicatorHUDs indicatorType;
    public GameObject instance;
}