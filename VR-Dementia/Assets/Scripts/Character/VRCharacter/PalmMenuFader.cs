using UnityEngine;

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

    [Header("Fade Settings")]
    [Tooltip("The angle at which the menu is 100% visible")]
    public float fullyVisibleAngle = 20;

    [Tooltip("The angle at which the menu starts fading in from 0%")]
    public float startFadingAngle = 45;

    [Tooltip("How smoothly the menu fades in and out")]
    public float fadeSpeed = 12;

    private void Update()
    {
        if (headTransform == null || palmTransform == null || menuCanvasGroup == null) { return; }

        Vector3 directionToFace = (headTransform.position - palmTransform.position).normalized;
        // Calculate the exact angle between the palm's direction and the face
        float angleToFace = Vector3.Angle(-palmTransform.up, directionToFace);

        // Calculate the target transparency based on the angle
        float targetAlpha = 0f;

        if (angleToFace <= startFadingAngle)
        {
            // Map angle between startFadingAngle and fullyVisibleAngle to a 0 to 1 range
            targetAlpha = 1f - Mathf.Clamp01((angleToFace - fullyVisibleAngle) / (startFadingAngle - fullyVisibleAngle));
        }

        // Smoothly transition the current alpha to the target alpha
        menuCanvasGroup.alpha = Mathf.Lerp(menuCanvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);

        // Turn off UI interactions when invisible (disables interactions)
        bool isVisible = menuCanvasGroup.alpha > 0.1f;
        menuCanvasGroup.interactable = isVisible;
        menuCanvasGroup.blocksRaycasts = isVisible;
    }
}