using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;

public class LocalizationManager : MonoBehaviour
{
    private bool isUpdating;

    private void Awake()
    {
        if (GameManager.Instance != null) { GameManager.Instance.OnInputSetup += SubscribeInput; }
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

    public void ChangeLanguage(string localeCode)
    {
        if (isUpdating) { return; }
        StartCoroutine(SetLocale(localeCode));
    }

    private void OnSwitchLanguage(InputAction.CallbackContext context)
    {
        string nextLocaleCode = (LocalizationSettings.SelectedLocale.Identifier.Code == "en-GB") ? "nl-NL" : "en-GB";
        ChangeLanguage(nextLocaleCode);
    }

    private IEnumerator SetLocale(string localeCode)
    {
        isUpdating = true;

        yield return LocalizationSettings.InitializationOperation;

        var targetLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
        if (targetLocale != null)
        {
            LocalizationSettings.SelectedLocale = targetLocale;
        }

        isUpdating = false;
    }
}