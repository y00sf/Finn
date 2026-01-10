using System.Collections;
using UnityEngine;

public class FishingSpotManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject fishingSpotPrefab;
    [SerializeField] private int maxSpots = 3; 
    [SerializeField] public int CurrentSpots = 1;
    
    [Header("Spawn Areas")]
    [SerializeField] private BoxCollider[] spawnZones;
    
    [SerializeField] private int waitTime = 3;

   

    void Start()
    {
        SpawnRandomSpot();
    }

    void Update()
    {
        
    }

    public void SpawnRandomSpot()
    {
        if (CurrentSpots <= maxSpots)
        {
            StartCoroutine(SpawnRandomSpotCoroutine());
        }
        else
        {
            Debug.Log("Max spots reached");
        }
    }

    private IEnumerator SpawnRandomSpotCoroutine()
    {
        yield return new WaitForSeconds(waitTime);
        if (spawnZones.Length == 0 || fishingSpotPrefab == null)
        {
            Debug.LogWarning("Missing Spawn Zones or Prefab in FishingSpotManager");
            yield break;
        }
        
        int randomIndex = Random.Range(0, spawnZones.Length);
        BoxCollider selectedZone = spawnZones[randomIndex];
        
        Vector3 randomPosition = GetRandomPointInBounds(selectedZone.bounds);
        
        Instantiate(fishingSpotPrefab, randomPosition, selectedZone.transform.rotation);
        CurrentSpots++;
        
        Debug.Log($"Spawned Fishing Spot in Zone {randomIndex}");
    }

    private Vector3 GetRandomPointInBounds(Bounds bounds)
    {
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);
        
        float fixedY = bounds.center.y; 
        return new Vector3(randomX, fixedY, randomZ);
    }
}
