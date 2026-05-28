using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PressPhone : SimulationTask
{
    [Header("Phone Setup")]
    [SerializeField] private Transform phone;

    [Header("Hand Setup")]
    [SerializeField] private Transform[] leftHandComponents;

    [Header("Hint Setup")]
    [SerializeField] GameObject indicatorHUD;
    [SerializeField] Image sendButtonImage;
    private Timer timer;
    private int reminderIndex;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1.0f);

        indicatorHUD.SetActive(false);

        // Deactivate handheld menu - it will be activated during InputChecker activeness
        EventBus<OnChangePalmMenuActive>.Publish(new OnChangePalmMenuActive(false));
        for (int i = 0; i < leftHandComponents.Length; i++)
        {
            leftHandComponents[i].gameObject.SetActive(false);
        }

        timer = gameObject.AddComponent<Timer>();
        timer.Setup(3, true, true);
        timer.OnTimerFinished += ShowReminder;
    }

    private void OnDestroy()
    {
        timer.OnTimerFinished -= ShowReminder;
    }

    public void SendButtonPressed()
    {
        HidePhone();
    }

    private void HidePhone()
    {
        EventBus<OnStartSimulation>.Publish(new OnStartSimulation());

        indicatorHUD.SetActive(false);
        phone.gameObject.SetActive(false);
        for (int i = 0; i < leftHandComponents.Length; i++)
        {
            leftHandComponents[i].gameObject.SetActive(true);
        }
        FinishTask();
    }

    private void ShowReminder()
    {
        switch (reminderIndex)
        {
            case 0:
                {
                    if (indicatorHUD != null) { indicatorHUD.SetActive(true); }
                    break;
                }
            case 1:
                {
                    StartCoroutine(AnimateSendButtonImage());
                    timer.StopTimer();
                    break;
                }
        }

        reminderIndex++;
    }

    private IEnumerator AnimateSendButtonImage()
    {
        Color baseColor = sendButtonImage.color;
        float pulseSpeed = 5f;

        while (true)
        {
            baseColor.a = 0.7f + (Mathf.Sin(Time.time * pulseSpeed) * 0.3f);
            sendButtonImage.color = baseColor;

            yield return null;
        }
    }
}