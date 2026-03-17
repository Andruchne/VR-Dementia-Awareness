using UnityEngine;
using Oculus.Interaction.Locomotion;
using System.Collections;


/// <summary>
/// Moves the physics hands in a physics-based way.
/// It orients itself on the real hand position, and on the current movement velocity - predicting where the hand is supposed to be
/// This predict approach, helps synch the movement, happening in Update(), together with the physics movement, happening in FixedUpdate()
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class VRPhysicsHand : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Transform targetTransform;
    [SerializeField] private FirstPersonLocomotor playerLocomotor;
    [SerializeField] private GameObject ghostHand;

    [Header("Show Ghost Hand")]
    [SerializeField] private float distanceToShowGhost = 0.1f;

    [Header("Physics Settings")]
    [SerializeField] private float maxDistanceBeforeTeleport = 1f;
    [SerializeField] private float positionMultiplier = 20f;
    [SerializeField] private float rotationMultiplier = 20f;
    [SerializeField] private float maxVelocity = 15f;
    [SerializeField] private float maxAngularVelocity = 30f;

    [Header("Locomotion Sync Tuning")]
    [Range(0f, 1f)]
    [Tooltip("Controls how far ahead the target position is predicted. Lower to reduce forward overshoot.")]
    [SerializeField] private float predictionDamping = 0.5f;

    [Range(0f, 1f)]
    [Tooltip("Controls how much raw player velocity is injected into the hand. Lower to reduce forward pushing.")]
    [SerializeField] private float feedForwardDamping = 0.5f;

    private Rigidbody _rb;
    
    // Timer to pause physics update after teleport
    private float _currentTeleportCooldown = 0f;
    private const float TeleportCooldown = 0.05f; 

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.maxAngularVelocity = 150f;
        _rb.maxDepenetrationVelocity = 3f;
        _rb.solverIterations = 25;
        _rb.solverVelocityIterations = 15;

        if (ghostHand != null) { ghostHand.SetActive(false); }

        DelayedTeleport();
    }

    void FixedUpdate()
    {
        if (targetTransform == null) { return; }

        // Skip physics move part, if timer is not yet up
        if (_currentTeleportCooldown > 0f)
        {
            _currentTeleportCooldown -= Time.fixedDeltaTime;
            
            // Let the hand stick completely to the hand during the timer
            TeleportHand();
            CheckGhostHandVisibility();
            return;
        }

        PhysicsMoveToTarget();
        PhysicsRotateToTarget();
        CheckGhostHandVisibility();
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
    /// Check whether snap movement was applied, and if so, teleport the hands
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
            TeleportHand();
            // Start cooldown to prevent hands from moving immediately after teleport
            _currentTeleportCooldown = TeleportCooldown;
        }
    }

    /// <summary>
    /// Moves the hands physics-based
    /// </summary>
    private void PhysicsMoveToTarget()
    {
        Vector3 predictedTargetPosition = targetTransform.position;
        Vector3 locomotorVelocity = Vector3.zero;

        if (playerLocomotor != null && !playerLocomotor.IgnoringVelocity)
        {
            locomotorVelocity = playerLocomotor.Velocity;

            // Predict future position, damping it slightly to fit the actual hand position
            predictedTargetPosition += locomotorVelocity * Time.fixedDeltaTime * predictionDamping;
        }

        Vector3 positionDifference = predictedTargetPosition - transform.position;

        if (positionDifference.magnitude > maxDistanceBeforeTeleport)
        {
            TeleportHand();
            return;
        }

        Vector3 targetVelocity = positionDifference * positionMultiplier;

        // Calculate velocity to push the hand forward, based on the movement velocity
        targetVelocity += locomotorVelocity * feedForwardDamping;

        _rb.linearVelocity = Vector3.ClampMagnitude(targetVelocity, maxVelocity);
    }

    /// <summary>
    /// Rotates the hands physics-based
    /// </summary>
    private void PhysicsRotateToTarget()
    {
        Quaternion rotationDifference = targetTransform.rotation * Quaternion.Inverse(transform.rotation);
        rotationDifference.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);

        if (angleInDegrees > 180f) { angleInDegrees -= 360f; }

        if (Mathf.Abs(angleInDegrees) > 0.1f && !float.IsInfinity(rotationAxis.x))
        {
            Vector3 targetAngularVelocity = angleInDegrees * rotationAxis * Mathf.Deg2Rad * rotationMultiplier;
            _rb.angularVelocity = Vector3.ClampMagnitude(targetAngularVelocity, maxAngularVelocity);
        }
    }

    /// <summary>
    /// Show real position of hands, if physics hands fall behind
    /// </summary>
    private void CheckGhostHandVisibility()
    {
        if (ghostHand == null) { return; }
        
        float distance = Vector3.Distance(transform.position, targetTransform.position);
        ghostHand.SetActive(distance > distanceToShowGhost);
    }

    /// <summary>
    /// To teleport physics hands, in case they fall behind too much
    /// </summary>
    private void TeleportHand()
    {
        transform.position = targetTransform.position;
        transform.rotation = targetTransform.rotation;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }

    private IEnumerator DelayedTeleport()
    {
        yield return 1.0f;
        TeleportHand();
    }
}