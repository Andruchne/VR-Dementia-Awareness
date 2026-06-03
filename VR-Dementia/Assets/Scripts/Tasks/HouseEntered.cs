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

    private Timer timer;

    private void Start()
    {
        LocalizationSettings.SelectedLocaleChanged += HandleLocalizationChanged;
        EventBus<OnEnterBuilding>.OnEvent += BuildingEntered;

        if (LocalizationSettings.SelectedLocale != null)
        {
            HandleLocalizationChanged(LocalizationSettings.SelectedLocale);
        }

        timer = gameObject.AddComponent<Timer>();
        timer.OnTimerFinished += PlayVoice;
    }

    private void OnDestroy()
    {
        LocalizationSettings.SelectedLocaleChanged -= HandleLocalizationChanged;
        EventBus<OnEnterBuilding>.OnEvent -= BuildingEntered;
        timer.OnTimerFinished -= PlayVoice;
    }

    private void BuildingEntered(OnEnterBuilding evt)
    {
        timer.Setup(julietteSitDownDelay, false, true);
    }

    private void PlayVoice()
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
