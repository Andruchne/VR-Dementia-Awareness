using UnityEngine;
using UnityEngine.InputSystem;
using Oculus.Interaction;

[RequireComponent(typeof(Rigidbody))]
public class FPSCharacter : MonoBehaviour, IActiveState
{
    public bool Active => Keyboard.current != null && Keyboard.current.eKey.isPressed;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float runMultiplier = 1.5f;
    private Rigidbody rb;

    [Header("Camera")]
    public Camera playerCamera;
    public float mouseSensitivity = 0.2f;
    private float xRot, yRot;

    [Header("Object Grab")]
    public float grabDistance = 3f;
    public LayerMask grabbableLayer;
    public Transform holdPosition;
    public float grabForce = 15f;

    [Header("Meta UI Integration")]
    [Tooltip("Ray Interactor) here")]
    public RayInteractor desktopUIRay;

    private Rigidbody grabbedRb;
    private float originalLinearDamping;
    private float originalAngularDamping;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Set current rotation of character
        Vector3 currentRot = playerCamera.transform.rotation.eulerAngles;
        xRot = currentRot.x;
        yRot = transform.rotation.eulerAngles.y;
    }

    void Update()
    {
        HandleLook();
        HandleInteractionInput();
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleGrabbedObject();
    }

    private void HandleLook()
    {
        if (Mouse.current == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        // Rotate camera for up/down
        xRot -= mouseDelta.y * mouseSensitivity;
        xRot = Mathf.Clamp(xRot, -85f, 85f);

        // Rotate character for left/right
        yRot += mouseDelta.x * mouseSensitivity;

        playerCamera.transform.localRotation = Quaternion.Euler(xRot, 0, 0);
        transform.rotation = Quaternion.Euler(0, yRot, 0);
    }

    private void HandleMovement()
    {
        if (Keyboard.current == null) return;

        // Movement input
        float x = 0, z = 0;
        if (Keyboard.current.wKey.isPressed) z += 1;
        if (Keyboard.current.sKey.isPressed) z -= 1;
        if (Keyboard.current.aKey.isPressed) x -= 1;
        if (Keyboard.current.dKey.isPressed) x += 1;

        Vector3 moveDir = (transform.forward * z + transform.right * x).normalized;
        Vector3 targetVelocity = moveDir * moveSpeed;

        targetVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = targetVelocity;
    }

    private void HandleInteractionInput()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (grabbedRb != null)
            {
                Drop();
            }
            else
            {
                bool isHoveringUI = desktopUIRay != null && desktopUIRay.State == InteractorState.Hover;
                if (!isHoveringUI)
                {
                    TryGrab();
                }
            }
        }
    }

    private void TryGrab()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance, grabbableLayer))
        {
            if (hit.rigidbody != null)
            {
                grabbedRb = hit.rigidbody;
                grabbedRb.useGravity = false;

                originalLinearDamping = grabbedRb.linearDamping;
                originalAngularDamping = grabbedRb.angularDamping;

                grabbedRb.linearDamping = 10f;
                grabbedRb.angularDamping = 10f;
            }
        }
    }

    private void Drop()
    {
        // Reset object physics
        if (grabbedRb != null)
        {
            grabbedRb.useGravity = true;
            grabbedRb.linearDamping = originalLinearDamping;
            grabbedRb.angularDamping = originalAngularDamping;
            grabbedRb = null;
        }
    }

    private void HandleGrabbedObject()
    {
        // Move object to aimed for direction
        if (grabbedRb != null)
        {
            Vector3 directionToPoint = holdPosition.position - grabbedRb.position;
            grabbedRb.linearVelocity = directionToPoint * grabForce;
        }
    }
}