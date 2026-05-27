using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

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

        // Deactivate handheld menu - it will be activated during InputChecker activeness
        EventBus<OnChangePalmMenuActive>.Publish(new OnChangePalmMenuActive(false));
        for (int i = 0; i < leftHandComponents.Length; i++)
        {
            leftHandComponents[i].gameObject.SetActive(false);
        }

        timer = gameObject.AddComponent<Timer>();
        timer.Setup(3, false, true);
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

                    timer.StopTimer();
                    break;
                }
        }

        reminderIndex++;
    }
}