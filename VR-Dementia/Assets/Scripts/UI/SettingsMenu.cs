using UnityEngine;

/// <summary>
/// Used for the settings menu (Handheld and Main Menu)
/// More functions can be added, depending on the amount of options.
/// </summary>
public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private GameObject settingsMenu;

    public void OnGoBack()
    {
        settingsMenu.SetActive(false);
    }

    public void OnSetDutchLocal()
    {
        GameManager.Instance.ChangeLocalization("nl-NL");
    }

    public void OnSetEnglishLocal()
    {
        GameManager.Instance.ChangeLocalization("en-GB");
    }
}
