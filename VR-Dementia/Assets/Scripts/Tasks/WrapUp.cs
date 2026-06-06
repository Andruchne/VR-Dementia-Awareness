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
    [SerializeField] float fadeDuration = 0.5f;

    private EventReference currentRecord;

    private int sequenceIndex;
    private Timer timer;
    private Timer fadeTimer;

    private Color fadeStartColor;
    private Color fadeTargetColor;
    private float currentFadeDuration;

    private void Start()
    {
        LocalizationSettings.SelectedLocaleChanged += HandleLocalizationChanged;

        if (LocalizationSettings.SelectedLocale != null)
        {
            HandleLocalizationChanged(LocalizationSettings.SelectedLocale);
        }

        if (blackScreenImage != null)
        {
            Color c = blackScreenImage.color;
            c.a = 0f;
            blackScreenImage.color = c;
        }
    }

    private void OnDestroy()
    {
        LocalizationSettings.SelectedLocaleChanged -= HandleLocalizationChanged;

        if (timer != null) { timer.OnTimerFinished -= PlayVoicelines; }

        if (fadeTimer != null)
        {
            fadeTimer.OnTimerRunning -= UpdateFade;
            fadeTimer.OnTimerFinished -= FinishFade;
        }

        EventBus<OnJulietteFinishedTalk>.OnEvent -= JulietteFinishedTalk;

        RuntimeManager.GetEventDescription(humming.Guid).unloadSampleData();
    }

    public override void StartTask()
    {
        base.StartTask();

        timer = gameObject.AddComponent<Timer>();
        timer.Setup(recordDelay, false, true);
        timer.OnTimerFinished += PlayVoicelines;

        fadeTimer = gameObject.AddComponent<Timer>();
        fadeTimer.OnTimerRunning += UpdateFade;
        fadeTimer.OnTimerFinished += FinishFade;

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
                    StartFadeToBlack(fadeDuration);
                    break;
                }
        }

        sequenceIndex++;
    }

    private void StartFadeToBlack(float duration)
    {
        if (blackScreenImage == null) { return; }

        currentFadeDuration = duration;
        fadeStartColor = blackScreenImage.color;
        fadeTargetColor = new Color(fadeStartColor.r, fadeStartColor.g, fadeStartColor.b, 1f);

        fadeTimer.Setup(duration, false, true);
    }

    private void UpdateFade()
    {
        if (blackScreenImage == null || currentFadeDuration <= 0) { return; }

        float elapsedTime = currentFadeDuration - fadeTimer.GetTimeLeft();
        blackScreenImage.color = Color.Lerp(fadeStartColor, fadeTargetColor, elapsedTime / currentFadeDuration);
    }

    private void FinishFade()
    {
        if (blackScreenImage != null) { blackScreenImage.color = fadeTargetColor; }
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