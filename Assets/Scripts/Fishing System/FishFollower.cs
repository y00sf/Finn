using UnityEngine;

public class FishFollower : MonoBehaviour
{
    [SerializeField] private float followSpeed = 3f;
    [SerializeField] private float turnSpeed = 360f;
    [SerializeField] private float swimDepth = 0.5f;
    [SerializeField] private float stopDistance = 0.2f;
    [SerializeField] private float bitePullAmplitude = 0.2f;
    [SerializeField] private float bitePullFrequency = 12f;
    [SerializeField] private float biteYawAngle = 28f;
    [SerializeField] private float biteYawFrequency = 16f;

    private Transform baitTarget;
    private float surfaceY;
    private bool isBiting;
    private Vector3 biteLocalAnchor;
    private Quaternion biteLocalRotationAnchor;
    private float bitePhaseOffset;

    public void Initialize(Transform bait, float waterSurfaceY)
    {
        baitTarget = bait;
        surfaceY = waterSurfaceY;
    }

    private void Update()
    {
        if (isBiting)
        {
            if (transform.parent == null)
            {
                Destroy(gameObject);
                return;
            }

            float pull = Mathf.Sin((Time.time + bitePhaseOffset) * bitePullFrequency) * bitePullAmplitude;
            float yaw = Mathf.Sin((Time.time + bitePhaseOffset) * biteYawFrequency) * biteYawAngle;
            transform.localPosition = biteLocalAnchor + new Vector3(pull, 0f, 0f);
            transform.localRotation = biteLocalRotationAnchor * Quaternion.Euler(0f, yaw, 0f);
            return;
        }

        if (baitTarget == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPos = baitTarget.position;
        targetPos.y = surfaceY - swimDepth;
        Vector3 toTarget = targetPos - transform.position;
        float distance = toTarget.magnitude;

        if (distance > stopDistance)
        {
            Vector3 moveDir = toTarget / distance;
            transform.position += moveDir * followSpeed * Time.deltaTime;

            Vector3 flatDir = new Vector3(moveDir.x, 0f, moveDir.z);
            if (flatDir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(flatDir);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
            }
        }
    }

    public void Bite(Transform bait)
    {
        if (bait == null)
        {
            Destroy(gameObject);
            return;
        }

        isBiting = true;
        transform.SetParent(bait, true);
        biteLocalAnchor = transform.localPosition;
        biteLocalRotationAnchor = transform.localRotation;
        bitePhaseOffset = Random.Range(0f, 10f);
    }

    public void Escape()
    {
        Destroy(gameObject);
    }
}
