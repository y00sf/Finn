using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class fishingBait : MonoBehaviour
{
    [SerializeField] private float flightTime = 1.5f;
    [SerializeField] private float arcHeight = 5.0f;
    [SerializeField] private float biomeDetectionRadius = 1f;
    [SerializeField] private LayerMask waterLayer;

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

        var fm = FishingManager.Instance;
        if (fm != null)
        {
        
            fm.OnBaitLanded(); 
        }
        else
        {
            Debug.LogError("[fishingBait] FishingManager Instance is NULL.");
        }
        
        Destroy(gameObject, 0.5f);
    }

    private void DetectBiome()
    {
        var fm = FishingManager.Instance;
        if (fm == null) return;

      
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, 10f, waterLayer))
        {
            Debug.Log(hit.collider.gameObject.name);
            fm.SetCurrentBiome(GetBiomeFromTag(hit.collider.tag));
        }
      
        else
        {
            Collider[] cols = Physics.OverlapSphere(transform.position, biomeDetectionRadius, waterLayer);
            if (cols.Length > 0)
                fm.SetCurrentBiome(GetBiomeFromTag(cols[0].tag));
        }
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