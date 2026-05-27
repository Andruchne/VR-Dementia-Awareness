using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Rendering;
using FMODUnity;

public class InputChecker : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] InputStep[] checkSteps;

    [Header("Feedback Settings")]
    [SerializeField] private EventReference successSound;
    [SerializeField] private Volume successVolume;
    [SerializeField] private float volumeTransitionTime = 1f;

    private int currentIndex = 0;
    private Coroutine volumeRoutine;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < checkSteps.Length; i++)
        {
            for (int a = 0; a < checkSteps[i].locomotionComponent.Length; a++)
            {
                checkSteps[i].locomotionComponent[a].SetActive(false);
            }
        }

        if (successVolume != null) { successVolume.weight = 0f; }

        EventBus<OnStartSimulation>.OnEvent += InitiateCheck;
    }

    private void OnDestroy()
    {
        EventBus<OnStartSimulation>.OnEvent -= InitiateCheck;

        if (checkSteps == null || currentIndex >= checkSteps.Length) { return; }
        if (currentIndex < checkSteps.Length && checkSteps[currentIndex].actionReference != null)
        {
            checkSteps[currentIndex].actionReference.action.performed -= InputPressed;
        }
    }

    private void CheckPalmMenu()
    {
        EventBus<OnChangePalmMenuActive>.Publish(new OnChangePalmMenuActive(true));
        EventBus<OnPalmMenuVisibilityChanged>.OnEvent += PalmMenuVisibilityChanged;
    }

    private void PalmMenuVisibilityChanged(OnPalmMenuVisibilityChanged evt)
    {
        if (evt.isVisible)
        {
            EventBus<OnPalmMenuVisibilityChanged>.OnEvent -= PalmMenuVisibilityChanged;
            TriggerSuccessFeedback();
        }
    }

    private void InitiateCheck(OnStartSimulation evt)
    {
        SetupCurrentInput();
    }

    private void SetupCurrentInput()
    {
        if (currentIndex >= checkSteps.Length) 
        {
            CheckPalmMenu();
            return; 
        }

        InputAction currentAction = checkSteps[currentIndex].actionReference.action;
        if (currentAction != null)
        {
            currentAction.Enable();
            currentAction.performed += InputPressed;

            for (int i = 0; i < checkSteps[currentIndex].locomotionComponent.Length; i++)
            {
                checkSteps[currentIndex].locomotionComponent[i].SetActive(true);
            }
        }
    }

    private void InputPressed(InputAction.CallbackContext context)
    {
        InputStep currentStep = checkSteps[currentIndex];
        InputAction currentAction = currentStep.actionReference.action;

        if (currentAction == null || currentAction.name != context.action.name) { return; }

        bool inputPassed = false;

        // Check specific joystick directions if the input is a Vector2
        if (context.valueType == typeof(Vector2))
        {
            Vector2 axisValue = context.ReadValue<Vector2>();

            if (currentStep.requiredDirection == JoystickDirection.Any && axisValue.magnitude > 0.5f) { inputPassed = true; }
            else if (currentStep.requiredDirection == JoystickDirection.Up && axisValue.y > 0.5f) { inputPassed = true; }
            else if (currentStep.requiredDirection == JoystickDirection.Horizontal && Mathf.Abs(axisValue.x) > 0.5f) { inputPassed = true; }
        }
        else
        {
            inputPassed = true;
        }

        if (inputPassed)
        {
            Debug.LogWarning($"Input ('{context.action.name}') was successfully performed!");

            TriggerSuccessFeedback();

            currentAction.performed -= InputPressed;
            currentAction.Disable();

            currentIndex++;
            SetupCurrentInput();
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
        float speed = 1f / (volumeTransitionTime / 2f);

        while (successVolume.weight < 1f)
        {
            successVolume.weight = Mathf.MoveTowards(successVolume.weight, 1f, speed * Time.deltaTime);
            yield return null;
        }

        while (successVolume.weight > 0f)
        {
            successVolume.weight = Mathf.MoveTowards(successVolume.weight, 0f, speed * Time.deltaTime);
            yield return null;
        }
    }
}