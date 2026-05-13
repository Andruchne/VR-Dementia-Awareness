using UnityEngine;
using UnityEngine.XR;

public class EyeController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The SkinnedMeshRenderer that contains the blendshapes for blinking.")]
    [SerializeField] private SkinnedMeshRenderer targetMesh;

    [Header("Camera Targets")]
    [Tooltip("The target to look at when a VR headset is active.")]
    [SerializeField] private Transform vrTarget;
    [Tooltip("The target to look at when playing in standard FPS mode.")]
    [SerializeField] private Transform fpsTarget;

    [Header("Eye Transforms")]
    [SerializeField] private Transform leftEye;
    [SerializeField] private Transform leftEyeGloss;
    [SerializeField] private Transform rightEye;
    [SerializeField] private Transform rightEyeGloss;

    [Header("Eye Movement Limits")]
    [SerializeField] private float maxYaw = 45f;
    [SerializeField] private float maxPitch = 30f;
    [SerializeField] private float eyeMoveSpeed = 12f;

    [Header("Blink Settings")]
    [SerializeField] private int blinkIndex = 1;
    [SerializeField] private float minBlinkInterval = 2f;
    [SerializeField] private float maxBlinkInterval = 6f;
    [SerializeField] private float blinkCloseDurationMs = 80f;
    [SerializeField] private float blinkClosedDurationMs = 150f;
    [SerializeField] private float blinkOpenDurationMs = 120f;

    private Transform targetToLookAt;

    private Quaternion _leftEyeStartRel;
    private Quaternion _leftEyeGlossStartRel;
    private Quaternion _rightEyeStartRel;
    private Quaternion _rightEyeGlossStartRel;

    private float _dartTimer;
    private Vector2 _targetDartOffset;
    private Vector2 _currentDartOffset;

    private float _currentYaw;
    private float _currentPitch;

    private enum BlinkState { Idle, Closing, Closed, Opening }
    private BlinkState _blinkState = BlinkState.Idle;
    private float _blinkTimer;
    private float _blinkHoldTimer;
    private float _currentBlinkWeight = 0f;

    private void Start()
    {
        DetermineActiveTarget();

        Quaternion inverseReference = Quaternion.Inverse(transform.rotation);

        if (leftEye != null) _leftEyeStartRel = inverseReference * leftEye.rotation;
        if (leftEyeGloss != null) _leftEyeGlossStartRel = inverseReference * leftEyeGloss.rotation;
        if (rightEye != null) _rightEyeStartRel = inverseReference * rightEye.rotation;
        if (rightEyeGloss != null) _rightEyeGlossStartRel = inverseReference * rightEyeGloss.rotation;

        ResetBlinkTimer();
        ResetDartTimer();
    }

    private void Update()
    {
        if (targetMesh != null) HandleBlinking();
        if (targetToLookAt != null) HandleEyeMovement();
    }

    public void DetermineActiveTarget()
    {
        if (XRSettings.isDeviceActive && vrTarget != null)
        {
            targetToLookAt = vrTarget;
        }
        else if (!XRSettings.isDeviceActive && fpsTarget != null)
        {
            targetToLookAt = fpsTarget;
        }
        else if (Camera.main != null)
        {
            targetToLookAt = Camera.main.transform;
        }
    }

    private void HandleEyeMovement()
    {
        _dartTimer -= Time.deltaTime;
        if (_dartTimer <= 0)
        {
            if (Random.value < 0.7f)
            {
                _targetDartOffset = Vector2.zero;
                _dartTimer = Random.Range(1.5f, 4f);
            }
            else
            {
                _targetDartOffset = new Vector2(Random.Range(-20f, 20f), Random.Range(-15f, 15f));
                _dartTimer = Random.Range(0.5f, 1.5f);
            }
        }

        _currentDartOffset = Vector2.Lerp(_currentDartOffset, _targetDartOffset, Time.deltaTime * 20f);

        Vector3 eyesCenter = transform.position;
        if (leftEye != null && rightEye != null)
        {
            eyesCenter = (leftEye.position + rightEye.position) / 2f;
        }

        Vector3 directionToTarget = targetToLookAt.position - eyesCenter;
        Quaternion lookRotation = Quaternion.LookRotation(directionToTarget, transform.up);

        Quaternion localLookRotation = Quaternion.Inverse(transform.rotation) * lookRotation;
        Vector3 euler = localLookRotation.eulerAngles;

        float targetPitch = NormalizeAngle(euler.x) + _currentDartOffset.y;
        float targetYaw = NormalizeAngle(euler.y) + _currentDartOffset.x;

        targetPitch = Mathf.Clamp(targetPitch, -maxPitch, maxPitch);
        targetYaw = Mathf.Clamp(targetYaw, -maxYaw, maxYaw);

        _currentPitch = Mathf.Lerp(_currentPitch, targetPitch, Time.deltaTime * eyeMoveSpeed);
        _currentYaw = Mathf.Lerp(_currentYaw, targetYaw, Time.deltaTime * eyeMoveSpeed);

        Quaternion clampedRelativeRot = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
        Quaternion finalGlobalRotation = transform.rotation * clampedRelativeRot;

        if (leftEye != null) leftEye.rotation = finalGlobalRotation * _leftEyeStartRel;
        if (leftEyeGloss != null) leftEyeGloss.rotation = finalGlobalRotation * _leftEyeGlossStartRel;
        if (rightEye != null) rightEye.rotation = finalGlobalRotation * _rightEyeStartRel;
        if (rightEyeGloss != null) rightEyeGloss.rotation = finalGlobalRotation * _rightEyeGlossStartRel;
    }

    private void HandleBlinking()
    {
        switch (_blinkState)
        {
            case BlinkState.Idle:
                _blinkTimer -= Time.deltaTime;
                if (_blinkTimer <= 0) _blinkState = BlinkState.Closing;
                break;

            case BlinkState.Closing:
                float closeSpeed = 100f / (blinkCloseDurationMs / 1000f);
                _currentBlinkWeight = Mathf.MoveTowards(_currentBlinkWeight, 100f, closeSpeed * Time.deltaTime);
                if (_currentBlinkWeight >= 100f)
                {
                    _blinkState = BlinkState.Closed;
                    _blinkHoldTimer = blinkClosedDurationMs / 1000f;
                }
                targetMesh.SetBlendShapeWeight(blinkIndex, _currentBlinkWeight);
                break;

            case BlinkState.Closed:
                _blinkHoldTimer -= Time.deltaTime;
                if (_blinkHoldTimer <= 0) _blinkState = BlinkState.Opening;
                break;

            case BlinkState.Opening:
                float openSpeed = 100f / (blinkOpenDurationMs / 1000f);
                _currentBlinkWeight = Mathf.MoveTowards(_currentBlinkWeight, 0f, openSpeed * Time.deltaTime);
                if (_currentBlinkWeight <= 0f)
                {
                    _blinkState = BlinkState.Idle;
                    ResetBlinkTimer();
                }
                targetMesh.SetBlendShapeWeight(blinkIndex, _currentBlinkWeight);
                break;
        }
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }

    private void ResetBlinkTimer()
    {
        _blinkTimer = Random.Range(minBlinkInterval, maxBlinkInterval);
    }

    private void ResetDartTimer()
    {
        _dartTimer = Random.Range(1f, 3f);
    }
}