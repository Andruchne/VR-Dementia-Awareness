using UnityEngine;

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

    public void OnLoadMainMenu()
    {
        GameManager.Instance.LoadSceneNext();
    }
}
