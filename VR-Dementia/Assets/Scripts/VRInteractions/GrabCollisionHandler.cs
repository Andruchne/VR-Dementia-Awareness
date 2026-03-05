using UnityEngine;

public class GrabCollisionHandler : MonoBehaviour
{
    [Header("Setup")]
    public GameObject targetPhysicsObject;
    public string ignoreHandLayerName = "Ignore Physics Hands";

    public bool IsGrabbed { get; private set; } = false;

    private int _ignoreHandLayer;
    private int _originalLayer;


    void Awake()
    {
        _ignoreHandLayer = LayerMask.NameToLayer(ignoreHandLayerName);
        if (targetPhysicsObject != null)
        {
            _originalLayer = targetPhysicsObject.layer;
        }
    }

    // Called with "When Select" Event on the grab object
    public void HandleGrab()
    {
        if (targetPhysicsObject == null) return;
        IsGrabbed = true;
        SetLayerRecursively(targetPhysicsObject, _ignoreHandLayer);
    }

    // Called with "When Unselect" Event on the grab object
    public void HandleRelease()
    {
        if (targetPhysicsObject == null) return;
        IsGrabbed = false;
        SetLayerRecursively(targetPhysicsObject, _originalLayer);
    }

    // Make all children of gameObject have the same layer
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}