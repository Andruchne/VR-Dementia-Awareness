using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum JoystickDirection { Any, Up, Horizontal }

[Serializable]
public struct InputStep
{
    public InputActionReference actionReference;
    public JoystickDirection requiredDirection;
    public GameObject locomotionComponent;
}

public class InputChecker : MonoBehaviour
{
    [SerializeField] private InputStep[] checkSteps;

    private int currentIndex = 0;

    private void OnEnable()
    {
        for (int i = 0; i < checkSteps.Length; i++)
        {
            checkSteps[i].locomotionComponent.SetActive(false);
        }

        SetupCurrentInput();
    }

    private void OnDisable()
    {
        if (checkSteps == null) { return; }

        if (currentIndex < checkSteps.Length && checkSteps[currentIndex].actionReference != null)
        {
            checkSteps[currentIndex].actionReference.action.performed -= InputPressed;
        }
    }

    private void SetupCurrentInput()
    {
        InputAction currentAction = checkSteps[currentIndex].actionReference.action;
        if (currentAction != null)
        {
            currentAction.Enable();
            currentAction.performed += InputPressed;
            checkSteps[currentIndex].locomotionComponent.SetActive(true);
        }
    }

    private void InputPressed(InputAction.CallbackContext context)
    {
        InputStep currentStep = checkSteps[currentIndex];
        InputAction currentAction = currentStep.actionReference.action;

        if (currentAction == null || currentAction.name != context.action.name) { return; }

        bool inputPassed = false;

        // Check specific joystick directions if the input is a Vector2 (Thumbstick)
        if (context.valueType == typeof(Vector2))
        {
            Vector2 axisValue = context.ReadValue<Vector2>();

            // We require a solid push (> 0.5) to avoid accidental touches
            if (currentStep.requiredDirection == JoystickDirection.Any && axisValue.magnitude > 0.5f) { inputPassed = true; }
            else if (currentStep.requiredDirection == JoystickDirection.Up && axisValue.y > 0.5f) { inputPassed = true; }
            else if (currentStep.requiredDirection == JoystickDirection.Horizontal && Mathf.Abs(axisValue.x) > 0.5f) { inputPassed = true; }
        }
        else
        {
            // Normal button press
            inputPassed = true;
        }

        if (inputPassed)
        {
            Debug.LogWarning($"Input ('{context.action.name}') was successfully performed!");

            currentAction.performed -= InputPressed;
            currentAction.Disable();

            currentIndex++;
            SetupCurrentInput();
        }
    }
}