using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Used in the alpha scene to trigger the dialogues, and make the necessary description textfields visible
/// </summary>
[RequireComponent(typeof(Collider))]
public class DialogueChecker : MonoBehaviour
{
    [Header("Zone Settings")]
    [Tooltip("Tag of the player to trigger the zone.")]
    public string playerTag = "Player";
    [Tooltip("Maximum amount of questions the player can ask.")]
    public int maxQuestions = 5;

    [Header("UI Group References (Parents)")]
    public GameObject questionGroup;
    public GameObject statusGroup;
    public GameObject promptGroup;
    public GameObject moodInteraction;
    public GameObject moodArea;
    public GameObject moodDescription;

    [Header("UI Text References")]
    public TextMeshProUGUI promptText;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI statusText;

    [Header("Localization Strings")]
    public List<LocalizedString> localizedQuestions;
    public LocalizedString localizedCounter;
    public LocalizedString promptStartRecording;
    public LocalizedString promptStopRecording;

    [Header("Input")]
    public InputActionReference actionButton;

    // Component References
    private MeshRenderer zoneRenderer;
    private Collider zoneCollider;

    // State tracking
    private bool isPlayerInZone = false;
    private int questionsAsked = 0;
    private bool isProcessingOrSpeaking = false;

    private Coroutine recordingTimerCoroutine;
    private Coroutine processingAnimationCoroutine;

    private void Start()
    {
        zoneRenderer = GetComponent<MeshRenderer>();
        zoneCollider = GetComponent<Collider>();

        // Hide everything at start
        SetZoneUIActive(false);
        if (moodInteraction != null) { moodInteraction.SetActive(false); }
        if (moodArea != null) { moodArea.SetActive(false); }
        if (moodDescription != null) { moodDescription.SetActive(false); }

        // Preload texts at startup
        StartCoroutine(PreloadTextsAsync());

        // Subscribe to events
        LocalizationSettings.SelectedLocaleChanged += HandleLanguageChanged;

        if (GameManager.Instance != null && GameManager.Instance.VoiceInterManager != null)
        {
            GameManager.Instance.VoiceInterManager.OnProcessingStarted += HandleProcessingStarted;
            GameManager.Instance.VoiceInterManager.OnProcessingFinished += HandleProcessingFinished;
        }
    }

    private void OnDestroy()
    {
        LocalizationSettings.SelectedLocaleChanged -= HandleLanguageChanged;

        if (GameManager.Instance != null && GameManager.Instance.VoiceInterManager != null)
        {
            GameManager.Instance.VoiceInterManager.OnProcessingStarted -= HandleProcessingStarted;
            GameManager.Instance.VoiceInterManager.OnProcessingFinished -= HandleProcessingFinished;
        }
    }

    private void OnEnable()
    {
        if (actionButton != null)
        {
            actionButton.action.Enable();
            actionButton.action.performed += OnButtonPressed;
        }
    }

    private void OnDisable()
    {
        if (actionButton != null)
        {
            actionButton.action.performed -= OnButtonPressed;
            actionButton.action.Disable();
        }
    }

    // When the user changes language in the settings, preload texts again
    private void HandleLanguageChanged(Locale newLocale)
    {
        StartCoroutine(PreloadTextsAsync());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && questionsAsked < maxQuestions)
        {
            isPlayerInZone = true;

            if (zoneRenderer != null) { zoneRenderer.enabled = false; }

            // Texts are already preloaded! Just show the UI instantly.
            if (!isProcessingOrSpeaking)
            {
                SetZoneUIActive(true);

                // Hide counter group if no questions left (just to be safe)
                if (questionsAsked >= maxQuestions && statusGroup != null) { statusGroup.SetActive(false); }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInZone = false;

            SetZoneUIActive(false);

            if (questionsAsked < maxQuestions && zoneRenderer != null) { zoneRenderer.enabled = true; }

            if (GameManager.Instance != null && GameManager.Instance.VoiceInterManager.IsRecording)
            {
                if (recordingTimerCoroutine != null) StopCoroutine(recordingTimerCoroutine);
                GameManager.Instance.DiscardRecordingVoice();
                ResetPromptTextToStartAsync();
            }
        }
    }

    private void OnButtonPressed(InputAction.CallbackContext context)
    {
        if (!isPlayerInZone || isProcessingOrSpeaking || questionsAsked >= maxQuestions || GameManager.Instance == null) { return; }

        bool isRecording = GameManager.Instance.VoiceInterManager.IsRecording;

        if (!isRecording)
        {
            GameManager.Instance.StartRecordingVoice();

            if (promptText != null) { promptText.text = ""; }

            LocalizationSettings.StringDatabase.GetLocalizedStringAsync(promptStopRecording.TableReference, promptStopRecording.TableEntryReference).Completed += (op) =>
            {
                if (promptText != null) { promptText.text = op.Result; }
            };

            recordingTimerCoroutine = StartCoroutine(AutoStopRecordingTimer(20));
        }
        else
        {
            if (recordingTimerCoroutine != null) StopCoroutine(recordingTimerCoroutine);
            ProcessRecording();
        }
    }

    private IEnumerator AutoStopRecordingTimer(float maxDuration)
    {
        yield return new WaitForSeconds(maxDuration);
        ProcessRecording();
    }

    private void ProcessRecording()
    {
        GameManager.Instance.StopRecordingVoice();
        questionsAsked++;

        if (questionsAsked == 1)
        {
            if (moodInteraction != null) { moodInteraction.SetActive(true); }
            if (moodArea != null) { moodArea.SetActive(true); }
            if (moodDescription != null) { moodDescription.SetActive(true); }
        }
    }

    private void SetZoneUIActive(bool isActive)
    {
        if (questionGroup != null) { questionGroup.SetActive(isActive); }
        if (statusGroup != null) { statusGroup.SetActive(isActive); }
        if (promptGroup != null) { promptGroup.SetActive(isActive); }
    }

    // Loads all required texts in the background without affecting the visual state of the UI GameObjects.
    private IEnumerator PreloadTextsAsync()
    {
        yield return LocalizationSettings.InitializationOperation;

        int questionsLeft = maxQuestions - questionsAsked;

        bool promptDone = false;
        bool questionDone = false;
        bool counterDone = false;

        string promptStr = "";
        string questionStr = "";
        string counterStr = "";

        // Fetch Prompt
        LocalizationSettings.StringDatabase.GetLocalizedStringAsync(promptStartRecording.TableReference, promptStartRecording.TableEntryReference).Completed += (op) =>
        {
            promptStr = op.Result;
            promptDone = true;
        };

        // Fetch Question
        if (localizedQuestions != null && questionsAsked < localizedQuestions.Count)
        {
            LocalizedString targetQuestion = localizedQuestions[questionsAsked];
            LocalizationSettings.StringDatabase.GetLocalizedStringAsync(targetQuestion.TableReference, targetQuestion.TableEntryReference).Completed += (op) =>
            {
                questionStr = op.Result;
                questionDone = true;
            };
        }
        else questionDone = true;

        // Fetch Counter
        if (questionsLeft > 0)
        {
            object[] smartArgs = new object[] { questionsLeft };
            LocalizationSettings.StringDatabase.GetLocalizedStringAsync(localizedCounter.TableReference, localizedCounter.TableEntryReference, smartArgs).Completed += (op) =>
            {
                counterStr = op.Result;
                counterDone = true;
            };
        }
        else counterDone = true;

        yield return new WaitUntil(() => promptDone && questionDone && counterDone);

        if (promptText != null) { promptText.text = promptStr; }
        if (questionText != null) { questionText.text = questionStr; }

        if (statusText != null && !isProcessingOrSpeaking)
        {
            statusText.fontSize = 50;
            statusText.text = counterStr;
        }
    }

    private void ResetPromptTextToStartAsync()
    {
        LocalizationSettings.StringDatabase.GetLocalizedStringAsync(promptStartRecording.TableReference, promptStartRecording.TableEntryReference).Completed += (op) =>
        {
            if (promptText != null) { promptText.text = op.Result; }
        };
    }

    private void SetProcessingVisualsActive(bool isActive)
    {
        if (questionGroup != null) { questionGroup.SetActive(!isActive); }
        if (promptGroup != null) { promptGroup.SetActive(!isActive); }

        if (statusGroup != null) { statusGroup.SetActive(true); }
    }

    private void HandleProcessingStarted()
    {
        isProcessingOrSpeaking = true;
        SetProcessingVisualsActive(true);

        processingAnimationCoroutine = StartCoroutine(AnimateProcessingText());
    }

    private void HandleProcessingFinished()
    {
        StartCoroutine(WaitForSpeechToFinish());
    }

    private IEnumerator WaitForSpeechToFinish()
    {
        yield return new WaitForSeconds(0.5f);

        while (GameManager.Instance.VoiceInterManager.IsSpeaking) { yield return null; }

        isProcessingOrSpeaking = false;

        if (processingAnimationCoroutine != null) { StopCoroutine(processingAnimationCoroutine); }
        SetProcessingVisualsActive(false);

        if (questionsAsked < maxQuestions)
        {
            // Preload the next question in the background
            yield return StartCoroutine(PreloadTextsAsync());

            // Show UI only if player is still waiting in the zone
            if (isPlayerInZone) { SetZoneUIActive(true); }
        }
        else
        {
            SetZoneUIActive(false);

            if (zoneRenderer != null) zoneRenderer.enabled = false;
            if (zoneCollider != null) zoneCollider.enabled = false;

            isPlayerInZone = false;
        }
    }

    private IEnumerator AnimateProcessingText()
    {
        string baseText = "Processing";
        int dotCount = 0;

        statusText.fontSize = 100;

        while (true)
        {
            dotCount = (dotCount + 1) % 4;

            if (statusText != null) { statusText.text = baseText + new string('.', dotCount); }

            yield return new WaitForSeconds(0.4f);
        }
    }
}