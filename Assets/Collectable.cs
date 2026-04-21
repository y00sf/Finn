using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public enum CollectableType
{
    Recyclables,
    Coins
}

[Serializable]
public class CollectablePickedUpEvent : UnityEvent<PlayerCollectableTracker, CollectableType, int>
{
}

[RequireComponent(typeof(Collider))]
public class Collectable : MonoBehaviour
{
    [Header("Collectable Data")]
    [SerializeField] private CollectableType type = CollectableType.Coins;
    [SerializeField] private int amount = 1;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool destroyOnCollect = true;

    [Header("Events")]
    [SerializeField] private CollectablePickedUpEvent onCollected;

    public CollectableType Type => type;
    public int Amount => amount;

    private bool hasBeenCollected;

    private void Reset()
    {
        Collider itemCollider = GetComponent<Collider>();

        if (itemCollider != null)
        {
            itemCollider.isTrigger = true;
        }
    }
    

    public void Collect(PlayerCollectableTracker tracker)
    {
        if (hasBeenCollected || tracker == null)
        {
            return;
        }

        hasBeenCollected = true;
        tracker.Add(type, amount);

        onCollected?.Invoke(tracker, type, amount);

        if (destroyOnCollect)
        {
            Destroy(gameObject);
        }
    }
}
