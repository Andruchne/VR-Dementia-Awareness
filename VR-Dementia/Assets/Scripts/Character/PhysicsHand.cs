using UnityEngine;

public class VRPhysicsHand : MonoBehaviour
{
    [Header("Target Setup")]
    public Transform targetTransform;

    [Header("Physics Settings")]
    [Range(0.1f, 1f)]
    [Tooltip("1 = Hand schnappt sofort ans Ziel. 0.5 = Hand folgt weich.")]
    public float positionFollowSpeed = 0.5f;
    [Range(0.1f, 1f)]
    public float rotationFollowSpeed = 0.5f;
    public float maxDistanceBeforeTeleport = 1.0f;

    [Header("Ghost Hand Settings")]
    public Renderer ghostHandRenderer;
    public float distanceToShowGhost = 0.1f;

    private Rigidbody _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();

        // GANZ WICHTIG: Schwerkraft aus, sonst fällt die Hand jeden Frame nach unten
        _rb.useGravity = false;
        _rb.maxAngularVelocity = 150f;

        if (ghostHandRenderer != null) ghostHandRenderer.enabled = false;
    }

    void FixedUpdate()
    {
        if (targetTransform == null) return;

        PhysicsMoveToTarget();
        PhysicsRotateToTarget();
        CheckGhostHandVisibility();
    }

    private void PhysicsMoveToTarget()
    {
        Vector3 positionDifference = targetTransform.position - transform.position;

        // Teleport, falls die Hand komplett feststeckt
        if (positionDifference.magnitude > maxDistanceBeforeTeleport)
        {
            transform.position = targetTransform.position;
            transform.rotation = targetTransform.rotation;
            // Velocity resetten, damit sie nach dem Teleport nicht wild weiterfliegt
            _rb.linearVelocity = Vector3.zero;
            return;
        }

        // Berechne exakte Velocity, um das Ziel zu erreichen, gedämpft durch FollowSpeed
        Vector3 targetVelocity = (positionDifference / Time.fixedDeltaTime);
        _rb.linearVelocity = targetVelocity * positionFollowSpeed;
    }

    private void PhysicsRotateToTarget()
    {
        Quaternion rotationDifference = targetTransform.rotation * Quaternion.Inverse(transform.rotation);
        rotationDifference.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);

        if (angleInDegrees > 180f) angleInDegrees -= 360f;

        if (Mathf.Abs(angleInDegrees) > 0.1f && !float.IsInfinity(rotationAxis.x))
        {
            Vector3 targetAngularVelocity = (angleInDegrees * rotationAxis * Mathf.Deg2Rad) / Time.fixedDeltaTime;
            _rb.angularVelocity = targetAngularVelocity * rotationFollowSpeed;
        }
    }

    private void CheckGhostHandVisibility()
    {
        if (ghostHandRenderer == null) return;
        float distance = Vector3.Distance(transform.position, targetTransform.position);
        print(distance);
        ghostHandRenderer.enabled = distance > distanceToShowGhost;
    }
}