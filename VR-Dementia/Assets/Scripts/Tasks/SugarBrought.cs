using FMODUnity;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class SugarBrought : SimulationTask
{
    [Header("Go Ahead Voicelines")]
    [SerializeField] float julietteGoAheadDelay = 0;
    [SerializeField] EventReference enGoAhead;
    [SerializeField] EventReference nlGoAhead;

    private EventReference currentGoAhead;

    private void Start()
    {
        LocalizationSettings.SelectedLocaleChanged += HandleLocalizationChanged;
        EventBus<OnPlayerSitDown>.OnEvent += PlayerSatDown;
    }

    private void OnDestroy()
    {
        LocalizationSettings.SelectedLocaleChanged -= HandleLocalizationChanged;
        EventBus<OnPlayerSitDown>.OnEvent -= PlayerSatDown;
    }

    public override void StartTask()
    {
        base.StartTask();
        EventBus<OnJulietteTalk>.Publish(new OnJulietteTalk(currentGoAhead));
    }

    private void PlayerSatDown(OnPlayerSitDown evt)
    {
        FinishTask();
    }

    private void HandleLocalizationChanged(Locale newLocale)
    {
        if (newLocale.Identifier.Code.StartsWith("en"))
        {
            currentGoAhead = enGoAhead;
        }
        else if (newLocale.Identifier.Code.StartsWith("nl"))
        {
            currentGoAhead = nlGoAhead;
        }
    }
}
