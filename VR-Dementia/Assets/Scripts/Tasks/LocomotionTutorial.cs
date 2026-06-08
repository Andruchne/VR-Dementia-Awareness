using FMODUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class LocomotionTutorial : SimulationTask
{
    [Header("Feedback Settings")]
    [SerializeField] EventReference successSound;
    [SerializeField] Volume successVolume;
    [SerializeField] float volumeTransitionTime = 1.0f;

    [Header("Locomotion Activation Order")]
    [SerializeField] Components[] locomotionComponents;

    private int currentIndex;
    private Coroutine volumeRoutine;

    private void Start()
    {
        // Deactivate all listed locomotion components
        EventBus<OnChangePalmMenuActive>.Publish(new OnChangePalmMenuActive(false));
        for (int i = 0; i < locomotionComponents.Length; i++)
        {
            for (int a = 0; a < locomotionComponents[i].components.Length; a++)
            {
                locomotionComponents[i].components[a].SetActive(false);
            }
        }

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
        AdvanceCurrentLocomotion();
    }

    private void MovingPerformed(OnMoved evt)
    {
        AdvanceCurrentLocomotion();
    }

    private void TurnPerformed(OnTurned evt)
    {
        AdvanceCurrentLocomotion();
    }

    private void AdvanceCurrentLocomotion()
    {
        Debug.LogWarning(currentIndex);

        if (currentIndex < locomotionComponents.Length)
        {
            for (int i = 0; i < locomotionComponents[currentIndex].components.Length; i++)
            {
                locomotionComponents[currentIndex].components[i].SetActive(true);
            }
        }

        switch (currentIndex)
        {
            case 0:
                {
                    EventBus<OnTurned>.OnEvent += TurnPerformed;
                    break;
                }
            case 1:
                {
                    EventBus<OnMoved>.OnEvent += MovingPerformed;
                    EventBus<OnTurned>.OnEvent -= TurnPerformed;
                    break;
                }
            case 2:
                {
                    EventBus<OnMoved>.OnEvent -= MovingPerformed;
                    EventBus<OnChangePalmMenuActive>.Publish(new OnChangePalmMenuActive(true));
                    EventBus<OnPalmMenuVisibilityChanged>.OnEvent += PalmMenuVisibilityChanged;
                    break;
                }
            case 3:
                {
                    EventBus<OnPalmMenuVisibilityChanged>.OnEvent -= PalmMenuVisibilityChanged;
                    FinishTask();
                    break;
                }
        }

        if (currentIndex > 0) { TriggerSuccessFeedback(); }
        currentIndex++;
    }

    private void PalmMenuVisibilityChanged(OnPalmMenuVisibilityChanged evt)
    {
        if (evt.isVisible)
        {
            AdvanceCurrentLocomotion();
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
