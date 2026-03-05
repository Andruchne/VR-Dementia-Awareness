using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsProxyFollower : MonoBehaviour
{
    [Header("Setup")]
    public Transform proxyTarget;
    public GrabCollisionHandler layerController;

    [Tooltip("For fixing bug affecting character camera (grabbed object reaches infinity velocity)")]
    public float maxVelocity = 15;
    public float maxDistanceBeforeTeleport = 1;

    private Rigidbody _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();

        // Align object at start
        TeleportObject();
    }

    void FixedUpdate()
    {
        if (proxyTarget == null || layerController == null) { return; }

        if (layerController.IsGrabbed)
        {
            _rb.useGravity = false;

            Vector3 positionDifference = proxyTarget.position - transform.position;

            // Teleport the gameObject, in case the distance is > maxDistance
            if (positionDifference.magnitude > maxDistanceBeforeTeleport)
            {
                TeleportObject();
                return;
            }

            // Velocity calculation
            Vector3 targetVelocity = positionDifference / Time.fixedDeltaTime;
            _rb.linearVelocity = Vector3.ClampMagnitude(targetVelocity, maxVelocity);

            // Rotation calculations
            Quaternion rotationDifference = proxyTarget.rotation * Quaternion.Inverse(transform.rotation);
            rotationDifference.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);

            if (angleInDegrees > 180f) angleInDegrees -= 360f;
            if (Mathf.Abs(angleInDegrees) > 0.1f && !float.IsInfinity(rotationAxis.x))
            {
                Vector3 angularVelocity = (angleInDegrees * rotationAxis * Mathf.Deg2Rad) / Time.fixedDeltaTime;
                _rb.angularVelocity = Vector3.ClampMagnitude(angularVelocity, maxVelocity * 2f);
            }
        }
        // Ungrab logic
        else
        {
            _rb.useGravity = true;
            // Synch the position of the proxy with the actual visible object
            proxyTarget.position = transform.position;
            proxyTarget.rotation = transform.rotation;
        }
    }

    private void TeleportObject()
    {
        transform.position = proxyTarget.position;
        transform.rotation = proxyTarget.rotation;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }
}