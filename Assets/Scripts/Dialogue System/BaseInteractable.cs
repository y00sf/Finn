using UnityEngine;
public enum InteractionType
{
    Door,
    Item,
    Toggle,
    NPC,
    Sit,
    Sign,
    Collectable,
}

public class BaseInteractable : MonoBehaviour
{
     public InteractionType interactionType;
    [HideInInspector] public BaseInteractable interactable;
    public void Interact()
    {
        switch (interactionType)
        {
            case InteractionType.Door:
                InteractDoor();
                break;
            case InteractionType.Item:
                InteractItem();
                break;
            case InteractionType.Toggle:
                InteractToggle();
                break;
            case InteractionType.NPC:
                InteractNPC();
                break;
            case InteractionType.Sit:
                InteractSit();
                break;
            case InteractionType.Sign:
                InteractSign();
                break;
            case InteractionType.Collectable:
                InteractCollectable();
                break;
                
            default:
                Debug.LogWarning("Unknown interaction type");
                break;
        }
    }
    public void UnInteract()
    {
        switch (interactionType)
        {
            case InteractionType.Door:
                return;
                break;
            case InteractionType.Item:
                return;
                break;
            case InteractionType.Toggle:
                UnInteractToggle();
                break;
            case InteractionType.NPC:
                return;
                break;
            case InteractionType.Sit:
                return;
                break;
            case InteractionType.Collectable:
                return;
                break;
            default:
                Debug.LogWarning("Unknown interaction type");
                break;
        }
    }

    protected virtual void InteractDoor()
    {
        Debug.Log("Interacted with door");
    }

    protected virtual void InteractItem()
    {
        Debug.Log("Interacted with item");
    }

    protected virtual void InteractToggle()
    {
        Debug.Log("Interacted with toggle");
    }

    protected virtual void UnInteractToggle()
    {
        Debug.Log("Uninteracted with toggle");
    }

    protected virtual void InteractNPC()
    {
        Debug.Log("Interacted with NPC");
    }

    protected virtual void InteractSit()
    {
       GetComponent<Sit>().ToggleSit();
    }

    protected virtual void InteractSign()
    {
        GetComponent<Sign>().ShowSign();
    }

    protected virtual void InteractCollectable()
    {
        PlayerCollectableTracker tracker = FindObjectOfType<PlayerCollectableTracker>();
        
        GetComponent<Collectable>().Collect(tracker);
    }

}
