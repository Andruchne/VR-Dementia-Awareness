using FMODUnity;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class AskGetSugar : SimulationTask
{
    [Header("Forgot Sugar Voicelines")]
    [SerializeField] float forgotSugarDelay = 2;
    [SerializeField] EventReference enForgotSugar;
    [SerializeField] EventReference nlForgotSugar;

    [Header("Bring Sugar Sam Settings")]
    [SerializeField] float bringSugarSamDelay = 2;
    [SerializeField] EventReference enBringSugarSam;
    [SerializeField] EventReference nlBringSugarSam;

    [Header("Sorry Robin Settings")]
    [SerializeField] float sorryRobinDelay = 1;
    [SerializeField] EventReference enSorryRobin;
    [SerializeField] EventReference nlSorryRobin;

    [Header("Reminder Indicator Settings")]
    [SerializeField] float remindAfterSeconds = 60;
    [SerializeField] GameObject cabinetPosIndicator;
    [SerializeField] EventReference enSmallCabinet;
    [SerializeField] EventReference nlSmallCabinet;
    [SerializeField] GameObject indicatorHUD;

    [Header("Grab Tutorial")]
    [SerializeField] GameObject grabTutorialPicture;


    private EventReference currentForgotSugar;
    private EventReference currentBringSugarSam;
    private EventReference currentSorryRobin;
    private EventReference currentSmallCabinet;

    private int sequenceIndex;
    private Timer timer;

    private void Start()
    {
        // Setup localization changes
        LocalizationSettings.SelectedLocaleChanged += HandleLocalizationChanged;
        if (LocalizationSettings.SelectedLocale != null)
        {
            HandleLocalizationChanged(LocalizationSettings.SelectedLocale);
        }

        timer = gameObject.AddComponent<Timer>();
        timer.OnTimerFinished += PlaySequence;

        EventBus<OnJulietteSitDown>.OnEvent += JulietteSatDown;
        EventBus<OnSugarPlacedDown>.OnEvent += SugarPlaced;
    }

    private void OnDestroy()
    {
        timer.OnTimerFinished -= PlaySequence;
        EventBus<OnJulietteSitDown>.OnEvent -= JulietteSatDown;
        EventBus<OnJulietteFinishedTalk>.OnEvent -= JulietteFinishedTalk;
        EventBus<OnSugarPlacedDown>.OnEvent -= SugarPlaced;
    }

    private void JulietteSatDown(OnJulietteSitDown evt)
    {
        timer.Setup(forgotSugarDelay, false, true);
        EventBus<OnJulietteFinishedTalk>.OnEvent += JulietteFinishedTalk;
    }

    private void JulietteFinishedTalk(OnJulietteFinishedTalk evt)
    {
        switch (sequenceIndex)
        {
            case 0:
                {
                    timer.Setup(bringSugarSamDelay, false, true);
                    break;
                }
            case 1:
                {
                    timer.Setup(sorryRobinDelay, false, true);
                    break;
                }
            case 3:
                {
                    timer.Setup(remindAfterSeconds, true, true);
                    break;
                }
        }
    }

    private void PlaySequence()
    {
        switch (sequenceIndex)
        {
            case 0:
                {
                    if (!currentForgotSugar.IsNull) { EventBus<OnJulietteTalk>.Publish(new OnJulietteTalk(currentForgotSugar)); }
                    break;
                }
            case 1:
                {
                    if (!currentBringSugarSam.IsNull) { EventBus<OnJulietteTalk>.Publish(new OnJulietteTalk(currentBringSugarSam)); }
                    break;
                }
            case 2:
                {
                    if (!currentSorryRobin.IsNull) { EventBus<OnJulietteTalk>.Publish(new OnJulietteTalk(currentSorryRobin)); }
                    break;
                }
            case 3:
                {
                    if (cabinetPosIndicator != null) { cabinetPosIndicator.SetActive(true); }
                    if (!currentSmallCabinet.IsNull) { EventBus<OnJulietteTalk>.Publish(new OnJulietteTalk(currentSmallCabinet)); }
                    break;
                }
            case 4:
                {
                    if (indicatorHUD != null) 
                    { 
                        indicatorHUD.SetActive(true);
                        indicatorHUD.transform.parent.gameObject.SetActive(true);
                    }
                    timer.StopTimer();
                    break;
                }
        }

        sequenceIndex++;
    }

    private void SugarPlaced(OnSugarPlacedDown evt)
    {
        EventBus<OnJulietteFinishedTalk>.OnEvent -= JulietteFinishedTalk;

        if (indicatorHUD != null)
        {
            indicatorHUD.SetActive(false);
            indicatorHUD.transform.parent.gameObject.SetActive(false);
        }

        FinishTask();
    }

    private void HandleLocalizationChanged(Locale newLocale)
    {
        if (newLocale.Identifier.Code.StartsWith("en"))
        {
            currentForgotSugar = enForgotSugar;
            currentBringSugarSam = enBringSugarSam;
            currentSorryRobin = enSorryRobin;
        }
        else if (newLocale.Identifier.Code.StartsWith("nl"))
        {
            currentForgotSugar = nlForgotSugar;
            currentBringSugarSam = nlBringSugarSam;
            currentSorryRobin = nlSorryRobin;
        }
    }
}
