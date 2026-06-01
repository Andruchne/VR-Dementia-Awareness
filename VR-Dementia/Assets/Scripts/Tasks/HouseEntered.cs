using FMODUnity;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class HouseEntered : SimulationTask
{
    [Header("Come Sit Voicelines")]
    [SerializeField] float julietteSitDownDelay = 2;
    [SerializeField] EventReference enComeSitDown;
    [SerializeField] EventReference nlComeSitDown;

    private EventReference currentSitDown;

    private void Start()
    {
        LocalizationSettings.SelectedLocaleChanged += HandleLocalizationChanged;
        EventBus<OnEnterBuilding>.OnEvent += BuildingEntered;
    }

    private void OnDestroy()
    {
        LocalizationSettings.SelectedLocaleChanged -= HandleLocalizationChanged;
        EventBus<OnEnterBuilding>.OnEvent -= BuildingEntered;
    }

    private void BuildingEntered(OnEnterBuilding evt)
    {
        EventBus<OnJulietteTalk>.Publish(new OnJulietteTalk(currentSitDown));
        FinishTask();
    }

    private void HandleLocalizationChanged(Locale newLocale)
    {
        if (newLocale.Identifier.Code.StartsWith("en"))
        {
            currentSitDown = enComeSitDown;
        }
        else if (newLocale.Identifier.Code.StartsWith("nl"))
        {
            currentSitDown = nlComeSitDown;
        }
    }
}
