using UnityEditor.Localization;

/// <summary>
/// Declared as class instead of struct, to have an easier time with handling references.
/// Used to define and queue story progression, in form of dialogue and mood transitions.
/// </summary>
public class StoryAction
{
    public LocalizationTableCollection dialogue;
    public VolumeConfiguration moodConfig;
    public float moodTransitionTime;
}
