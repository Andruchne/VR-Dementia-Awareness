using UnityEngine;

public class MoodTransitionTrigger : MonoBehaviour
{
    [SerializeField]
    private VolumeConfiguration volumeConfiguration;

    [SerializeField]
    private float transitionTime = 2;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.TransitionMood(volumeConfiguration, transitionTime);
        }
    }
}
