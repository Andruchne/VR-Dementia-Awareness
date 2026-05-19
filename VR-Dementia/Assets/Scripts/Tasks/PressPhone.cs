using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class PressPhone : SimulationTask
{
    [Header("Camera References")]
    [SerializeField] private Transform vrCamera;
    [SerializeField] private Transform fpsCamera;

    [Header("Phone Setup")]
    [SerializeField] private Transform phone;

    private Transform targetCamera;

    private IEnumerator Start()
    {
        // Wait a brief moment to allow the Meta XR system to register the headset
        yield return new WaitForSeconds(0.2f);

        DetermineActiveCamera();
    }

    private void DetermineActiveCamera()
    {
        // Actively search for connected Head-Mounted Displays (HMD)
        List<InputDevice> hmdDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, hmdDevices);

        // Check if the list contains valid devices indicating an active VR headset
        bool isVRPresent = hmdDevices.Count > 0 && hmdDevices[0].isValid;

        if (isVRPresent && vrCamera != null) { targetCamera = vrCamera; }
        else if (!isVRPresent && fpsCamera != null) { targetCamera = fpsCamera; }
        else if (Camera.main != null) { targetCamera = Camera.main.transform; }

        if (targetCamera == null) { Debug.LogError("PressPhone: No valid camera found!"); }
    }

    public void SendButtonPressed()
    {
        StartCoroutine(HidePhone());
    }

    private IEnumerator HidePhone()
    {
        if (targetCamera == null || phone == null) { yield break; }

        // Disable the HeadFollow script so it doesn't fight animation
        HeadFollow headFollow = phone.GetComponent<HeadFollow>();
        if (headFollow != null) { headFollow.enabled = false; }

        float elapsedTime = 0f;
        float duration = 1f;

        // We want to rotate exactly 90 degrees downwards
        float totalAngleToRotate = 90f;
        float currentRotatedAngle = 0f;

        while (elapsedTime < duration)
        {
            // Calculate how many degrees we need to rotate in this specific frame
            float angleThisFrame = (totalAngleToRotate / duration) * Time.deltaTime;

            // Clamp the angle to ensure we don't overshoot past exactly 90 degrees
            if (currentRotatedAngle + angleThisFrame > totalAngleToRotate) { angleThisFrame = totalAngleToRotate - currentRotatedAngle; }

            // Rotate the phone around the camera's position
            phone.RotateAround(targetCamera.position, targetCamera.right, angleThisFrame);

            currentRotatedAngle += angleThisFrame;
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        phone.gameObject.SetActive(false);
        FinishTask();
    }
}