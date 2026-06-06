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

    private void OnDestroy()
    {
        EventBus<OnJulietteFinishedTalk>.OnEvent -= SpeechFinished;

        if (actionButton != null)
        {
            actionButton.action.performed -= OnStartDialogue;
            actionButton.action.Disable();
            EventBus<OnPlayerSitDown>.OnEvent -= PlayerSatDown;
        }
    }

    public override void StartTask()
    {
        base.StartTask();

        EventBus<OnPlayerSitDown>.OnEvent += PlayerSatDown;

        // Setup the event. We only skip when this event fires with isSitting = true.
        PlayerSatDown(new OnPlayerSitDown(isSitting));
    }

    public override void FinishTask()
    {
        base.FinishTask();

        EventBus<OnPlayerSitDown>.OnEvent -= PlayerSatDown;
        if (actionButton != null)
        {
            actionButton.action.Disable();
            actionButton.action.performed -= OnStartDialogue;
        }
    }

    private void OnStartDialogue(InputAction.CallbackContext context)
    {
        if (isProcessing || questionsAsked >= maxQuestions || GameManager.Instance == null) { return; }

        bool isRecording = GameManager.Instance.VoiceInterManager.IsRecording;

        if (!isRecording) { GameManager.Instance.StartRecordingVoice(); }
        else { ProcessRecording(); }
    }

    private void ProcessRecording()
    {
        GameManager.Instance.StopRecordingVoice();
        questionsAsked++;

        if (questionsAsked >= maxQuestions) { EventBus<OnJulietteFinishedTalk>.OnEvent += SpeechFinished; }
    }

    private void SpeechFinished(OnJulietteFinishedTalk evt)
    {
        EventBus<OnJulietteFinishedTalk>.OnEvent -= SpeechFinished;
        FinishTask();
    }

    private void PlayerSatDown(OnPlayerSitDown evt)
    {
        isSitting = evt.isSitting;

        if (isActive && evt.isSitting)
        {
            actionButton.action.Enable();
            actionButton.action.performed += OnStartDialogue;

            // Only skip when player is actually sitting
            StartCoroutine(SkipTaskDelay());
        }
        else if (isActive)
        {
            actionButton.action.Disable();
            actionButton.action.performed -= OnStartDialogue;
        }
    }

    private IEnumerator SkipTaskDelay()
    {
        yield return null;
        Debug.LogWarning("Skipping dialogue");
        FinishTask();
    }
}