using UnityEngine;
using UnityEngine.UIElements;

public class VRPhysicsHand : MonoBehaviour
{
    [Header("Real Hand")]
    [Tooltip("The actual hand that is moved from within the VR Camera Rig")]
    public Transform targetTransform;

    [Header("Ghost Hand Settings")]
    public GameObject ghostHand;
    public float distanceToShowGhost = 0.1f;

    [Header("Physics Settings")]
    public float maxDistanceBeforeTeleport = 1;

    private Rigidbody _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();

        _rb.useGravity = false;
        _rb.maxAngularVelocity = 150;
        _rb.maxDepenetrationVelocity = 3;
        _rb.solverIterations = 25;
        _rb.solverVelocityIterations = 15;

        if (ghostHand != null) { ghostHand.SetActive(false); }

        TeleportHand();
    }

    void FixedUpdate()
    {
        if (targetTransform == null) { return; }

        PhysicsMoveToTarget();
        PhysicsRotateToTarget();
        CheckGhostHandVisibility();
    }

    private void PhysicsMoveToTarget()
    {
        Vector3 positionDifference = targetTransform.position - transform.position;

        if (positionDifference.magnitude > maxDistanceBeforeTeleport)
        {
            TeleportHand();
            return;
        }

        Vector3 targetVelocity = positionDifference / Time.fixedDeltaTime;
        _rb.linearVelocity = Vector3.ClampMagnitude(targetVelocity, 15);
    }

    private void PhysicsRotateToTarget()
    {
        Quaternion rotationDifference = targetTransform.rotation * Quaternion.Inverse(transform.rotation);
        rotationDifference.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);

        if (angleInDegrees > 180) { angleInDegrees -= 360; }

        if (Mathf.Abs(angleInDegrees) > 0.1f && !float.IsInfinity(rotationAxis.x))
        {
            Vector3 targetAngularVelocity = angleInDegrees * rotationAxis * Mathf.Deg2Rad / Time.fixedDeltaTime;
            // NEU: Auch die Rotation muss begrenzt werden
            _rb.angularVelocity = Vector3.ClampMagnitude(targetAngularVelocity, 30f);
        }
    }

    private void CheckGhostHandVisibility()
    {
        if (ghostHand == null) { return; }
        float distance = Vector3.Distance(transform.position, targetTransform.position);
        ghostHand.SetActive(distance > distanceToShowGhost);
    }

    private void TeleportHand()
    {
        transform.position = targetTransform.position;
        transform.rotation = targetTransform.rotation;
        _rb.linearVelocity = Vector3.zero;
    }
}