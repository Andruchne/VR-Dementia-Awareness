using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LocalizationManager : MonoBehaviour
{
    private bool isUpdating;

    public void ChangeLanguage(string localeCode)
    {
        if (isUpdating) { return; }
        StartCoroutine(SetLocale(localeCode));
    }

    private IEnumerator SetLocale(string localeCode)
    {
        isUpdating = true;

        yield return LocalizationSettings.InitializationOperation;

        Locale targetLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
        if (targetLocale != null)
        {
            LocalizationSettings.SelectedLocale = targetLocale;
        }

        isUpdating = false;
    }
}