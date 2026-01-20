using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class FishingSpot : MonoBehaviour
{
    [SerializeField] private float flightTime = 1.5f;
    [SerializeField] private float arcHeight = 5.0f;
    [SerializeField] private float biomeDetectionRadius = 1f;
    [SerializeField] private LayerMask waterLayer;

    [Header("References")]
    [SerializeField] private FishingManager fishingManager;

    public UnityEvent OnLanded;

    private void Start()
    {
        if (fishingManager == null)
        {
            fishingManager = FindObjectOfType<FishingManager>();
        }
    }

    public void FlyToTarget(Vector3 targetPos)
    {
        StartCoroutine(MoveRoutine(targetPos));
    }

    private IEnumerator MoveRoutine(Vector3 destination)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < flightTime)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / flightTime;

            Vector3 currentPos = Vector3.Lerp(startPos, destination, percent);
            float height = Mathf.Sin(percent * Mathf.PI) * arcHeight;
            currentPos.y += height;

            transform.position = currentPos;
            yield return null;
        }

        transform.position = destination;

    
        DetectBiome();

      
        OnLanded?.Invoke();

      
        if (fishingManager != null)
        {
           // fishingManager.OnBaitLanded();
        }
    }

    private void DetectBiome()
    {
      
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, 10f, waterLayer))
        {
            BiomeType detectedBiome = GetBiomeFromTag(hit.collider.tag);

            if (fishingManager != null)
            {
                fishingManager.SetCurrentBiome(detectedBiome);
            }

            Debug.Log($"Bait landed in: {detectedBiome} (Tag: {hit.collider.tag})");
        }
        else
        {
            
            Collider[] colliders = Physics.OverlapSphere(transform.position, biomeDetectionRadius, waterLayer);

            if (colliders.Length > 0)
            {
                BiomeType detectedBiome = GetBiomeFromTag(colliders[0].tag);

                if (fishingManager != null)
                {
                    fishingManager.SetCurrentBiome(detectedBiome);
                }

                Debug.Log($"Bait landed in: {detectedBiome} (Tag: {colliders[0].tag})");
            }
            else
            {
                Debug.LogWarning("Could not detect biome - no water found!");
            }
        }
    }

 
    private BiomeType GetBiomeFromTag(string tag)
    {
        return tag switch
        {
            "IceWater" => BiomeType.IceBiome,
            "VolcanoWater" => BiomeType.VolcanoBiome,
            "WildeWater" => BiomeType.WiledBiome,
            _ => BiomeType.WiledBiome // Default fallback
        };
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, biomeDetectionRadius);
    }
}