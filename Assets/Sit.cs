using UnityEngine;

public class Sit : MonoBehaviour
{
    [SerializeField] private Transform sitPoint;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private bool isSitting;
    [SerializeField] private Transform returnPoint;
    [SerializeField] private float toggleCooldown = 0.25f;

    private PlayerMovement playerMovement;
    private Rigidbody playerRigidbody;
    private Vector3 cachedStandPosition;
    private Quaternion cachedStandRotation;
    private bool cachedIsKinematic;
    private bool cachedUseGravity;
    private float nextToggleTime;

    private void Awake()
    {
        if (sitPoint == null)
        {
            Debug.LogError("Sit point not assigned.", this);
        }

        ResolvePlayerReferences();
    }

    private void ResolvePlayerReferences()
    {
        if (playerTransform == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
            }
        }

        if (playerTransform == null)
        {
            Debug.LogError("Player transform not found.", this);
            return;
        }

        playerMovement = playerTransform.GetComponentInParent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerTransform = playerMovement.transform;
        }
        else
        {
            Debug.LogError("PlayerMovement not found on player.", this);
        }

        playerRigidbody = playerTransform.GetComponent<Rigidbody>();
        if (playerRigidbody == null)
        {
            playerRigidbody = playerTransform.GetComponentInParent<Rigidbody>();
        }

        if (playerRigidbody != null)
        {
            playerTransform = playerRigidbody.transform;
        }
        else
        {
            Debug.LogWarning("Player Rigidbody not found. Sitting will move the transform only.", this);
        }
    }

    public void ToggleSit()
    {
        if (Time.time < nextToggleTime || sitPoint == null || playerTransform == null)
        {
            return;
        }

        if (isSitting)
        {
            StandUp();
        }
        else
        {
            SitDown();
        }

        nextToggleTime = Time.time + toggleCooldown;
    }

    public void SitDown()
    {
        if (isSitting || sitPoint == null || playerTransform == null)
        {
            return;
        }

        cachedStandPosition = playerTransform.position;
        cachedStandRotation = playerTransform.rotation;

        if (playerMovement != null)
        {
            playerMovement.SetMovementEnabled(false);
        }

        CacheRigidbodyState();
        FreezePlayerForSitting();
        SetPlayerPose(sitPoint.position, sitPoint.rotation);

        isSitting = true;
    }

    public void StandUp()
    {
        if (!isSitting || playerTransform == null)
        {
            return;
        }

        Vector3 targetPosition = returnPoint != null ? returnPoint.position : cachedStandPosition;
        Quaternion targetRotation = returnPoint != null ? returnPoint.rotation : cachedStandRotation;

        FreezePlayerForSitting();
        SetPlayerPose(targetPosition, targetRotation);
        RestoreRigidbodyState();

        if (playerMovement != null)
        {
            playerMovement.SetMovementEnabled(true);
        }

        isSitting = false;
    }

    private void CacheRigidbodyState()
    {
        if (playerRigidbody == null)
        {
            return;
        }

        cachedIsKinematic = playerRigidbody.isKinematic;
        cachedUseGravity = playerRigidbody.useGravity;
    }

    private void FreezePlayerForSitting()
    {
        if (playerRigidbody == null)
        {
            return;
        }

        playerRigidbody.linearVelocity = Vector3.zero;
        playerRigidbody.angularVelocity = Vector3.zero;
        playerRigidbody.isKinematic = true;
        playerRigidbody.useGravity = false;
    }

    private void RestoreRigidbodyState()
    {
        if (playerRigidbody == null)
        {
            return;
        }

        playerRigidbody.linearVelocity = Vector3.zero;
        playerRigidbody.angularVelocity = Vector3.zero;
        playerRigidbody.isKinematic = cachedIsKinematic;
        playerRigidbody.useGravity = cachedUseGravity;
    }

    private void SetPlayerPose(Vector3 position, Quaternion rotation)
    {
        if (playerRigidbody != null)
        {
            playerRigidbody.position = position;
            playerRigidbody.rotation = rotation;
        }

        playerTransform.SetPositionAndRotation(position, rotation);
        Physics.SyncTransforms();
    }
}
