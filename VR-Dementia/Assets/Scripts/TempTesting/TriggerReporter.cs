using System;
using UnityEngine;

public class TriggerReporter : MonoBehaviour
{
    public event Action<Collider> onTriggerEntered;

    private void OnTriggerEnter(Collider other)
    {
        onTriggerEntered?.Invoke(other);
    }
}