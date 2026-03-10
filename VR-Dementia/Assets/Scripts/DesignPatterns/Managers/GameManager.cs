using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public VolumeConfiguration volumeconfig;

    #region Variables and Instances

    private PostProcessingManager _ppManager;
    DialogueManager _dialogueManager;

    private void GetInstances()
    {
        _ppManager = GetComponent<PostProcessingManager>();
        if (_ppManager == null) { Debug.LogWarning("GameManager: Missing PostProcessingManager - Please attach the script to Transform of GameManager."); }
        _dialogueManager = GetComponent<DialogueManager>();
        if (_dialogueManager == null) { Debug.LogWarning("GameManager: Missing StoryManager - Please attach the script to Transform of GameManager."); }

        _queuePlayTimer = Instantiate(new Timer());
        _queuePlayTimer.Initialize(queueTimer);
    }

    #endregion

    #region Singleton Pattern

    public static GameManager Instance { get; private set; }

    private void SetupSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    #endregion

    #region Input

    public GameControlls gameInput;
    public event Action OnInputSetup;

    private void SetupInput()
    {
        gameInput = new GameControlls();
        gameInput.Enable();
        OnInputSetup?.Invoke();
    }

    #endregion

    private void Awake()
    {
        SetupSingleton();
    }

    private void Start()
    {
        SetupInput();
        GetInstances();
    }

    private void OnDestroy()
    {
        if (Instance == this && gameInput != null)
        {
            gameInput.Disable();
        }
    }

    #region Load Scene Logic
    public void LoadScene(int sceneIndex)
    {
        if (sceneIndex < 0 || sceneIndex > SceneManager.sceneCountInBuildSettings)
        {
            // If given index is invalid, load default level
            sceneIndex = 0;
        }

        SceneManager.LoadScene(sceneIndex);
    }

    public void LoadSceneNext()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        // Check if next index is valid to load, else reset it to zero
        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings) { nextSceneIndex = 0; }
        SceneManager.LoadScene(nextSceneIndex);
    }
    #endregion


    #region Mood & Dialogue Queue

    private int _currentQueueIndex = 0;
    private Timer _queuePlayTimer;
    private List<StoryAction> _queuedAction = new List<StoryAction>();

    [SerializeField]
    [Description("Timer for queued up moods and dialogue. The given amount will be waited, before playing both.")]
    private float queueTimer = 2;

    private void SetupQueueSystem()
    {
        _dialogueManager.OnDialogueFinished += ProgressQueue;
    }

    private void QueueAction(StoryAction action)
    {
        if (_dialogueManager.IsPlaying)
        {
            _queuedAction.Add(action);
        }
        else
        {
            ExecAction(action);
        }
    }

    private void ProgressQueue()
    {
        if (_queuedAction.Count <= 0) { return; }

        // Start Timer here...

        _currentQueueIndex++;

        if (_currentQueueIndex >= _queuedAction.Count)
        {
            _currentQueueIndex = 0;
            _queuedAction.Clear();
        }
    }

    private void ExecAction(StoryAction action)
    {
        TransitionMood(action.moodConfig, action.moodTransitionTime);
        StartDialogue(action.dialogue);
    }

    #endregion


    #region Actions

    public void TransitionMood(VolumeConfiguration volumeConfiguration, float transitionTime)
    {
        if (_ppManager != null) { _ppManager.SwitchMood(volumeConfiguration, transitionTime); }
    }

    public void StartDialogue(LocalizationTableCollection dialogue)
    {
        if (_dialogueManager != null) { }
    }

    #endregion

}