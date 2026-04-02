using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Used for the VR Menu with the mood sliders.
/// Utilized GameManager functions, to control both the visual mood, and the sound plaýed.
/// </summary>
public class MoodUI : MonoBehaviour
{
    [Header("UI Sliders (Set Min: 0, Max: 100, Whole Numbers: True)")]
    public Slider M_Anxious_Slider;
    public Slider M_Furious_Slider;
    public Slider M_Happy_Slider;
    public Slider M_Nostalgic_Slider;
    public Slider M_Sad_Slider;

    private void Start()
    {
        if (M_Anxious_Slider != null) { M_Anxious_Slider.onValueChanged.AddListener((val) => SetMood(Mood.Anxious, val)); }

        if (M_Furious_Slider != null) { M_Furious_Slider.onValueChanged.AddListener((val) => SetMood(Mood.Furious, val)); }

        if (M_Happy_Slider != null) { M_Happy_Slider.onValueChanged.AddListener((val) => SetMood(Mood.Happy, val)); }

        if (M_Nostalgic_Slider != null) { M_Nostalgic_Slider.onValueChanged.AddListener((val) => SetMood(Mood.Nostalgic, val)); }

        if (M_Sad_Slider != null) { M_Sad_Slider.onValueChanged.AddListener((val) => SetMood(Mood.Sad, val)); }
    }

    private void SetMood(Mood mood, float val)
    {
        if (GameManager.Instance != null)
        {
            // Convert float to integer and set percentage
            GameManager.Instance.SetMoodPercentage(mood, (int)val);
        }
    }

    private void OnDestroy()
    {
        if (M_Anxious_Slider != null) M_Anxious_Slider.onValueChanged.RemoveAllListeners();
        if (M_Furious_Slider != null) M_Furious_Slider.onValueChanged.RemoveAllListeners();
        if (M_Happy_Slider != null) M_Happy_Slider.onValueChanged.RemoveAllListeners();
        if (M_Nostalgic_Slider != null) M_Nostalgic_Slider.onValueChanged.RemoveAllListeners();
        if (M_Sad_Slider != null) M_Sad_Slider.onValueChanged.RemoveAllListeners();
    }
}