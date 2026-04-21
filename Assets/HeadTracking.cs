using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class HeadTracking : MonoBehaviour
{
    
    [Header("Head Tracking Settings")]
    [SerializeField] private Transform headBone;
    [SerializeField] private float maxHorizontalAngle = 60f;
    [SerializeField] private float maxVerticalAngle = 30f;
    [SerializeField] private float headSmoothSpeed = 8f; // Higher = smoother, less jitter
    
    [Header("Camera Settings")]
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private float cameraOffsetAmount = 0.3f;
    [SerializeField] private float cameraSmoothSpeed = 6f;
    
    [Header("Input Settings")]
    [SerializeField] private bool useMousePosition = true;
    [SerializeField] private bool useGamepad = true;
    [SerializeField] private float mouseSensitivity = 0.5f; // Lower sensitivity for less jitter
    [SerializeField] private float gamepadSensitivity = 1.5f;
    [SerializeField] private float inputDeadzone = 0.1f; // Ignore tiny movements
    [SerializeField] private float inputSmoothing = 10f; // Smooth input changes
    
    [Header("Reset Settings")]
    [SerializeField] private bool autoResetWhenIdle = true;
    [SerializeField] private float resetSpeed = 4f;
    [SerializeField] private float idleTimeBeforeReset = 0.3f;
    
    [Header("External Target")]
    [SerializeField] private bool allowExternalTarget = true;
    [SerializeField] private float externalTargetWeight = 1f; // 0-1, how much external target influences
    [SerializeField] private float externalTargetSmoothSpeed = 5f;
    
    // Internal state
    private Quaternion originalHeadRotation;
    private Vector2 currentInput;
    private Vector2 targetInput;
    private Vector2 smoothedInput;
    private float idleTimer;
    
    // Camera
    private CinemachineFollow followComponent;
    private Vector3 originalCameraOffset;
    
    // Input devices
    private Mouse mouse;
    private Gamepad gamepad;
    
    // External target
    private Transform externalTarget;
    private Quaternion externalTargetRotation;
    private bool hasExternalTarget;
    private float currentExternalWeight;

    void Start()
    {
        if (headBone != null)
        {
            originalHeadRotation = headBone.localRotation;
        }
        
        if (virtualCamera != null)
        {
            followComponent = virtualCamera.GetComponent<CinemachineFollow>();
            if (followComponent != null)
            {
                originalCameraOffset = followComponent.FollowOffset;
            }
        }
        
        mouse = Mouse.current;
        gamepad = Gamepad.current;
    }

    void Update()
    {
        ProcessInput();
    }
    
    void LateUpdate()
    {
        // Update external target rotation if we have one
        if (hasExternalTarget && externalTarget != null)
        {
            UpdateExternalTargetRotation();
        }
        
        UpdateHeadRotation();
        UpdateCameraOffset();
    }

    void ProcessInput()
    {
        targetInput = Vector2.zero;
        bool hasActiveInput = false;

        // MOUSE INPUT - Screen position based
        if (useMousePosition && mouse != null)
        {
            Vector2 mousePos = mouse.position.ReadValue();
            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            Vector2 mouseOffset = (mousePos - screenCenter);
            
            // Normalize by screen size
            mouseOffset.x /= Screen.width * 0.5f;
            mouseOffset.y /= Screen.height * 0.5f;
            
            // Apply deadzone
            if (mouseOffset.magnitude > inputDeadzone)
            {
                mouseOffset = Vector2.ClampMagnitude(mouseOffset, 1f);
                targetInput = mouseOffset * mouseSensitivity;
                hasActiveInput = true;
            }
        }

        // GAMEPAD INPUT - Right stick
        if (useGamepad && gamepad != null)
        {
            Vector2 stickInput = gamepad.rightStick.ReadValue();
            
            // Apply deadzone
            if (stickInput.magnitude > inputDeadzone)
            {
                stickInput = Vector2.ClampMagnitude(stickInput, 1f);
                targetInput += stickInput * gamepadSensitivity * Time.deltaTime;
                hasActiveInput = true;
            }
        }

        // Clamp accumulated input
        targetInput.x = Mathf.Clamp(targetInput.x, -1f, 1f);
        targetInput.y = Mathf.Clamp(targetInput.y, -1f, 1f);

        // Smooth the input to reduce jitter
        smoothedInput = Vector2.Lerp(smoothedInput, targetInput, Time.deltaTime * inputSmoothing);
        
        // Handle idle timer for auto-reset
        if (hasActiveInput)
        {
            idleTimer = 0f;
            currentInput = smoothedInput;
        }
        else
        {
            idleTimer += Time.deltaTime;
            
            if (autoResetWhenIdle && idleTimer > idleTimeBeforeReset && !hasExternalTarget)
            {
                // Smoothly reset to zero
                currentInput = Vector2.Lerp(currentInput, Vector2.zero, Time.deltaTime * resetSpeed);
                smoothedInput = Vector2.Lerp(smoothedInput, Vector2.zero, Time.deltaTime * resetSpeed);
            }
        }
    }

    void UpdateHeadRotation()
    {
        if (headBone == null) return;

        Quaternion targetRotation;

        // Calculate input-based rotation
        float yawAngle = currentInput.x * maxHorizontalAngle;
        float pitchAngle = -currentInput.y * maxVerticalAngle;
        Quaternion inputBasedRotation = originalHeadRotation * Quaternion.Euler(pitchAngle, yawAngle, 0f);

        // Blend with external target if present
        if (hasExternalTarget && allowExternalTarget)
        {
            // Smoothly transition external target weight
            currentExternalWeight = Mathf.Lerp(currentExternalWeight, externalTargetWeight, Time.deltaTime * externalTargetSmoothSpeed);
            targetRotation = Quaternion.Lerp(inputBasedRotation, externalTargetRotation, currentExternalWeight);
        }
        else
        {
            // Smoothly reduce external target weight when cleared
            currentExternalWeight = Mathf.Lerp(currentExternalWeight, 0f, Time.deltaTime * externalTargetSmoothSpeed);
            targetRotation = inputBasedRotation;
        }

        // Apply smooth rotation with damping
        headBone.localRotation = Quaternion.Lerp(
            headBone.localRotation,
            targetRotation,
            Time.deltaTime * headSmoothSpeed
        );
    }

    void UpdateCameraOffset()
    {
        if (followComponent == null) return;

        Vector3 targetOffset = originalCameraOffset + new Vector3(
            currentInput.x * cameraOffsetAmount,
            currentInput.y * cameraOffsetAmount * 0.5f,
            0f
        );

        followComponent.FollowOffset = Vector3.Lerp(
            followComponent.FollowOffset,
            targetOffset,
            Time.deltaTime * cameraSmoothSpeed
        );
    }

    // PUBLIC API
    public void SetLookTarget(Transform target)
    {
        externalTarget = target;
        hasExternalTarget = target != null;
        
        if (hasExternalTarget && headBone != null)
        {
            UpdateExternalTargetRotation();
        }
    }
    
    private void UpdateExternalTargetRotation()
    {
        if (externalTarget == null || headBone == null) return;
        
        // Calculate direction from head to target
        Vector3 directionToTarget = externalTarget.position - headBone.position;
        
        if (directionToTarget.sqrMagnitude > 0.001f)
        {
            // Get the look rotation in world space
            Quaternion worldLookRotation = Quaternion.LookRotation(directionToTarget);
            
            // Calculate the angle difference from body forward
            Vector3 bodyForward = transform.forward;
            Vector3 targetForward = directionToTarget.normalized;
            
            // Project onto horizontal plane for yaw
            bodyForward.y = 0;
            targetForward.y = 0;
            
            if (bodyForward.sqrMagnitude > 0.001f && targetForward.sqrMagnitude > 0.001f)
            {
                bodyForward.Normalize();
                targetForward.Normalize();
                
                // Calculate horizontal angle (yaw)
                float horizontalAngle = Vector3.SignedAngle(bodyForward, targetForward, Vector3.up);
                
                // Calculate vertical angle (pitch)
                float verticalAngle = Mathf.Atan2(directionToTarget.y, 
                    new Vector2(directionToTarget.x, directionToTarget.z).magnitude) * Mathf.Rad2Deg;
                
                // Clamp angles
                horizontalAngle = Mathf.Clamp(horizontalAngle, -maxHorizontalAngle, maxHorizontalAngle);
                verticalAngle = Mathf.Clamp(verticalAngle, -maxVerticalAngle, maxVerticalAngle);
                
                // Create target rotation in local space
                externalTargetRotation = originalHeadRotation * Quaternion.Euler(verticalAngle, horizontalAngle, 0f);
            }
        }
    }
    
    public void ClearLookTarget()
    {
        externalTarget = null;
        hasExternalTarget = false;
    }
    
    public bool HasExternalTarget()
    {
        return hasExternalTarget;
    }
    
    public void ResetHead()
    {
        currentInput = Vector2.zero;
        smoothedInput = Vector2.zero;
        targetInput = Vector2.zero;
    }

    void OnDrawGizmos()
    {
        if (headBone != null && Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(headBone.position, 0.1f);
            Gizmos.DrawRay(headBone.position, headBone.forward * 0.5f);
            
            if (hasExternalTarget && externalTarget != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(headBone.position, externalTarget.position);
            }
        }
    }
}