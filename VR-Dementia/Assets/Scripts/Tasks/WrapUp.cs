using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class WrapUp : SimulationTask
{
    [Header("Record Voicelines")]
    [SerializeField] float recordDelay = 2;
    [SerializeField] EventReference enRecord;
    [SerializeField] EventReference nlRecord;

    [Header("Humming Voiceline")]
    [SerializeField] float hummingDelay = 1;
    [SerializeField] EventReference humming;

    [Header("Fade Settings")]
    [SerializeField] Image blackScreenImage;
    [SerializeField] float fadeDuration = 8.0f;

    private EventReference currentRecord;

    private int sequenceIndex;
    private Timer timer;

    private void Start()
    {
        LocalizationSettings.SelectedLocaleChanged += HandleLocalizationChanged;

        if (LocalizationSettings.SelectedLocale != null)
        {
            HandleLocalizationChanged(LocalizationSettings.SelectedLocale);
        }
    }

    private void OnDestroy()
    {
        LocalizationSettings.SelectedLocaleChanged -= HandleLocalizationChanged;

        if (timer != null) { timer.OnTimerFinished -= PlayVoicelines; }

        EventBus<OnJulietteFinishedTalk>.OnEvent -= JulietteFinishedTalk;

        RuntimeManager.GetEventDescription(humming.Guid).unloadSampleData();
    }

    public override void StartTask()
    {
        base.StartTask();

        timer = gameObject.AddComponent<Timer>();
        timer.Setup(recordDelay, false, true);
        timer.OnTimerFinished += PlayVoicelines;

        EventBus<OnJulietteFinishedTalk>.OnEvent += JulietteFinishedTalk;
    }

    private void JulietteFinishedTalk(OnJulietteFinishedTalk evt)
    {
        timer.Setup(hummingDelay, false, true);
    }

    private void PlayVoicelines()
    {
        switch (sequenceIndex)
        {
            case 0:
                {
                    EventBus<OnJulietteTalk>.Publish(new OnJulietteTalk(currentRecord));
                    break;
                }
            case 1:
                {
                    EventBus<OnJulietteTalk>.Publish(new OnJulietteTalk(humming));
                    EventBus<OnTransitionScreen>.Publish(new OnTransitionScreen(1.0f, fadeDuration));
                    break;
                }
        }

        sequenceIndex++;
    }

    private void HandleLocalizationChanged(Locale newLocale)
    {
        if (newLocale.Identifier.Code.StartsWith("en"))
        {
            currentRecord = enRecord;
        }
        else if (newLocale.Identifier.Code.StartsWith("nl"))
        {
            currentRecord = nlRecord;
        }
    }
}