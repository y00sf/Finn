using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Smooth Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float speedSmoothTime = 0.1f; 
    [SerializeField] private float turnSmoothTime = 0.1f; 

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravityScale = 2.5f;
    [SerializeField] private float airControl = 0.8f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float groundCheckOffset = 0.1f;
    [SerializeField] private float groundCheckRadius = 0.25f;
    
    private Rigidbody rb;
    private Transform cam;
    
    private Vector2 input;
    private Vector3 currentVelocity;
    private float currentTurnVelocity; 
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.useGravity = false;

        if (Camera.main != null)
        {
            cam = Camera.main.transform;
        }
        else
        {
            Debug.LogError("Main Camera not found!");
        }
    }

    private void Update()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        CheckGround();
        Move();
        ApplyGravity();
    }

    private void Move()
    {
        if (input.sqrMagnitude >= 0.01f)
        {
           
            float targetAngle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + cam.eulerAngles.y;
            
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref currentTurnVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            Vector3 targetVelocity = moveDir.normalized * moveSpeed;
            Vector3 smoothVel = Vector3.SmoothDamp(
                new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z), 
                targetVelocity, 
                ref currentVelocity, 
                speedSmoothTime 
            );

            
            rb.linearVelocity = new Vector3(smoothVel.x, rb.linearVelocity.y, smoothVel.z);
        }
        else
        {
            Vector3 smoothVel = Vector3.SmoothDamp(
                new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z),
                Vector3.zero,
                ref currentVelocity,
                speedSmoothTime
            );
            rb.linearVelocity = new Vector3(smoothVel.x, rb.linearVelocity.y, smoothVel.z);
        }
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