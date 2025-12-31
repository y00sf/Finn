using UnityEngine;
using UnityEngine.Events;

public class FishingSpot : MonoBehaviour
{
    
    public bool isAvailable = true;
    
    public float interactionDistance = 20f;
    
    public UnityEvent OnMiniGameStart;

    private void Start()
    {
       
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
        
        OnMiniGameStart.Invoke();
    }

   


    

   
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}