using FMODUnity;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class JulietteDoorOpen : SimulationTask
{
    [Header("Juliette Greeting Voicelines")]
    [SerializeField] float julietteGreetingDelay = 2;
    [SerializeField] EventReference enJulietteGreeting;
    [SerializeField] EventReference nlJulietteGreeting;

    [Header("Reminder Indicator Settings")]
    [SerializeField] float remindAfterSeconds = 60;
    [SerializeField] EventReference enJulietteComeIn;
    [SerializeField] EventReference nlJulietteComeIn;

    private EventReference currentGreeting;
    private EventReference currentComeIn;

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

        EventBus<OnEnterBuilding>.OnEvent += GuestEntersBuilding;
    }

    private void OnDestroy()
    {
        timer.OnTimerFinished -= PlaySequence;
        EventBus<OnEnterBuilding>.OnEvent -= GuestEntersBuilding;
    }

    public override void StartTask()
    {
        base.StartTask();
        // Set the delay time for the grandma to show up & greet
        timer.Setup(julietteGreetingDelay, false, true);
        EventBus<OnUpdateTask>.Publish(new OnUpdateTask());
        EventBus<OnOpenDoorAnim>.Publish(new OnOpenDoorAnim());
    }

    private void GuestEntersBuilding(OnEnterBuilding evt)
    {
        timer.StopTimer();

        EventBus<OnShowIndicator>.Publish(new OnShowIndicator(false));

        FinishTask();
    }

    private void SetReminderTimer(OnJulietteFinishedTalk evt)
    {
        if (isFinished) { return; }

        timer.Setup(remindAfterSeconds, true, true);
    }

    private void PlaySequence()
    {
        if (isFinished) { return; }

        switch (sequenceIndex)
        {
            case 0:
                {
                    if (!currentGreeting.IsNull) { EventBus<OnJulietteTalk>.Publish(new OnJulietteTalk(currentGreeting)); }
                    // Set wait time for the reminder sequences to show up
                    EventBus<OnJulietteFinishedTalk>.OnEvent += SetReminderTimer;
                    break;
                }
            case 1:
                {
                    if (!currentComeIn.IsNull) { EventBus<OnJulietteTalk>.Publish(new OnJulietteTalk(currentComeIn)); }
                    break;
                }
            case 2:
                {
                    EventBus<OnShowIndicator>.Publish(new OnShowIndicator(true, IndicatorHUDs.EnterHome));
                    timer.StopTimer();
                    break;
                }
        }

        sequenceIndex++;
    }


    private void HandleLocalizationChanged(Locale newLocale)
    {
        if (newLocale.Identifier.Code.StartsWith("en"))
        {
            currentGreeting = enJulietteGreeting;
            currentComeIn = enJulietteComeIn;
        }
        else if (newLocale.Identifier.Code.StartsWith("nl"))
        {
            currentGreeting = nlJulietteGreeting;
            currentComeIn = nlJulietteComeIn;
        }
    }
}
