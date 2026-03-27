using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject exitConfirm;
    [SerializeField] private GameObject settingsWindow;

    public void OnExitClicked()
    {
        exitConfirm.SetActive(true);
    }

    public void OnSettingsClicked()
    {
        settingsWindow.SetActive(true);
    }

    public void OnStartClicked()
    {
        GameManager.Instance.LoadSceneNext();
    }}
