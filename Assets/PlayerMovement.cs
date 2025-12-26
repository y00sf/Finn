using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    public Transform cameraTransform;

    public float moveSpeed = 6f;
    public float accel = 30f;
    public float decel = 40f;
    public float turnSpeedDeg = 720f;

    public float jumpSpeed = 7f;
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;

    public LayerMask groundLayers;
    public float groundProbeRadius = 0.22f;
    public float groundProbeDistance = 0.35f;
    public float groundSnapDownSpeed = 2f;

    Rigidbody rb;
    Vector2 moveAxis;
    Vector3 moveDirWorld;

    bool grounded;
    float coyoteTimer;
    float jumpBufferTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
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

    void FixedUpdate()
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

    void CheckGround()
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
