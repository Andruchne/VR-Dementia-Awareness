using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class JulietteConversation : SimulationTask
{
    [Header("Input to check")]
    [SerializeField] InputActionReference actionButton;

    [Header("How many questions can be asked")]
    [SerializeField] int maxQuestions = 5;

    private int questionsAsked;
    private bool isProcessing;

    private bool isSitting;

    private void Start()
    {
        EventBus<OnRequestTalk>.OnEvent += TriggerDialogue;
    }

    private void OnDestroy()
    {
        EventBus<OnRequestTalk>.OnEvent -= TriggerDialogue;
        EventBus<OnJulietteFinishedTalk>.OnEvent -= SpeechFinished;

        if (actionButton != null)
        {
            // Unsubscribe from both press and release events
            actionButton.action.started -= OnButtonStarted;
            actionButton.action.canceled -= OnButtonCanceled;
            actionButton.action.Disable();
            EventBus<OnPlayerSitDown>.OnEvent -= PlayerSatDown;
        }
    }

    public override void StartTask()
    {
        base.StartTask();

        EventBus<OnPlayerSitDown>.OnEvent += PlayerSatDown;
        GameManager.Instance.conversationActive = true;

        // Setup the event. We only skip when this event fires with isSitting = true.
        PlayerSatDown(new OnPlayerSitDown(isSitting));
        EventBus<OnUpdateTask>.Publish(new OnUpdateTask());
    }

    public override void FinishTask()
    {
        base.FinishTask();

        GameManager.Instance.conversationActive = false;
        EventBus<OnPlayerSitDown>.OnEvent -= PlayerSatDown;
        if (actionButton != null)
        {
            actionButton.action.Disable();
            // Unsubscribe from both press and release events
            actionButton.action.started -= OnButtonStarted;
            actionButton.action.canceled -= OnButtonCanceled;
        }
    }

    private void TriggerDialogue(OnRequestTalk evt)
    {
        Debug.LogWarning("heheh");
    }

    // Triggered exactly when the button is pressed down
    private void OnButtonStarted(InputAction.CallbackContext context)
    {
        if (isProcessing || questionsAsked >= maxQuestions || GameManager.Instance == null) { return; }

        bool isRecording = GameManager.Instance.VoiceInterManager.IsRecording;

        if (!isRecording)
        {
            GameManager.Instance.StartRecordingVoice();
            EventBus<OnShowMicrophonePickup>.Publish(new OnShowMicrophonePickup());
        }
    }

    // Triggered exactly when the button is released
    private void OnButtonCanceled(InputAction.CallbackContext context)
    {
        if (isProcessing || questionsAsked >= maxQuestions || GameManager.Instance == null) { return; }

        bool isRecording = GameManager.Instance.VoiceInterManager.IsRecording;

        // Only process if we were actually recording
        if (isRecording)
        {
            ProcessRecording();
            EventBus<OnShowProcessing>.Publish(new OnShowProcessing());
        }
    }

    private void ProcessRecording()
    {
        GameManager.Instance.StopRecordingVoice();
        questionsAsked++;
        EventBus<OnShowAfterDiscard>.Publish(new OnShowAfterDiscard());

        if (questionsAsked >= maxQuestions) { EventBus<OnJulietteFinishedTalk>.OnEvent += SpeechFinished; }
    }

    private void SpeechFinished(OnJulietteFinishedTalk evt)
    {
        EventBus<OnHideTalk>.Publish(new OnHideTalk());
        EventBus<OnJulietteFinishedTalk>.OnEvent -= SpeechFinished;
        FinishTask();
    }

    private void PlayerSatDown(OnPlayerSitDown evt)
    {
        isSitting = evt.isSitting;

        if (isActive && evt.isSitting)
        {
            actionButton.action.Enable();
            // Subscribe to both press and release events for push-to-talk
            actionButton.action.started += OnButtonStarted;
            actionButton.action.canceled += OnButtonCanceled;

            // Only skip when player is actually sitting
            StartCoroutine(SkipTaskDelay());
        }
        else if (isActive)
        {
            actionButton.action.Disable();
            // Unsubscribe from both press and release events
            actionButton.action.started -= OnButtonStarted;
            actionButton.action.canceled -= OnButtonCanceled;
        }
    }

    private IEnumerator SkipTaskDelay()
    {
        yield return null;
        Debug.LogWarning("Skipping dialogue");
        FinishTask();
    }
}