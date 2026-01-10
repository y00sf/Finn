using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(AudioSource))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Physics")]
    [SerializeField] private float moveSpeed = 6f;
    [Tooltip("How fast you reach top speed. Lower = Snappier.")]
    [SerializeField] private float accelerationTime = 0.1f; 
    [Tooltip("How fast you stop. Higher = More slide/weight.")]
    [SerializeField] private float decelerationTime = 0.25f; 
    [Tooltip("Smoothing when turning.")]
    [SerializeField] private float turnSmoothTime = 0.15f; 

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravityScale = 2.5f;
    [SerializeField] private float airControlSmoothTime = 0.5f; // "Floaty" air control

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float groundCheckOffset = 0.1f;
    [SerializeField] private float groundCheckRadius = 0.25f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private float footstepInterval = 0.5f;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.9f, 1.1f);
    [Range(0f, 1f)] [SerializeField] private float footstepVolume = 0.5f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private Rigidbody rb;
    private AudioSource audioSource;
    private Transform cam;
    
    private Vector2 input;
    private Vector3 currentVelocity;
    private float currentTurnVelocity; 
    private bool isGrounded;
    private float stepCycleTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.useGravity = false;

        audioSource = GetComponent<AudioSource>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (Camera.main != null) cam = Camera.main.transform;
    }

    private void Update()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        HandleFootsteps();
        HandleAnimation();
    }

    private void FixedUpdate()
    {
        CheckGround();
        Move();
        ApplyGravity();
    }

    private void Move()
    {
        // 1. Determine which "Smooth Time" to use
        float targetSmoothTime;
        
        if (isGrounded)
        {
            // If we are pushing keys: Use Acceleration (Fast)
            // If we let go: Use Deceleration (Slow/Slide)
            if (input.sqrMagnitude >= 0.01f)
            {
                targetSmoothTime = accelerationTime;
            }
            else
            {
                targetSmoothTime = decelerationTime;
            }
        }
        else
        {
            // In air, we are always floaty
            targetSmoothTime = airControlSmoothTime;
        }

        // 2. Calculate Direction
        Vector3 targetVelocity = Vector3.zero;

        if (input.sqrMagnitude >= 0.01f)
        {
            float targetAngle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + cam.eulerAngles.y;
            
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref currentTurnVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            targetVelocity = moveDir.normalized * moveSpeed;
        }

        // 3. Apply the Smooth Movement (The "Feel" Magic happens here)
        Vector3 smoothVel = Vector3.SmoothDamp(
            new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z), 
            targetVelocity, 
            ref currentVelocity, 
            targetSmoothTime 
        );

        rb.linearVelocity = new Vector3(smoothVel.x, rb.linearVelocity.y, smoothVel.z);
    }

    private void HandleAnimation()
    {
        if (animator != null)
        {
            float horizontalSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
            float normalizedSpeed = horizontalSpeed / moveSpeed;
            
            // Prevent slow-motion robot look when barely moving
            float animSpeed = (normalizedSpeed < 0.01f) ? 0 : Mathf.Max(normalizedSpeed, 0.5f);
            
            animator.SetFloat("Speed", animSpeed);
            animator.SetBool("IsGrounded", isGrounded);
        }
    }

    private void HandleFootsteps()
    {
        if (isGrounded && input.sqrMagnitude > 0.1f)
        {
            stepCycleTimer -= Time.deltaTime;
            if (stepCycleTimer <= 0)
            {
                PlayRandomFootstep();
                stepCycleTimer = footstepInterval;
            }
        }
        else
        {
            stepCycleTimer = 0.1f;
        }
    }

    private void PlayRandomFootstep()
    {
        if (footstepSounds.Length == 0) return;
        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.volume = footstepVolume;
        audioSource.PlayOneShot(footstepSounds[Random.Range(0, footstepSounds.Length)]);
    }

    private void ApplyGravity()
    {
        rb.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);
    }

    private void Jump()
    {
        Vector3 v = rb.linearVelocity;
        v.y = 0;
        rb.linearVelocity = v;
        
        float jumpForce = Mathf.Sqrt(jumpHeight * -2f * (Physics.gravity.y * gravityScale));
        rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
    }

    private void CheckGround()
    {
        isGrounded = Physics.CheckSphere(transform.position + Vector3.up * groundCheckOffset, groundCheckRadius, groundLayers);
    }
}