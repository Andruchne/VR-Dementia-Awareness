using Meta.XR.InputActions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class ControllerHandSwitch : MonoBehaviour
{
    [SerializeField] private InputActionReference[] checkInput;
    private bool[] inputPressed;

    private bool combinationCompleted;

    private void Start()
    {
        if (checkInput != null)
        {
            inputPressed = new bool[checkInput.Length];
        }
    }

    private void Update()
    {
        CheckControllerSwap();
    }

    private void CheckControllerSwap()
    {
        if (combinationCompleted && OVRManager.instance.controllerDrivenHandPosesType == OVRManager.ControllerDrivenHandPosesType.None)
        {
            OVRManager.instance.controllerDrivenHandPosesType = OVRManager.ControllerDrivenHandPosesType.Natural;
        }
    }

    private void OnEnable()
    {
        if (checkInput == null) { return; }

        foreach (InputActionReference inputRef in checkInput)
        {
            if (inputRef != null && inputRef.action != null)
            {
                // Wichtig bei Custom Actions: Sie müssen explizit aktiviert werden
                inputRef.action.Enable();
                inputRef.action.performed += InputPressed;
            }
        }
    }

    private void OnDisable()
    {
        if (checkInput == null) { return; }

        foreach (InputActionReference inputRef in checkInput)
        {
            if (inputRef != null && inputRef.action != null)
            {
                inputRef.action.performed -= InputPressed;
            }
        }
    }

    private void InputPressed(InputAction.CallbackContext context)
    {
        if (combinationCompleted) { return; }

        for (int i = 0; i < checkInput.Length; i++)
        {
            if (checkInput[i] != null && checkInput[i].action.name == context.action.name)
            {
                if (!inputPressed[i])
                {
                    inputPressed[i] = true;
                    Debug.Log($"Input {i} ('{context.action.name}') was pressed!");
                    CheckAllPressed();
                }
                break;
            }
        }
    }

    private void CheckAllPressed()
    {
        for (int i = 0; i < inputPressed.Length; i++)
        {
            if (!inputPressed[i]) { return; }
        }

        combinationCompleted = true;
    }
}