using UnityEngine;

/// <summary>
/// Used in the menu canvas (Handheld & Main Menu).
/// Either closes App or load next scene.
/// </summary>
public class ExitConfirmMenu : MonoBehaviour
{
    [SerializeField] GameObject mainMenu;

    public void OnExitConfirm()
    {
        Debug.Log("Closing App...");
        Application.Quit();
    }

    public void OnStayConfirm()
    {
        gameObject.SetActive(false);
        mainMenu.SetActive(true);
    }
}
