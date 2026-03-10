using UnityEditor.Localization;
using UnityEngine;

public class MoodTransitionTrigger : MonoBehaviour
{

    [Header("Mood Configuration")]
    [SerializeField] private VolumeConfiguration volumeConfiguration;
    [SerializeField] private float transitionTime = 2;

    [Header("Dialogue (Optional)")]
    [SerializeField] private LocalizationTableCollection dialogueTrigger;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.TransitionMood(volumeConfiguration, transitionTime);
        }
    }
}
