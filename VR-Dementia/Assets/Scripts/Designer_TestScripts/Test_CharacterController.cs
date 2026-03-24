using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Test_CharacterController : MonoBehaviour
{
    public Transform cameraTransform;
    public float gravity = -9.81f;

    [Header("Look")]
    public float mouseSensitivity = 1f;
    [Header("Movement")]
    public float moveSpeed = 5f;

    private CharacterController _controller;
    private Vector3 _velocity;
    private float pitch;

    private void HandleLook()
    {
        if (Mouse.current == null)
            return;

        Vector2 delta = Mouse.current.delta.ReadValue();
        float yaw = delta.x * mouseSensitivity * Time.deltaTime;
        float pitchDelta = -delta.y * mouseSensitivity * Time.deltaTime;

        // rotate the body around y
        transform.Rotate(Vector3.up, yaw);

        // apply pitch to camera
        if (cameraTransform != null)
        {
            pitch = Mathf.Clamp(pitch + pitchDelta, -90f, 90f);
            cameraTransform.localEulerAngles = new Vector3(pitch, 0f, 0f);
        }
    }

    private void HandleMovement()
    {
        if (_controller == null)
            return;

        Vector2 input = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) input.y += 1;
            if (Keyboard.current.sKey.isPressed) input.y -= 1;
            if (Keyboard.current.aKey.isPressed) input.x -= 1;
            if (Keyboard.current.dKey.isPressed) input.x += 1;
        }

        Vector3 move = transform.forward * input.y + transform.right * input.x;
        move.Normalize();
        _controller.Move(move * moveSpeed * Time.deltaTime);

        // gravity
        if (!_controller.isGrounded)
        {
            _velocity.y += gravity * Time.deltaTime;
            _controller.Move(_velocity * Time.deltaTime);
        }
        else if (_velocity.y < 0f)
        {
            _velocity.y = -2f; // small downward force to keep controller grounded
        }
    }

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        if (cameraTransform == null && transform.childCount > 0)
        {
            // assume first child is the camera
            cameraTransform = transform.GetChild(0);
        }
    }

    void Update()
    {
        HandleLook();
        HandleMovement();
    }
}
