using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class FishingCaster : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject baitPrefab;
    [SerializeField] private Transform spawnPoint;
    
    [Header("UI References")]
    [SerializeField] private Slider powerSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Gradient colorGradient;

    [Header("Settings")]
    [SerializeField] private float chargeSpeed = 150f;
    [SerializeField] private float maxCastDistance = 20f;
    [SerializeField] private LayerMask waterLayer;

    [Header("Input")]
    public InputAction castAction;

    private bool isCharging = false;
    private float currentValue = 0f;
    private int direction = 1;

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
            if (_fishingManager == null) Debug.LogError("[FishingCaster] No FishingManager found in scene!");
        }
    }

    private void Update()
    {
        if (_fishingManager == null || powerSlider == null) return;

      
        bool canCast = _fishingManager.CanCastWithCurrentBait();

        if (!canCast)
        {
            if (isCharging)
            {
                isCharging = false;
                currentValue = 0;
                powerSlider.SetValueWithoutNotify(0);
                powerSlider.gameObject.SetActive(false);
            }
            return;
        }

      
        if (castAction != null && castAction.IsPressed())
        {
            if (!powerSlider.gameObject.activeSelf) powerSlider.gameObject.SetActive(true);
            isCharging = true;
            ProcessCharge();
        }
        else if (isCharging)
        {
            isCharging = false;
            PerformThrow();
            powerSlider.gameObject.SetActive(false); 
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

    private void PerformThrow()
    {
        if (baitPrefab == null || spawnPoint == null)
        {
            Debug.LogError("[FishingCaster] baitPrefab or spawnPoint not assigned!");
            return;
        }

    
        float percentage = currentValue / 100f;
        float dist = percentage * maxCastDistance;
    
        Vector3 flatForward = transform.forward;
        flatForward.y = 0;
        flatForward.Normalize();

        Vector3 targetPoint = spawnPoint.position + (flatForward * dist);

      
        Vector3 rayOrigin = targetPoint;
        rayOrigin.y += 50f; // Start high up

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 100f, waterLayer))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint.y = transform.position.y;
        }
        
        GameObject baitObj = Instantiate(baitPrefab, spawnPoint.position, Quaternion.identity);
        fishingBait baitScript = baitObj.GetComponent<fishingBait>();

        if (baitScript != null)
        {
            baitScript.FlyToTarget(targetPoint);
        }
        else
        {
            Debug.LogError("[FishingCaster] Your Bait Prefab is missing the 'fishingBait' script!");
        }
        
        currentValue = 0;
        powerSlider.SetValueWithoutNotify(0);
        direction = 1;
    }
}