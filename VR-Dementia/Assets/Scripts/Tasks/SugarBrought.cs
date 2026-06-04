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

    private Timer timer;

    private void Start()
    {
        LocalizationSettings.SelectedLocaleChanged += HandleLocalizationChanged;

        if (LocalizationSettings.SelectedLocale != null)
        {
            HandleLocalizationChanged(LocalizationSettings.SelectedLocale);
        }

        timer = gameObject.AddComponent<Timer>();
        timer.OnTimerFinished += PlayPhrase;
    }

    private void OnDestroy()
    {
        timer.OnTimerFinished -= PlayPhrase;
        LocalizationSettings.SelectedLocaleChanged -= HandleLocalizationChanged;
        EventBus<OnPlayerSitDown>.OnEvent -= PlayerSatDown;
    }

    public override void StartTask()
    {
        base.StartTask();
        timer.Setup(julietteGoAheadDelay, false, true);
    }

    public void PlayPhrase()
    {
        EventBus<OnJulietteTalk>.Publish(new OnJulietteTalk(currentGoAhead));
        EventBus<OnPlayerSitDown>.OnEvent += PlayerSatDown;
    }

    private void PlayerSatDown(OnPlayerSitDown evt)
    {
        if (!evt.isSitting) { return; }
        Debug.LogWarning("sat down");
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
