using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;

public class LocalizationManager : MonoBehaviour
{
    private bool active;

    private void Start()
    {
        if (GameManager.Instance != null) { GameManager.Instance.OnInputSetup += SubscribeInput; Debug.Log("huhu"); }
        Debug.Log("Through");
    }

    private void SubscribeInput()
    {
        GameManager.Instance.gameInput.Debug.SwitchLanguage.performed += OnSwitchLanguage;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnInputSetup -= SubscribeInput;
            GameManager.Instance.gameInput.Debug.SwitchLanguage.performed -= OnSwitchLanguage;
        }
    }

    public void ChangeLanguage(int localeID)
    {
        if (active) return;
        StartCoroutine(SetLocale(localeID));
    }

    void OnSwitchLanguage(InputAction.CallbackContext context)
    {
        int nextLocaleID = (LocalizationSettings.SelectedLocale.Identifier.Code == "en-GB") ? 0 : 1;
        ChangeLanguage(nextLocaleID);
    }

    IEnumerator SetLocale(int localeID)
    {
        active = true;

        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeID];

        active = false;
    }
}
