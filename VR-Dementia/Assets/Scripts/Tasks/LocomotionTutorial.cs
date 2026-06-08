using FMODUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class LocomotionTutorial : SimulationTask
{
    [Header("Feedback Settings")]
    [SerializeField] private EventReference successSound;
    [SerializeField] private Volume successVolume;
    [SerializeField] private float volumeTransitionTime = 1.0f;

    private int currentIndex;
    private Coroutine volumeRoutine;

    private void Start()
    {
        SetupCurrentLocomotion();

        if (successVolume != null) { successVolume.weight = 0.0f; }
    }

    private void OnDestroy()
    {
        EventBus<OnMoved>.OnEvent -= MovingPerformed;
        EventBus<OnTurned>.OnEvent -= TurnPerformed;
        EventBus<OnPalmMenuVisibilityChanged>.OnEvent -= PalmMenuVisibilityChanged;
    }

    public override void StartTask()
    {
        base.StartTask();
        SetupCurrentLocomotion();
    }

    private void MovingPerformed(OnMoved evt)
    {
        SetupCurrentLocomotion();
    }

    private void TurnPerformed(OnTurned evt)
    {
        SetupCurrentLocomotion();
    }

    private void SetupCurrentLocomotion()
    {
        switch (currentIndex)
        {
            case 1:
                {
                    EventBus<OnMoved>.OnEvent += MovingPerformed;
                    break;
                }
            case 2:
                {
                    EventBus<OnMoved>.OnEvent -= MovingPerformed;
                    EventBus<OnTurned>.OnEvent += TurnPerformed;
                    break;
                }
            case 3:
                {
                    EventBus<OnTurned>.OnEvent -= TurnPerformed;
                    EventBus<OnChangePalmMenuActive>.Publish(new OnChangePalmMenuActive(true));
                    EventBus<OnPalmMenuVisibilityChanged>.OnEvent += PalmMenuVisibilityChanged;
                    break;
                }
        }

        currentIndex++;
    }

    private void PalmMenuVisibilityChanged(OnPalmMenuVisibilityChanged evt)
    {
        if (evt.isVisible)
        {
            EventBus<OnPalmMenuVisibilityChanged>.OnEvent -= PalmMenuVisibilityChanged;
            TriggerSuccessFeedback();
            FinishTask();
        }
    }

    private void TriggerSuccessFeedback()
    {
        if (!successSound.IsNull) { RuntimeManager.PlayOneShot(successSound); }

        if (successVolume != null)
        {
            if (volumeRoutine != null) { StopCoroutine(volumeRoutine); }
            volumeRoutine = StartCoroutine(AnimateVolume());
        }
    }

    private IEnumerator AnimateVolume()
    {
        float speed = 1.0f / (volumeTransitionTime / 2.0f);

        while (successVolume.weight < 1.0f)
        {
            successVolume.weight = Mathf.MoveTowards(successVolume.weight, 1.0f, speed * Time.deltaTime);
            yield return null;
        }

        while (successVolume.weight > 0.0f)
        {
            successVolume.weight = Mathf.MoveTowards(successVolume.weight, 0.0f, speed * Time.deltaTime);
            yield return null;
        }
    }
}
