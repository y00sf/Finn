using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(AudioSource))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Physics")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float accelerationTime = 0.1f;
    [SerializeField] private float decelerationTime = 0.25f;
    [SerializeField] private float turnSmoothTime = 0.15f;

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravityScale = 2.5f;
    [SerializeField] private float airControlSmoothTime = 0.5f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float groundCheckOffset = 0.1f;
    [SerializeField] private float groundCheckRadius = 0.25f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private float footstepInterval = 0.5f;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.9f, 1.1f);
    [Range(0f, 1f)] [SerializeField] private float footstepVolume = 0.5f;

    [Header("Dust Particles")]
    [SerializeField] private ParticleSystem dustParticles;
    [SerializeField] private float maxDustEmissionRate = 20f;
    [SerializeField] private float minSpeedForDust = 0.5f;

    [Header("Footprint Decals")]
    [SerializeField] private GameObject footprintPrefab;
    [SerializeField] private Transform leftFootBone;
    [SerializeField] private Transform rightFootBone;
    [SerializeField] private LayerMask decalGroundLayer;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    public Rigidbody rb;
    private AudioSource audioSource;
    private Transform cam;
    private ParticleSystem.EmissionModule dustEmission;

    private PlayerControls controls;
    private InputAction moveAction;
    private InputAction jumpAction;

    private Vector2 input;
    private Vector3 currentVelocity;
    private float currentTurnVelocity;
    private bool isGrounded;
    private float stepCycleTimer;
    private bool isRightFootStep = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.useGravity = false;

        audioSource = GetComponent<AudioSource>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (Camera.main != null) cam = Camera.main.transform;

        if (dustParticles != null)
        {
            dustEmission = dustParticles.emission;
        }

        controls = new PlayerControls();
        moveAction = controls.Gameplay.Move;
        jumpAction = controls.Gameplay.Jump;
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        input = moveAction.ReadValue<Vector2>();

        if (jumpAction.WasPressedThisFrame() && isGrounded)
        {
            Jump();
        }

        HandleFootsteps();
        HandleAnimation();
        HandleDust();
    }

    private void FixedUpdate()
    {
        CheckGround();
        Move();
        ApplyGravity();
    }

    private void HandleDust()
    {
        if (dustParticles == null) return;

        Vector3 velocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        float horizontalSpeed = velocity.magnitude;

        if (isGrounded && horizontalSpeed > minSpeedForDust)
        {
            if (horizontalSpeed > 0.1f)
            {
                dustParticles.transform.rotation = Quaternion.LookRotation(velocity.normalized);
            }

            float speedRatio = Mathf.Clamp01(horizontalSpeed / moveSpeed);
            dustEmission.rateOverTime = speedRatio * maxDustEmissionRate;
        }
        else
        {
            dustEmission.rateOverTime = 0f;
        }
    }

    private void Move()
    {
        float targetSmoothTime;

        if (isGrounded)
        {
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
            targetSmoothTime = airControlSmoothTime;
        }

        Vector3 targetVelocity = Vector3.zero;

        if (input.sqrMagnitude >= 0.01f)
        {
            float targetAngle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref currentTurnVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            targetVelocity = moveDir.normalized * moveSpeed;
        }

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
                SpawnFootprint();
                stepCycleTimer = footstepInterval;
            }
        }
        else
        {
            stepCycleTimer = 0.1f;
        }
    }

    private void SpawnFootprint()
    {
        if (footprintPrefab == null) return;

        Transform currentFoot = isRightFootStep ? rightFootBone : leftFootBone;
        isRightFootStep = !isRightFootStep;

        if (currentFoot == null) return;

        RaycastHit hit;
        if (Physics.Raycast(currentFoot.position + Vector3.up * 0.5f, Vector3.down, out hit, 1.5f, decalGroundLayer))
        {
            GameObject decal = Instantiate(footprintPrefab, hit.point + Vector3.up * 0.01f, Quaternion.identity);

            decal.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * Quaternion.Euler(0, transform.eulerAngles.y, 0) * Quaternion.Euler(90, 0, 0);

            if (!isRightFootStep)
            {
                Vector3 scale = decal.transform.localScale;
                scale.x *= -1;
                decal.transform.localScale = scale;
            }
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
        if (dustParticles != null)
        {
            dustParticles.Emit(10);
        }

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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * groundCheckOffset, groundCheckRadius);
    }
}
