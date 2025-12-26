using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;

    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float accel = 30f;
    [SerializeField] private float decel = 40f;
    [SerializeField] private float turnSpeedDeg = 720f;

    [SerializeField] private float jumpSpeed = 7f;
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.12f;

    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float groundProbeRadius = 0.22f;
    [SerializeField] private float groundProbeDistance = 0.35f;
    [SerializeField] private float groundSnapDownSpeed = 2f;

    private Rigidbody rb;
    private Vector2 moveAxis;
    private Vector3 moveDirWorld;

    private bool grounded;
    private float coyoteTimer;
    private float jumpBufferTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Update()
    {
        moveAxis = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (moveAxis.sqrMagnitude < 0.01f) moveAxis = Vector2.zero;
        else moveAxis.Normalize();

        if (Input.GetButtonDown("Jump"))
            jumpBufferTimer = jumpBufferTime;

        if (jumpBufferTimer > 0f) jumpBufferTimer -= Time.deltaTime;
        if (coyoteTimer > 0f) coyoteTimer -= Time.deltaTime;

        if (moveAxis == Vector2.zero)
        {
            moveDirWorld = Vector3.zero;
        }
        else if (cameraTransform == null)
        {
            moveDirWorld = new Vector3(moveAxis.x, 0f, moveAxis.y).normalized;
        }
        else
        {
            Vector3 f = cameraTransform.forward; f.y = 0f; f.Normalize();
            Vector3 r = cameraTransform.right;   r.y = 0f; r.Normalize();
            moveDirWorld = (r * moveAxis.x + f * moveAxis.y).normalized;
        }
    }

    private void FixedUpdate()
    {
        CheckGround();

        Vector3 v = rb.linearVelocity;
        Vector3 planar = new Vector3(v.x, 0f, v.z);

        Vector3 targetPlanar = moveDirWorld * moveSpeed;
        float rate = (moveDirWorld.sqrMagnitude > 0f) ? accel : decel;

        planar = Vector3.MoveTowards(planar, targetPlanar, rate * Time.fixedDeltaTime);

        if (grounded && v.y <= 0f)
            v.y = -groundSnapDownSpeed;

        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            v.y = jumpSpeed;
            grounded = false;
        }

        rb.linearVelocity = new Vector3(planar.x, v.y, planar.z);

        if (moveDirWorld.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDirWorld, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                turnSpeedDeg * Time.fixedDeltaTime
            );
        }
    }

    private void CheckGround()
    {
        Vector3 origin = transform.position + Vector3.up * 0.05f;

        grounded = Physics.SphereCast(
            origin,
            groundProbeRadius,
            Vector3.down,
            out _,
            groundProbeDistance,
            groundLayers,
            QueryTriggerInteraction.Ignore
        );

        if (grounded)
            coyoteTimer = coyoteTime;
    }
}
