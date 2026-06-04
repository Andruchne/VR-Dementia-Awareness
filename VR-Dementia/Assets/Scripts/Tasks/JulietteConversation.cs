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

    // Used when you're already sitting, but the script is checking only after the fact
    private bool isSitting;

    private void Start()
    {
        EventBus<OnPlayerSitDown>.OnEvent += PlayerSatDown;
    }

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
        // Called for when the player is already sitting
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
        }
        else if (isActive)
        {
            actionButton.action.Disable();
            actionButton.action.performed -= OnStartDialogue;
        }

        if (evt.isSitting) { FinishTask(); }
        Debug.LogWarning("Skipping dialogue");
    }
}
