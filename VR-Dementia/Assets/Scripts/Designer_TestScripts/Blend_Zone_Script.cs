using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class Blend_Zone_Script : MonoBehaviour
{
    [Header("GameObjects with FMOD Event Emitters")]
    public List<GameObject> emitterObjects = new List<GameObject>();

    [Header("Points to measure distance from")]
    public GameObject point_A;
    public GameObject point_B;

    // cached emitters
    private List<StudioEventEmitter> emitters = new List<StudioEventEmitter>();

    // cached player transform
    private Transform playerTransform;

    private void LogDistances()
    {
        if (playerTransform == null || point_A == null || point_B == null)
            return;

        float distA = Vector3.Distance(playerTransform.position, point_A.transform.position);
        float distB = Vector3.Distance(playerTransform.position, point_B.transform.position);

        Debug.LogFormat("Player distance to A: {0:F2}, to B: {1:F2}", distA, distB);

        // Apply parameters to every emitter instance
        foreach (StudioEventEmitter emitter in emitters)
        {
            if (emitter.EventInstance.isValid())
            {
                emitter.EventInstance.setParameterByName("BlendZone_A", distA);
                emitter.EventInstance.setParameterByName("BlendZone_B", distB);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerTransform = other.transform;
            LogDistances();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerTransform = null;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LogDistances();
        }
    }

    void Start()
    {
        // Collect emitters from the assigned GameObjects
        foreach (GameObject obj in emitterObjects)
        {
            if (obj == null) continue;

            StudioEventEmitter emitter = obj.GetComponent<StudioEventEmitter>();

            if (emitter != null)
            {
                emitters.Add(emitter);
            }
            else
            {
                Debug.LogWarning(obj.name + " does not have a StudioEventEmitter component.");
            }
        }
    }
}