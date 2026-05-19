using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// This script can be attached to any GameObject
/// The object maintains a fixed distance, stays level horizontally (ignoring pitch/roll),
/// and smoothly follows the camera's yaw only when a certain threshold is exceeded.
/// </summary>
public class HeadFollow : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The VR Camera.")]
    [SerializeField] private Transform vrCamera;

    [Tooltip("The FPS Camera.")]
    [SerializeField] private Transform fpsCamera;

    [Header("Positioning Settings")]
    [Tooltip("Distance from the camera to the HUD.")]
    [SerializeField] private float distance = 2.0f;

    [Tooltip("Vertical offset relative to the camera's eye level. Usually slightly below 0 (-0.2) is best.")]
    [SerializeField] private float heightOffset = -0.2f;

    [Header("Movement Settings")]
    [Tooltip("The angle (in degrees) the user can look left/right before the HUD starts moving.")]
    [SerializeField] private float yawThreshold = 25.0f;

    [Tooltip("How fast the HUD smoothly catches up to the target position.")]
    [SerializeField] private float followSpeed = 5.0f;

    private float targetYaw;
    private Transform targetCamera;

    private IEnumerator Start()
    {
        // Wait a brief moment to allow the Meta XR system to register the headset
        yield return new WaitForSeconds(0.2f);

        DetermineActiveTarget();

        if (targetCamera != null)
        {
            // Initialize the HUD's starting yaw to exactly match the camera at spawn
            targetYaw = targetCamera.eulerAngles.y;

            // Set start position to be infront of the camera
            Vector3 targetDirection = Quaternion.Euler(0, targetYaw, 0) * Vector3.forward;
            Vector3 targetPosition = targetCamera.position + new Vector3(0, heightOffset, 0) + (targetDirection * distance);
            Quaternion targetRotation = Quaternion.Euler(gameObject.transform.rotation.eulerAngles.x, targetYaw, gameObject.transform.rotation.eulerAngles.z);

            transform.position = targetPosition;
            transform.rotation = targetRotation;
        }
        else
        {
            Debug.LogError("VRHUDController: No VR Camera found! Please assign it in the inspector.");
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null) { return; }

        // Get the camera's current yaw (horizontal rotation)
        float cameraYaw = targetCamera.eulerAngles.y;

        // Calculate the shortest difference between the current target yaw and the camera's yaw
        float yawDifference = Mathf.DeltaAngle(targetYaw, cameraYaw);

        // If the user looks further left/right than our threshold, drag the target yaw along
        if (Mathf.Abs(yawDifference) > yawThreshold)
        {
            targetYaw = cameraYaw - (Mathf.Sign(yawDifference) * yawThreshold);
        }

        // Calculate the target direction using only the yaw (ignoring pitch/roll completely)
        Vector3 targetDirection = Quaternion.Euler(0, targetYaw, 0) * Vector3.forward;

        // Calculate the final target position
        // It takes the camera's actual Y position (crouching/standing) plus our manual offset
        Vector3 targetPosition = targetCamera.position + new Vector3(0, heightOffset, 0) + (targetDirection * distance);

        // Calculate the target rotation so the canvas directly faces the user horizontally
        Quaternion targetRotation = Quaternion.Euler(gameObject.transform.rotation.eulerAngles.x, targetYaw, gameObject.transform.rotation.eulerAngles.z);

        // Smoothly interpolate current position and rotation towards the newly calculated targets
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, followSpeed * Time.deltaTime);
    }

    public void DetermineActiveTarget()
    {
        // Actively search for connected Head-Mounted Displays (HMD)
        List<InputDevice> hmdDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, hmdDevices);

        // Check if the list contains valid devices indicating an active VR headset
        bool isVRPresent = hmdDevices.Count > 0 && hmdDevices[0].isValid;

        if (isVRPresent && vrCamera != null) { targetCamera = vrCamera; }
        else if (!isVRPresent && fpsCamera != null) { targetCamera = fpsCamera; }
        else if (Camera.main != null) { targetCamera = Camera.main.transform; }
    }
}