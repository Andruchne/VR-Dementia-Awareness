using UnityEngine;

/// <summary>
/// Used in the menu canvas (Handheld & Main Menu).
/// Either closes App or load next scene.
/// </summary>
public class ExitConfirmMenu : MonoBehaviour
{
    [SerializeField] private GameObject exitConfirm;

    public void OnExitConfirm()
    {
        Application.Quit();
        Debug.Log("Closing App...");
    }

    public void OnStayConfirm()
    {
        exitConfirm.SetActive(false);
    }

    public void OnLoadNextLevel()
    {
        GameManager.Instance.LoadSceneNext();
    }
}
