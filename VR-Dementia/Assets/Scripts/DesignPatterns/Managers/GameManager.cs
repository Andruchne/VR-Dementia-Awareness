using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Singleton Pattern

    public static GameManager Instance { get; private set; }

    private void ManageSingleton()
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

    private void ManageInput(bool activate)
    {
        if (activate) { gameInput.Enable(); }
        else { gameInput.Disable(); }
    }

    #endregion

    private void Awake()
    {
        // Singleton Setup
        ManageSingleton();

        // Input Setup
        gameInput = new GameControlls();
        ManageInput(true);
    }

    private void OnDestroy()
    {
        if (Instance == this && gameInput != null)
        {
            ManageInput(false);
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