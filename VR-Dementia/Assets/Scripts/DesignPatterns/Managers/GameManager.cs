using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
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
        //SetupSingleton();
        SetupInput();
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


}