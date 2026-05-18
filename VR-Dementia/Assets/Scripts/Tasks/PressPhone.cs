using System.Collections;
using UnityEngine;

public class PressPhone : SimulationTask
{
    [SerializeField] Transform phonePivot;

    public void SendButtonPressed()
    {
        StartCoroutine(HidePhone());
    }

    private IEnumerator HidePhone()
    {
        phonePivot.gameObject.GetComponentInChildren<HeadFollow>().enabled = false;

        float elapsedTime = 0f;
        float duration = 2f;

        Quaternion startRotation = phonePivot.localRotation;

        Vector3 currentEuler = phonePivot.localEulerAngles;
        Quaternion endRotation = Quaternion.Euler(currentEuler.x + 120f, currentEuler.y, currentEuler.z);

        while (elapsedTime < duration)
        {
            phonePivot.localRotation = Quaternion.Slerp(startRotation, endRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        phonePivot.localRotation = endRotation;
        phonePivot.gameObject.SetActive(false);
    }
}