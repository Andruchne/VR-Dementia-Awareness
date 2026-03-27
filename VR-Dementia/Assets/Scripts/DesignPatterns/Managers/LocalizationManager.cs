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

    public void ChangeLanguage(int localeID)
    {
        if (isUpdating) return;
        StartCoroutine(SetLocale(localeID));
    }

    private void OnSwitchLanguage(InputAction.CallbackContext context)
    {
        int nextLocaleID = (LocalizationSettings.SelectedLocale.Identifier.Code == "en-GB") ? 0 : 1;
        ChangeLanguage(nextLocaleID);
    }

    private IEnumerator SetLocale(int localeID)
    {
        isUpdating = true;

        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeID];

        isUpdating = false;
    }
}
