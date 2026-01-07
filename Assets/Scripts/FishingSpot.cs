using UnityEngine;
using UnityEngine.Events;

public class FishingSpot : MonoBehaviour
{
    
    public bool isAvailable = true;
    
    public float interactionDistance = 20f;
    public FishingMiniGame miniGameManager;
    

    private void Start()
    {
        miniGameManager = FindAnyObjectByType<FishingMiniGame>();
    }

    
    public void Interact(Transform playerTransform)
    {
     
        if (!isAvailable)
        {
            Debug.Log("Unavailable fishing spot");
            return;
        }

       
        if (interactionDistance > 0)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            if (distance > interactionDistance)
            {
                Debug.Log("too far");
                return;
            }
        }

        // 3. Trigger the Fishing Mechanics
        StartFishingSequence();
    }

    private void StartFishingSequence()
    {
        Debug.Log($"Fishing started");
        
        isAvailable = false;
        
        miniGameManager.StartFishing(this);
        
        if (miniGameManager != null)
        {
            miniGameManager.StartFishing(this);
        }
        else
        {
            Debug.LogError("FishingMiniGame Manager not found in scene!");
        }
    }
   
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }

    public void DistroyFishingSpot()
    {
        Debug.Log("Destroying Fishing Spot");
        Destroy(gameObject);
    }
}