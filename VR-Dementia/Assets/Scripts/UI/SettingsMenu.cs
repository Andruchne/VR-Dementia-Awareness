using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private GameObject settingsMenu;

    public void OnGoBack()
    {
        settingsMenu.SetActive(false);
    }

    public void OnSetDutchLocal()
    {
        GameManager.Instance.ChangeLocalization(0);
    }

    public void OnSetEnglishLocal()
    {
        GameManager.Instance.ChangeLocalization(1);
    }
}
