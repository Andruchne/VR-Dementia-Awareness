using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Used in the alpha scene, to transition between moods within a specific area.
/// It uses coroutines and simple timing logic, to transition between the available moods.
/// </summary>
[RequireComponent(typeof(Collider))]
public class MoodSequenceTrigger : MonoBehaviour
{
    [Header("Mood Sequence")]
    [Tooltip("List of VolumeConfigurations (Moods) to play in order.")]
    public VolumeConfiguration[] moodSequence;

    [Tooltip("How long it takes to transition into a new mood (in seconds).")]
    public float transitionTime = 10f;

    [Tooltip("How long the mood stays fully active BEFORE moving to the next one (in seconds).")]
    public float moodDuration = 3f;

    [Header("GameObject References")]
    [Tooltip("GameObject to hide as soon as the player enters the trigger.")]
    public GameObject objectToHideOnEnter;

    [Tooltip("GameObject to hide when the entire mood sequence is finished.")]
    public GameObject objectToHideOnComplete;

    [Tooltip("First GameObject to show when the sequence is finished.")]
    public GameObject objectToShowOnComplete1;

    [Tooltip("Second GameObject to show when the sequence is finished.")]
    public GameObject objectToShowOnComplete2;

    // Component References
    private MeshRenderer zoneRenderer;
    private Collider zoneCollider;

    // State tracking
    private int currentMoodIndex = 0;
    private float timeInCurrentMood = 0f;
    private bool sequenceCompleted = false;

    private string playerTag = "Player";

    private Coroutine sequenceCoroutine;

    private void Start()
    {
        zoneRenderer = GetComponent<MeshRenderer>();
        zoneCollider = GetComponent<Collider>();

        // Ensure the completion objects are hidden at the start
        if (objectToShowOnComplete1 != null) { objectToShowOnComplete1.SetActive(false); }
        if (objectToShowOnComplete2 != null) { objectToShowOnComplete2.SetActive(false); }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only trigger if it's the player and we haven't finished the sequence yet
        if (other.CompareTag(playerTag) && !sequenceCompleted)
        {
            if (objectToHideOnEnter != null) { objectToHideOnEnter.SetActive(false); }

            // Stop any potentially running sequence to avoid overlaps
            if (sequenceCoroutine != null) { StopCoroutine(sequenceCoroutine); }

            // Start or resume the sequence
            sequenceCoroutine = StartCoroutine(PlayMoodSequence());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            FadeOutMoods(0.5f);

            if (!sequenceCompleted)
            {
                // Pause the progression
                if (sequenceCoroutine != null)
                {
                    StopCoroutine(sequenceCoroutine);
                    sequenceCoroutine = null;
                }
            }
        }
    }

    private IEnumerator PlayMoodSequence()
    {
        // Continue going through the array as long as there are moods left
        while (currentMoodIndex < moodSequence.Length)
        {
            if (GameManager.Instance != null) { GameManager.Instance.TransitionMood(moodSequence[currentMoodIndex], transitionTime); }

            float totalTimeForThisMood = transitionTime + moodDuration;

            // Wait until our tracked time reaches the required total time
            while (timeInCurrentMood < totalTimeForThisMood)
            {
                timeInCurrentMood += Time.deltaTime;
                yield return null; // Wait for the next frame
            }

            currentMoodIndex++;
            timeInCurrentMood = 0f; 
        }

        sequenceCompleted = true;

        if (objectToHideOnComplete != null) { objectToHideOnComplete.SetActive(false); }
        if (objectToShowOnComplete1 != null) { objectToShowOnComplete1.SetActive(true); }
        if (objectToShowOnComplete2 != null) { objectToShowOnComplete2.SetActive(true); }

        FadeOutMoods(transitionTime);

        // Deactivate itself
        if (zoneRenderer != null) { zoneRenderer.enabled = false; }
        if (zoneCollider != null) { zoneCollider.enabled = false; }
        this.enabled = false;
    }

    private void FadeOutMoods(float duration)
    {
        if (GameManager.Instance == null) { return; }

        VolumeConfiguration emptyConfig = ScriptableObject.CreateInstance<VolumeConfiguration>();
        emptyConfig.configs = new List<VolumeConfig>();

        GameManager.Instance.TransitionMood(emptyConfig, duration);
    }
}