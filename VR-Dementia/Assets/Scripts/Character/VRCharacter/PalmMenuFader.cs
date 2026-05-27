using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using System;

/// <summary>
/// Handles fading the handheld menu in the left hand in/out.
/// By checking the angles between the palm and the head, the alpha of the menu is mapped to make it transition
/// Additionally checks, whether the hands are holding something, turning off/blocking the visibility in that case
/// </summary>
public class PalmMenuFader : MonoBehaviour
{
    [Header("Tracking References")]
    [Tooltip("CenterEyeAnchor (Head) from OVRCameraRig")]
    public Transform headTransform;

    [Tooltip("LeftHand from OVRCameraRig).")]
    public Transform palmTransform;

    [Header("UI References")]
    [Tooltip("Canvas Group of menu")]
    public CanvasGroup menuCanvasGroup;

    [Header("Interaction References")]
    [Tooltip("Drag the Left Hand Grab Interactor here (e.g., HandGrabInteractor)")]
    public HandGrabInteractor leftHandGrabInteractor;

    [Tooltip("Drag the Left Controller Grab Interactor here (e.g., ControllerGrabInteractor)")]
    public HandGrabInteractor leftControllerGrabInteractor;

    [Header("Fade Settings")]
    [Tooltip("The angle at which the menu is 100% visible")]
    public float fullyVisibleAngle = 20;

    [Tooltip("The angle at which the menu starts fading in from 0%")]
    public float startFadingAngle = 45;

    [Tooltip("How smoothly the menu fades in and out")]
    public float fadeSpeed = 12;

    public event Action<bool> OnMenuVisibilityChange;

    private bool isActive;
    private bool angledAfterActivation;

    private void Start()
    {
        EventBus<OnChangePalmMenuActive>.OnEvent += SetActiveness;
    }

    private void OnDestroy()
    {
        EventBus<OnChangePalmMenuActive>.OnEvent -= SetActiveness;
    }

    private void Update()
    {
        if (headTransform == null || palmTransform == null || menuCanvasGroup == null) { return; }

        float targetAlpha = 0f;

        // Only calculate visibility if the hand/controller is not holding anything
        if (!IsHoldingObject() && isActive)
        {
            Vector3 directionToFace = (headTransform.position - palmTransform.position).normalized;

            // Depending on how your Palm Transform is oriented, you might need to adjust -palmTransform.up
            float angleToFace = Vector3.Angle(-palmTransform.up, directionToFace);

            if (angleToFace <= startFadingAngle && !angledAfterActivation)
            {
                // Map angle between startFadingAngle and fullyVisibleAngle to a 0 to 1 range
                targetAlpha = 1f - Mathf.Clamp01((angleToFace - fullyVisibleAngle) / (startFadingAngle - fullyVisibleAngle));
            }
            else if (angleToFace > startFadingAngle && angledAfterActivation) { angledAfterActivation = false; }
        }

        // Smoothly transition the current alpha to the target alpha
        // If IsHoldingObject() is true, targetAlpha is 0, so it will smoothly fade out
        menuCanvasGroup.alpha = Mathf.Lerp(menuCanvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);

        // Turn off UI interactions when invisible (disables interactions)
        bool isVisible = menuCanvasGroup.alpha > 0.05f;

        // Only toggle Active state if it actually changes to save performance
        if (menuCanvasGroup.gameObject.activeSelf != isVisible)
        {
            menuCanvasGroup.gameObject.SetActive(isVisible);
        }

        // Only set interactable/blocksRaycasts if the object is active
        if (isVisible)
        {
            menuCanvasGroup.interactable = true;
            menuCanvasGroup.blocksRaycasts = true;
            EventBus<OnPalmMenuVisibilityChanged>.Publish(new OnPalmMenuVisibilityChanged(true));
        }
        else
        {
            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;
            EventBus<OnPalmMenuVisibilityChanged>.Publish(new OnPalmMenuVisibilityChanged(false));
        }
    }

    // Checks if either the hand or the controller is currently grabbing an object.
    private bool IsHoldingObject()
    {
        // Check Hand
        if (leftHandGrabInteractor != null && leftHandGrabInteractor.State == InteractorState.Select)
        {
            return true;
        }

        // Check Controller
        if (leftControllerGrabInteractor != null && leftControllerGrabInteractor.State == InteractorState.Select)
        {
            return true;
        }

        return false;
    }

    private void SetActiveness(OnChangePalmMenuActive newActive)
    {
        Vector3 directionToFace = (headTransform.position - palmTransform.position).normalized;
        float angleToFace = Vector3.Angle(-palmTransform.up, directionToFace);

        if (angleToFace <= startFadingAngle) { angledAfterActivation = true; }
        else { angledAfterActivation = false; }

        isActive = newActive.isActive;
    }
}