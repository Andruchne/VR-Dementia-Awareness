using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;

public class LocalizationManager : MonoBehaviour
{
    private bool isUpdating;

    private void Awake()
    {
        if (GameManager.Instance != null) { GameManager.Instance.OnInputSetup += SubscribeInput;}
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

    public void ChangeLanguage(int localeId)
    {
        if (isUpdating) return;
        StartCoroutine(SetLocale(localeId));
    }

    private void OnSwitchLanguage(InputAction.CallbackContext context)
    {
        int nextLocaleId = (LocalizationSettings.SelectedLocale.Identifier.Code == "en-GB") ? 0 : 1;
        ChangeLanguage(nextLocaleId);
    }

    private IEnumerator SetLocale(int localeId)
    {
        isUpdating = true;

        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeId];

        isUpdating = false;
    }
}
