using System.Collections;
using UnityEngine;

public class fishingBait : MonoBehaviour
{
    [SerializeField] private float flightTime = 1.5f;
    [SerializeField] private float arcHeight = 5.0f;
    [SerializeField] private float flightCollisionRadius = 0.15f;
    [SerializeField] private float impactSurfaceOffset = 0.05f;
    [SerializeField] private float biomeDetectionRadius = 1f;
    [SerializeField] private LayerMask waterLayer;
    [SerializeField] private LayerMask terrainCollisionMask;
    [SerializeField] private float waterDetectionRayHeight = 1f;
    [SerializeField] private float waterDetectionRayDistance = 10f;
    [SerializeField] private float reelSpeed = 6f;
    [SerializeField] private float catchDistance = 1.0f;
    [SerializeField] private float reelSurfaceOffset = 0f;
    [SerializeField] private float struggleSwayAmplitude = 0.7f;
    [SerializeField] private float struggleSwayFrequency = 8f;
    [SerializeField] private float struggleTurnSpeed = 360f;
    [SerializeField] private float strugglePullFrequency = 2.2f;
    [SerializeField] private float strugglePullAwayMultiplier = 2f;
    [SerializeField] private float struggleLateralSpeed = 2.8f;
    [SerializeField] private float fightPositionLerpSpeed = 14f;
    [SerializeField] private float fightEscapeDistanceMultiplier = 1.35f;
    [SerializeField] [Range(0.85f, 1f)] private float fightCatchProgressThreshold = 0.98f;
    [Header("VFX")]
    [SerializeField] private ParticleSystem waterSplashPrefab;
    [SerializeField] private float splashAutoDestroyDelay = 4f;

    private Transform reelTarget;
    private float reelSurfaceY;
    private bool reelResolved;
    private bool notifiedReelIn;
    private bool isFishStruggling;
    private float strugglePhaseOffset;
    private bool useFightProgressControl;
    private float fightProgress01 = 0.5f;
    private float fightEscapeDistance;
    private Vector3 fightAxisDirection;

    public void Initialize(Transform target)
    {
        reelTarget = target;
    }

    public void FlyToTarget(Vector3 targetPos)
    {
        StartCoroutine(MoveRoutine(targetPos));
    }

    private IEnumerator MoveRoutine(Vector3 destination)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;
        Vector3 previousPos = startPos;
        
        while (elapsed < flightTime)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / flightTime;
            
            Vector3 currentPos = Vector3.Lerp(startPos, destination, percent);
            float height = Mathf.Sin(percent * Mathf.PI) * arcHeight;
            currentPos.y += height;

            if (TryGetFlightImpact(previousPos, currentPos, out RaycastHit impactHit, out bool hitWater))
            {
                transform.position = GetImpactPosition(impactHit, hitWater);
                yield return StartCoroutine(HandleLanding(hitWater));
                yield break;
            }

            transform.position = currentPos;
            previousPos = currentPos;
            yield return null;
        }

        transform.position = destination;
        yield return StartCoroutine(HandleLanding(true));
    }

    private IEnumerator HandleLanding(bool landedInWater)
    {
        reelSurfaceY = transform.position.y + reelSurfaceOffset;
        if (landedInWater)
        {
            landedInWater = TryDetectBiome();
        }

        if (!landedInWater)
        {
            var failedManager = FishingManager.Instance;
            if (failedManager != null)
            {
                failedManager.SetFishingLock(false);
            }

            ResolveAndDestroy();
            yield break;
        }

        PlayWaterSplash(transform.position);

        var fm = FishingManager.Instance;
        if (fm != null)
        {
            fm.OnBaitLanded(this, reelSurfaceY);
        }
        else
        {
            Debug.LogError("[fishingBait] FishingManager Instance is NULL.");
        }
        
        yield return StartCoroutine(ReelRoutine());
    }

    private IEnumerator ReelRoutine()
    {
        if (reelTarget == null)
        {
            Debug.LogWarning("[fishingBait] Reel target not assigned, skipping reel.");
            var fm = FishingManager.Instance;
            if (fm != null)
            {
                fm.OnBaitReeledIn();
            }
            reelResolved = true;
            ResolveAndDestroy();
            yield break;
        }

        while (true)
        {
            Vector3 targetPos = reelTarget.position;
            Vector3 targetOnSurface = new Vector3(targetPos.x, reelSurfaceY, targetPos.z);
            Vector3 toTarget = targetOnSurface - transform.position;
            Vector3 flatToTarget = new Vector3(toTarget.x, 0f, toTarget.z);
            float horizontalDistance = flatToTarget.magnitude;
            bool fightDrivenReel = isFishStruggling && useFightProgressControl;
            float reelInThreshold = fightDrivenReel
                ? Mathf.Max(0.05f, catchDistance * 0.15f)
                : catchDistance;
            bool readyForReelIn = horizontalDistance <= reelInThreshold;
            if (fightDrivenReel && fightProgress01 < fightCatchProgressThreshold)
            {
                readyForReelIn = false;
            }

            if (readyForReelIn)
            {
                if (!notifiedReelIn)
                {
                    notifiedReelIn = true;
                    var fm = FishingManager.Instance;
                    if (fm != null)
                    {
                        fm.OnBaitReeledIn();
                    }
                }

                if (reelResolved)
                {
                    ResolveAndDestroy();
                    yield break;
                }

                yield return null;
                continue;
            }

            if (flatToTarget.sqrMagnitude > 0.0001f)
            {
                Vector3 moveDir = flatToTarget.normalized;
                
                if (isFishStruggling && useFightProgressControl)
                {
                    if (fightAxisDirection.sqrMagnitude <= 0.0001f)
                    {
                        InitializeFightAxis(targetOnSurface);
                    }

                    Vector3 lateralDir = Vector3.Cross(Vector3.up, fightAxisDirection).normalized;
                    float sway = Mathf.Sin((Time.time + strugglePhaseOffset) * struggleSwayFrequency) * struggleSwayAmplitude;
                    float desiredDistance = Mathf.Lerp(fightEscapeDistance, 0f, fightProgress01);
                    Vector3 desiredPos = targetOnSurface + (fightAxisDirection * desiredDistance) + (lateralDir * sway);
                    desiredPos.y = reelSurfaceY;

                    Vector3 toDesired = desiredPos - transform.position;
                    Vector3 flatToDesired = new Vector3(toDesired.x, 0f, toDesired.z);
                    if (flatToDesired.sqrMagnitude > 0.0001f)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(flatToDesired.normalized);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, struggleTurnSpeed * Time.deltaTime);
                    }

                    transform.position = Vector3.MoveTowards(
                        transform.position,
                        desiredPos,
                        fightPositionLerpSpeed * Time.deltaTime
                    );
                }
                else
                {
                    Vector3 velocity = moveDir * reelSpeed;

                    if (isFishStruggling)
                    {
                        Vector3 lateralDir = Vector3.Cross(Vector3.up, moveDir).normalized;
                        float sway = Mathf.Sin((Time.time + strugglePhaseOffset) * struggleSwayFrequency) * struggleSwayAmplitude;
                        float pullWave = (Mathf.Sin((Time.time + strugglePhaseOffset) * strugglePullFrequency) + 1f) * 0.5f;
                        float forwardMultiplier = 1f - (pullWave * strugglePullAwayMultiplier);

                        velocity = (moveDir * (reelSpeed * forwardMultiplier)) + (lateralDir * sway * struggleLateralSpeed);

                        if (velocity.sqrMagnitude > 0.0001f)
                        {
                            Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized);
                            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, struggleTurnSpeed * Time.deltaTime);
                        }
                    }

                    transform.position += velocity * Time.deltaTime;
                }
            }

            Vector3 surfacePos = transform.position;
            surfacePos.y = reelSurfaceY;
            transform.position = surfacePos;
            yield return null;
        }
    }

    public void ResolveReel()
    {
        reelResolved = true;
        isFishStruggling = false;
        useFightProgressControl = false;
    }

    public void SetFishStruggling(bool struggling)
    {
        isFishStruggling = struggling;
        if (struggling)
        {
            strugglePhaseOffset = Random.Range(0f, 10f);
            if (useFightProgressControl)
            {
                Vector3 targetPos = reelTarget != null ? reelTarget.position : transform.position;
                Vector3 targetOnSurface = new Vector3(targetPos.x, reelSurfaceY, targetPos.z);
                InitializeFightAxis(targetOnSurface);
            }
        }
        else
        {
            useFightProgressControl = false;
        }
    }

    public void SetFightProgress01(float progress01)
    {
        fightProgress01 = Mathf.Clamp01(progress01);
        if (useFightProgressControl) return;

        useFightProgressControl = true;
        Vector3 targetPos = reelTarget != null ? reelTarget.position : transform.position;
        Vector3 targetOnSurface = new Vector3(targetPos.x, reelSurfaceY, targetPos.z);
        InitializeFightAxis(targetOnSurface);
    }

    public void SnapToReelTarget()
    {
        if (reelTarget == null) return;

        Vector3 targetPos = reelTarget.position;
        transform.position = new Vector3(targetPos.x, reelSurfaceY, targetPos.z);
    }

    private void InitializeFightAxis(Vector3 targetOnSurface)
    {
        Vector3 awayFromPlayer = transform.position - targetOnSurface;
        awayFromPlayer.y = 0f;

        if (awayFromPlayer.sqrMagnitude <= 0.0001f)
        {
            Vector3 fallbackDir = reelTarget != null ? -reelTarget.forward : Vector3.forward;
            fallbackDir.y = 0f;
            if (fallbackDir.sqrMagnitude <= 0.0001f) fallbackDir = Vector3.forward;
            awayFromPlayer = fallbackDir.normalized;
        }

        fightAxisDirection = awayFromPlayer.normalized;
        float currentDistance = awayFromPlayer.magnitude;
        float scaledDistance = currentDistance * Mathf.Max(1f, fightEscapeDistanceMultiplier);
        fightEscapeDistance = Mathf.Max(catchDistance + 0.5f, scaledDistance);
    }

    private void ResolveAndDestroy()
    {
        Destroy(gameObject);
    }

    private void PlayWaterSplash(Vector3 splashPosition)
    {
        ParticleSystem splashSource = waterSplashPrefab;


        if (splashSource == null) return;

        ParticleSystem splashInstance = Instantiate(splashSource, splashPosition, Quaternion.identity);
        splashInstance.Play();
        Destroy(splashInstance.gameObject, Mathf.Max(0.5f, splashAutoDestroyDelay));
    }

    private bool TryGetFlightImpact(Vector3 startPos, Vector3 endPos, out RaycastHit bestHit, out bool hitWater)
    {
        Vector3 castDelta = endPos - startPos;
        float castDistance = castDelta.magnitude;
        if (castDistance <= 0.0001f)
        {
            bestHit = default;
            hitWater = false;
            return false;
        }

        RaycastHit[] hits = Physics.SphereCastAll(
            startPos,
            Mathf.Max(0.01f, flightCollisionRadius),
            castDelta.normalized,
            castDistance,
            GetSurfaceMask(),
            QueryTriggerInteraction.Collide
        );

        float closestDistance = float.MaxValue;
        bestHit = default;
        hitWater = false;

        foreach (RaycastHit hit in hits)
        {
            if (ShouldIgnoreHit(hit.collider))
            {
                continue;
            }

            bool isWaterHit = IsWaterLayer(hit.collider.gameObject.layer);
            if (hit.collider.isTrigger && !isWaterHit)
            {
                continue;
            }

            if (hit.distance >= closestDistance)
            {
                continue;
            }

            closestDistance = hit.distance;
            bestHit = hit;
            hitWater = isWaterHit;
        }

        return closestDistance < float.MaxValue;
    }

    private Vector3 GetImpactPosition(RaycastHit hit, bool hitWater)
    {
        return hitWater
            ? hit.point
            : hit.point + (hit.normal * Mathf.Max(0f, impactSurfaceOffset));
    }

    private bool TryDetectBiome()
    {
        var fm = FishingManager.Instance;
        if (fm == null) return false;

        Vector3 rayOrigin = transform.position + (Vector3.up * Mathf.Max(0.1f, waterDetectionRayHeight));
        float rayDistance = Mathf.Max(0.1f, waterDetectionRayDistance);

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayDistance, waterLayer, QueryTriggerInteraction.Collide))
        {
            fm.SetCurrentBiome(GetBiomeFromTag(hit.collider.tag));
            return true;
        }

        Collider[] cols = Physics.OverlapSphere(
            transform.position,
            biomeDetectionRadius,
            waterLayer,
            QueryTriggerInteraction.Collide
        );
        if (cols.Length > 0)
        {
            fm.SetCurrentBiome(GetBiomeFromTag(cols[0].tag));
            return true;
        }

        return false;
    }

    private bool ShouldIgnoreHit(Collider hitCollider)
    {
        if (hitCollider == null)
        {
            return true;
        }

        Transform hitTransform = hitCollider.transform;
        if (hitTransform == transform || hitTransform.IsChildOf(transform))
        {
            return true;
        }

        if (reelTarget == null)
        {
            return false;
        }

        Transform reelRoot = reelTarget.root;
        return hitTransform == reelRoot || hitTransform.IsChildOf(reelRoot);
    }

    private bool IsWaterLayer(int layer)
    {
        return (waterLayer.value & (1 << layer)) != 0;
    }

    private int GetSurfaceMask()
    {
        int terrainMask = terrainCollisionMask.value;
        if (terrainMask == 0)
        {
            terrainMask = Physics.DefaultRaycastLayers & ~waterLayer.value;
        }

        return terrainMask | waterLayer.value;
    }

    private BiomeType GetBiomeFromTag(string tag)
    {
        return tag switch
        {
            "IceWater" => BiomeType.IceBiome,
            "VolcanoWater" => BiomeType.VolcanoBiome,
            "WildeWater" => BiomeType.WiledBiome,
            _ => BiomeType.WiledBiome 
        };
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, biomeDetectionRadius);
    }
}
