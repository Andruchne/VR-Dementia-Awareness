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
    [SerializeField] GameObject indicatorHUD;
    [SerializeField] EventReference enJulietteComeIn;
    [SerializeField] EventReference nlJulietteComeIn;

    private EventReference currentGreeting;
    private EventReference currentComeIn;

    private int sequenceIndex;
    private Timer timer;

    private void Start()
    {
        indicatorHUD.SetActive(false);

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
        timer.Setup(julietteGreetingDelay, true, true);
    }

    private void GuestEntersBuilding(OnEnterBuilding evt)
    {
        timer.StopTimer();
        indicatorHUD.SetActive(false);

        FinishTask();
    }

    private void PlaySequence()
    {
        switch (sequenceIndex)
        {
            case 0:
                {
                    if (!currentGreeting.IsNull) { RuntimeManager.PlayOneShot(currentGreeting); }
                    // Set wait time for the reminder sequences to show up
                    timer.SetWaitTime(remindAfterSeconds);
                    break;
                }
            case 1:
                {
                    if (!currentGreeting.IsNull) { RuntimeManager.PlayOneShot(currentComeIn); }
                    break;
                }
            case 2:
                {
                    if (indicatorHUD != null) { indicatorHUD.SetActive(true); }
                    timer.StopTimer();
                    break;
                }
        }
        sequenceIndex++;
        Debug.LogWarning("blabl: " + sequenceIndex);
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
