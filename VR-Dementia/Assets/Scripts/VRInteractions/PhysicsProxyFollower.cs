using Oculus.Interaction.Locomotion;
using UnityEngine;

/// <summary>
/// Moves grabbable VR objects in a physics-based way.
/// Taking in a kinematically grabbable proxy, this script interprets and applies the movement to the physics object instead, allowing proper collisions.
/// These objects slightly fall behind the VRCharacter, when moving while holding the item.
/// This is not an issue however, as it creates a feel of weight to the object.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PhysicsProxyFollower : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Transform proxyTarget;
    [SerializeField] private GrabCollisionHandler grabCollisionHandler;

    [Header("Locomotion Sync")]
    [SerializeField] private FirstPersonLocomotor playerLocomotor;

    [Header("Physics Settings")]
    [Tooltip("Wie stark das Objekt zur Hand gezogen wird")]
    [SerializeField] private float positionMultiplier = 20f;
    [SerializeField] private float rotationMultiplier = 20f;

    [Tooltip("For fixing bug affecting character camera")]
    [SerializeField] private float maxVelocity = 15f;
    [SerializeField] private float maxAngularVelocity = 30f;
    [SerializeField] private float maxDistanceBeforeTeleport = 1f;

    private Rigidbody _rb;

    // Timer to pause physics update after teleport
    private float _currentTeleportCooldown = 0f;
    private const float TeleportCooldown = 0.05f;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        TeleportObject();
    }

    void FixedUpdate()
    {
        if (proxyTarget == null || grabCollisionHandler == null) { return; }

        if (_currentTeleportCooldown > 0f)
        {
            _currentTeleportCooldown -= Time.fixedDeltaTime;

            // Let the object stick completely to the hand during the timer
            TeleportObject();
            return;
        }

        if (grabCollisionHandler.IsGrabbed)
        {
            PhysicsMoveToTarget();
            PhysicsRotateToTarget();
        }
        else
        {
            // Let the object act freely otherwise
            _rb.useGravity = true;
            proxyTarget.position = transform.position;
            proxyTarget.rotation = transform.rotation;
        }
    }

    private void PhysicsMoveToTarget()
    {
        _rb.useGravity = false;

        Vector3 positionDifference = proxyTarget.position - transform.position;

        if (positionDifference.magnitude > maxDistanceBeforeTeleport)
        {
            TeleportObject();
            return;
        }

        // Apply movement velocity
        Vector3 targetVelocity = positionDifference * positionMultiplier;
        _rb.linearVelocity = Vector3.ClampMagnitude(targetVelocity, maxVelocity);
    }

    private void PhysicsRotateToTarget()
    {
        Quaternion rotationDifference = proxyTarget.rotation * Quaternion.Inverse(transform.rotation);
        rotationDifference.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);

        if (angleInDegrees > 180f) angleInDegrees -= 360f;

        if (Mathf.Abs(angleInDegrees) > 0.1f && !float.IsInfinity(rotationAxis.x))
        {
            Vector3 targetAngularVelocity = angleInDegrees * rotationAxis * Mathf.Deg2Rad * rotationMultiplier;
            _rb.angularVelocity = Vector3.ClampMagnitude(targetAngularVelocity, maxAngularVelocity);
        }
    }

    /// <summary>
    /// Subscribe to Movement Event (For differantiating between smooth and snap movement)
    /// </summary>
    void OnEnable()
    {
        if (playerLocomotor != null)
        {
            playerLocomotor.WhenLocomotionEventHandled += HandleLocomotionEvent;
        }
    }

    void OnDisable()
    {
        if (playerLocomotor != null)
        {
            playerLocomotor.WhenLocomotionEventHandled -= HandleLocomotionEvent;
        }
    }

    /// <summary>
    /// Check whether snap movement was applied, and if so, teleport the object
    /// </summary>
    /// <param name="locomotionEvent"></param>
    /// <param name="delta"></param>
    private void HandleLocomotionEvent(LocomotionEvent locomotionEvent, Pose delta)
    {
        bool isTranslationJump = locomotionEvent.Translation == LocomotionEvent.TranslationType.Absolute ||
                                 locomotionEvent.Translation == LocomotionEvent.TranslationType.Relative;

        bool isRotationJump = locomotionEvent.Rotation == LocomotionEvent.RotationType.Absolute ||
                              locomotionEvent.Rotation == LocomotionEvent.RotationType.Relative;

        if (isTranslationJump || isRotationJump)
        {
            TeleportObject();
            // Start cooldown to prevent hands from moving immediately after teleport
            _currentTeleportCooldown = TeleportCooldown;
        }
    }

    private void TeleportObject()
    {
        transform.position = proxyTarget.position;
        transform.rotation = proxyTarget.rotation;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }

    public bool IsGrabbed()
    {
        return grabCollisionHandler.IsGrabbed;
    }
}