using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public VolumeConfiguration volumeconfig;

    #region Variables and Instances

    private PostProcessingManager _ppManager;
    private DialogueManager _dialogueManager;
    private LocalizationManager _localManager;
    private VoiceInteractionManager _voiceInterManager;

    // To subscribe to processing events
    public VoiceInteractionManager VoiceInterManager => _voiceInterManager;

    private void GetInstances()
    {
        _ppManager = GetComponent<PostProcessingManager>();
        if (_ppManager == null) { Debug.LogWarning("GameManager: Missing PostProcessingManager - Please attach the script to Transform of GameManager."); }
        _dialogueManager = GetComponent<DialogueManager>();
        if (_dialogueManager == null) { Debug.LogWarning("GameManager: Missing StoryManager - Please attach the script to Transform of GameManager."); }
        _localManager = GetComponent<LocalizationManager>();
        if (_localManager == null) { Debug.LogWarning("GameManager: Missing LocalizationManager - Please attach the script to Transform of GameManager."); }
        _voiceInterManager = GetComponentInChildren<VoiceInteractionManager>();
        if (_voiceInterManager == null) { Debug.LogWarning("GameManager: Missing VoiceInteractionManager - Please attach AI Prefab as child of GameManager."); }
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
        GetInstances();

        SetupInput();

        ChangeLocalization("en-GB");
    }

    private void OnDestroy()
    {
        if (Instance == this && gameInput != null)
        {
            gameInput.Disable();
        }
    }

    #region Dialogue

    public void StartRecordingVoice()
    {
        if (_voiceInterManager == null) { return; }

        _voiceInterManager.StartRecording();
    }

    public void StopRecordingVoice()
    {
        if (_voiceInterManager == null) { return; }

        _voiceInterManager.StopRecordingAndProcess();
    }

    public void DiscardRecordingVoice()
    {
        if (_voiceInterManager == null) { return; }
        _voiceInterManager.DiscardRecording();
    }

    #endregion


    #region Localization Settings

    public void ChangeLocalization(string localIndex)
    {
        if (_localManager == null) { return; }

        _localManager.ChangeLanguage(localIndex);
    }

    #endregion


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

    public void SetMoodPercentage(Mood mood, int percentage)
    {
        if (_ppManager != null) { _ppManager.SetMoodPercentage(mood, percentage); }
    }

    #endregion


    #region Actions

    public void TransitionMood(VolumeConfiguration volumeConfiguration, float transitionTime)
    {
        if (_ppManager != null) { _ppManager.SwitchMood(volumeConfiguration, transitionTime); }
    }

    #endregion

}