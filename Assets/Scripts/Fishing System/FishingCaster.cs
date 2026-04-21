using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class FishingCaster : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject baitPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform reelTarget;
    
    [Header("UI References")]
    [SerializeField] private Slider powerSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Gradient colorGradient;

    [Header("Settings")]
    [SerializeField] private float chargeSpeed = 150f;
    [SerializeField] private float maxCastDistance = 20f;
    [SerializeField] private LayerMask waterLayer;
    [SerializeField] private LayerMask terrainCollisionMask;
    [SerializeField] private float surfaceProbeHeight = 50f;
    [SerializeField] private float surfaceProbeDistance = 120f;

    [Header("Input")]
    public InputAction castAction;

    private bool isCharging = false;
    private float currentValue = 0f;
    private int direction = 1;
    
    private GameObject activeBait; 

    private FishingManager _fishingManager;

    private void OnEnable()
    {
        if (castAction != null) castAction.Enable();
    }

    private void OnDisable()
    {
        if (castAction != null) castAction.Disable();
    }

    private void Start()
    {
        if (powerSlider != null)
        {
            powerSlider.minValue = 0;
            powerSlider.maxValue = 100;
            powerSlider.SetValueWithoutNotify(0);
            powerSlider.gameObject.SetActive(false);
        }

        _fishingManager = FishingManager.Instance;
        if (_fishingManager == null)
        {
            _fishingManager = FindObjectOfType<FishingManager>(true);
        }
    }

    private void Update()
    {
        if (_fishingManager == null || powerSlider == null) return;
        

        if (activeBait != null) return;
        if (MiniGameManager.IsMiniGameActive) return;

        bool canCast = _fishingManager.CanCastWithCurrentBait();

        if (!canCast)
        {
            if (isCharging)
            {
                isCharging = false;
                currentValue = 0;
                powerSlider.SetValueWithoutNotify(0);
                powerSlider.gameObject.SetActive(false);
                _fishingManager.SetFishingLock(false);
            }
            return;
        }

        if (castAction != null && castAction.IsPressed())
        {
            if (!powerSlider.gameObject.activeSelf) powerSlider.gameObject.SetActive(true);
            if (!isCharging)
            {
                isCharging = true;
                _fishingManager.SetFishingLock(true);
            }
            ProcessCharge();
        }
        else if (isCharging)
        {
            isCharging = false;
            bool threw = PerformThrow();
            powerSlider.gameObject.SetActive(false); 
            if (!threw)
            {
                _fishingManager.SetFishingLock(false);
            }
        }
    }

    private void ProcessCharge()
    {
        currentValue += direction * chargeSpeed * Time.deltaTime;

        if (currentValue >= 100f) { currentValue = 100f; direction = -1; }
        else if (currentValue <= 0f) { currentValue = 0f; direction = 1; }

        powerSlider.SetValueWithoutNotify(currentValue);

        if (fillImage != null)
            fillImage.color = colorGradient.Evaluate(currentValue / 100f);
    }

    private bool PerformThrow()
    {
        if (baitPrefab == null || spawnPoint == null)
        {
            Debug.LogError("[FishingCaster] baitPrefab or spawnPoint not assigned!");
            return false;
        }

        float percentage = currentValue / 100f;
        _fishingManager.SetLastCastStrength(percentage);
        float dist = percentage * maxCastDistance;
    
        Vector3 flatForward = transform.forward;
        flatForward.y = 0;
        flatForward.Normalize();

        Vector3 targetPoint = spawnPoint.position + (flatForward * dist);
        if (!TryResolveCastSurface(targetPoint, out targetPoint))
        {
            Debug.LogWarning("[FishingCaster] Cast cancelled because no landing surface was found.");
            currentValue = 0;
            powerSlider.SetValueWithoutNotify(0);
            direction = 1;
            return false;
        }
        
        activeBait = Instantiate(baitPrefab, spawnPoint.position, Quaternion.identity);
        
        fishingBait baitScript = activeBait.GetComponent<fishingBait>();

        if (baitScript != null)
        {
            baitScript.Initialize(reelTarget != null ? reelTarget : transform);
            baitScript.FlyToTarget(targetPoint);
        }
        else
        {
            Debug.LogError("[FishingCaster] Your Bait Prefab is missing the 'fishingBait' script!");
            Destroy(activeBait);
            activeBait = null;
            return false;
        }
        
        currentValue = 0;
        powerSlider.SetValueWithoutNotify(0);
        direction = 1;
        return true;
    }

    private bool TryResolveCastSurface(Vector3 desiredPoint, out Vector3 resolvedPoint)
    {
        Vector3 rayOrigin = desiredPoint + (Vector3.up * Mathf.Max(1f, surfaceProbeHeight));
        float probeDistance = Mathf.Max(surfaceProbeDistance, surfaceProbeHeight + 5f);
        int surfaceMask = GetSurfaceMask();

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, probeDistance, surfaceMask, QueryTriggerInteraction.Collide))
        {
            resolvedPoint = hit.point;
            return true;
        }

        resolvedPoint = desiredPoint;
        return false;
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
}
