using UnityEngine;
using UnityEngine.UI;

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
        // Listener hinzufügen: Wird aufgerufen, wenn der Spieler den Slider bewegt
        if (M_Anxious_Slider != null)
            M_Anxious_Slider.onValueChanged.AddListener((val) => SetMood(Mood.Triggered, val));

        if (M_Furious_Slider != null)
            M_Furious_Slider.onValueChanged.AddListener((val) => SetMood(Mood.Furious, val));

        if (M_Happy_Slider != null)
            M_Happy_Slider.onValueChanged.AddListener((val) => SetMood(Mood.Happy, val));

        if (M_Nostalgic_Slider != null)
            M_Nostalgic_Slider.onValueChanged.AddListener((val) => SetMood(Mood.Nostalgic, val));

        if (M_Sad_Slider != null)
            M_Sad_Slider.onValueChanged.AddListener((val) => SetMood(Mood.Sad, val));
    }

    private void SetMood(Mood mood, float val)
    {
        if (GameManager.Instance != null)
        {
            // Konvertiere den Float-Wert des Sliders in einen Integer und setze die Mood
            GameManager.Instance.SetMoodPercentage(mood, (int)val);
        }
    }

    private void OnDestroy()
    {
        // Sauberes Aufräumen der Listener, wenn das UI zerstört wird
        if (M_Anxious_Slider != null) M_Anxious_Slider.onValueChanged.RemoveAllListeners();
        if (M_Furious_Slider != null) M_Furious_Slider.onValueChanged.RemoveAllListeners();
        if (M_Happy_Slider != null) M_Happy_Slider.onValueChanged.RemoveAllListeners();
        if (M_Nostalgic_Slider != null) M_Nostalgic_Slider.onValueChanged.RemoveAllListeners();
        if (M_Sad_Slider != null) M_Sad_Slider.onValueChanged.RemoveAllListeners();
    }
}