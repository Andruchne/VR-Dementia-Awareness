using UnityEngine;

public class GrabLayerController : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Das sichtbare Objekt mit den Collidern, dessen Layer geändert werden soll.")]
    public GameObject targetPhysicsObject;

    [Tooltip("Der Name des Layers, den die Hand ignoriert.")]
    public string noHandLayerName = "NoHandInteraction";

    private int _noHandLayer;
    private int _originalLayer;
    private int _grabCount = 0;

    void Awake()
    {
        // Sucht die interne Layer-ID anhand deines Namens
        _noHandLayer = LayerMask.NameToLayer(noHandLayerName);

        if (targetPhysicsObject != null)
        {
            // Speichert den ursprünglichen Layer (meistens "Default" oder "Interactable")
            _originalLayer = targetPhysicsObject.layer;
        }
        else
        {
            Debug.LogWarning("[GrabLayerController] Bitte weise das targetPhysicsObject zu!");
        }
    }

    // Wird vom Interactable-Event aufgerufen, wenn EINE Hand greift
    public void HandleGrab()
    {
        if (targetPhysicsObject == null) return;

        _grabCount++;

        // Nur wenn die ERSTE Hand greift, wechseln wir den Layer
        if (_grabCount == 1)
        {
            SetLayerRecursively(targetPhysicsObject, _noHandLayer);
            Debug.Log($"[GrabLayerController] Layer zu {noHandLayerName} gewechselt.");
        }
    }

    // Wird vom Interactable-Event aufgerufen, wenn EINE Hand loslässt
    public void HandleRelease()
    {
        if (targetPhysicsObject == null) return;

        _grabCount--;

        // Sicherheits-Check, falls Events doppelt feuern
        if (_grabCount <= 0)
        {
            _grabCount = 0;
            // Nur wenn KEINE Hand mehr greift, stellen wir den Layer wieder her
            SetLayerRecursively(targetPhysicsObject, _originalLayer);
            Debug.Log($"[GrabLayerController] Layer zu Ursprung wiederhergestellt.");
        }
    }

    // Hilfsfunktion: Ändert den Layer des Objekts und ALLER Child-Objekte
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}