using UnityEngine;
using UnityEngine.InputSystem;

public class JulietteConversation : SimulationTask
{
    [Header("Input to check")]
    [SerializeField] InputActionReference actionButton;

    [Header("How many questions can be asked")]
    [SerializeField] int maxQuestions = 5;
    private int questionsAsked;
    private bool isProcessing = false;

    private void OnDestroy()
    {
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

        if (actionButton != null)
        {
            actionButton.action.Enable();
            actionButton.action.performed += OnStartDialogue;
            EventBus<OnPlayerSitDown>.OnEvent += PlayerSatDown;
        }
    }

    public override void FinishTask()
    {
        base.FinishTask();

        if (actionButton != null)
        {
            actionButton.action.Disable();
            actionButton.action.performed -= OnStartDialogue;
            EventBus<OnPlayerSitDown>.OnEvent -= PlayerSatDown;
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

        if (questionsAsked >= maxQuestions) { FinishTask(); }
    }

    private void PlayerSatDown(OnPlayerSitDown evt)
    {
        if (evt.isSitting)
        {
            actionButton.action.Enable();
            actionButton.action.performed += OnStartDialogue;
        }
        else
        {
            actionButton.action.Disable();
            actionButton.action.performed -= OnStartDialogue;
        }
    }
}
