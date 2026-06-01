using UnityEngine;
using UnityEngine.InputSystem;

public class LocomotionSwitcher : MonoBehaviour
{
    [Header("Locomotion Mode 1 (Default)")]
    [Tooltip("Drag the GameObjects responsible for Smooth Locomotion and Flick Teleport here.")]
    [SerializeField] private GameObject[] smoothAndFlickObjects;

    [Header("Locomotion Mode 2")]
    [Tooltip("Drag the GameObjects responsible for Targeted Ray Teleport here.")]
    [SerializeField] private GameObject[] targetedTeleportObjects;

    [Header("Input Setup")]
    [Tooltip("Input action to toggle the locomotion mode (e.g., clicking the right thumbstick).")]
    [SerializeField] private InputActionReference toggleModeAction;

    // State variable to track the current mode
    private bool isTeleportModeActive = false;

    private void Start()
    {
        // Ensure the initial state is applied correctly on startup
        UpdateLocomotionState();
    }

    private void OnEnable()
    {
        // Subscribe to the input action
        if (toggleModeAction != null)
        {
            toggleModeAction.action.Enable();
            toggleModeAction.action.performed += OnToggleLocomotion;
        }
    }

    private void OnDisable()
    {
        // Clean up the subscription to prevent memory leaks
        if (toggleModeAction != null)
        {
            toggleModeAction.action.performed -= OnToggleLocomotion;
            toggleModeAction.action.Disable();
        }
    }

    private void Update()
    {
        // Keyboard fallback for quick testing in the editor
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            ToggleMode();
        }
    }

    private void OnToggleLocomotion(InputAction.CallbackContext context)
    {
        ToggleMode();
    }

    private void ToggleMode()
    {
        isTeleportModeActive = !isTeleportModeActive;
        UpdateLocomotionState();

        Debug.Log($"Locomotion Mode Switched. Targeted Teleport Active: {isTeleportModeActive}");
    }

    private void UpdateLocomotionState()
    {
        // Toggle Smooth/Flick objects
        foreach (var obj in smoothAndFlickObjects)
        {
            if (obj != null) obj.SetActive(!isTeleportModeActive);
        }

        // Toggle Targeted Teleport objects
        foreach (var obj in targetedTeleportObjects)
        {
            if (obj != null) obj.SetActive(isTeleportModeActive);
        }
    }
}