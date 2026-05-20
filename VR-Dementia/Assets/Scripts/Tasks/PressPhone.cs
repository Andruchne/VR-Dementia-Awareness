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

    [Header("Hand Setup")]
    [SerializeField] private Transform[] leftHandComponents;

    private Transform targetCamera;

    private IEnumerator Start()
    {
        // Wait a brief moment to allow the Meta XR system to register the headset
        yield return new WaitForSeconds(0.2f);

        DetermineActiveCamera();
        for (int i = 0; i < leftHandComponents.Length; i++)
        {
            leftHandComponents[i].gameObject.SetActive(false);
        }
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
        float duration = 0.75f;

        // We want to rotate exactly 90 degrees downwards
        float totalAngleToRotate = 90f;
        float currentRotatedAngle = 0f;

        while (elapsedTime < duration)
        {
            // Normalize time to a 0.0 to 1.0 range
            float normalizedTime = elapsedTime / duration;

            // Apply an Ease-In curve (starts slow, then accelerates)
            // Using a cubic curve (t^3) for a distinct, organic drag at the beginning
            float easedTime = normalizedTime * normalizedTime * normalizedTime;

            // Calculate the exact angle the phone should be at in this specific frame
            float targetAngleThisFrame = totalAngleToRotate * easedTime;

            // Calculate the difference between where we need to be and where we currently are
            float angleThisFrame = targetAngleThisFrame - currentRotatedAngle;

            // Rotate the phone around the camera's position
            phone.RotateAround(targetCamera.position, targetCamera.right, angleThisFrame);

            currentRotatedAngle += angleThisFrame;
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Snap to the exact final angle to prevent tiny floating point inaccuracies
        float remainingAngle = totalAngleToRotate - currentRotatedAngle;
        if (remainingAngle > 0f)
        {
            phone.RotateAround(targetCamera.position, targetCamera.right, remainingAngle);
        }

        phone.gameObject.SetActive(false);
        for (int i = 0; i < leftHandComponents.Length; i++)
        {
            leftHandComponents[i].gameObject.SetActive(true);
        }
        FinishTask();
    }
}