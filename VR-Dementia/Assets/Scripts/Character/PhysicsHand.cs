using UnityEngine;

public class VRPhysicsHand : MonoBehaviour
{
    [Header("Target Setup")]
    [Tooltip("Ziehe hier die Geisterhand rein (z.B. l_handMeshNode oder b_l_wrist aus dem Camera Rig), NICHT den Anchor!")]
    public Transform targetTransform;

    [Header("Physics Settings")]
    [Range(0.1f, 1f)]
    public float positionFollowSpeed = 0.5f;
    [Range(0.1f, 1f)]
    public float rotationFollowSpeed = 0.5f;
    public float maxDistanceBeforeTeleport = 1.0f;

    [Header("Ghost Hand Settings")]
    public GameObject ghostHand;
    public float distanceToShowGhost = 0.1f;

    private Rigidbody _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();

        // Schwerkraft aus
        _rb.useGravity = false;
        _rb.maxAngularVelocity = 150f;

        if (ghostHand != null) ghostHand.SetActive(false);
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
            _rb.linearVelocity = Vector3.zero;
            return;
        }

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
        if (ghostHand == null) return;
        float distance = Vector3.Distance(transform.position, targetTransform.position);
        ghostHand.SetActive(distance > distanceToShowGhost);
    }
}