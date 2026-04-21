using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class CollectableItemChangedEvent : UnityEvent<CollectableType, int, int>
{
}

[Serializable]
public class CollectableItem
{
    [SerializeField] private string name = "Collectable";
    [SerializeField] private int amount;

    public CollectableItem()
    {
    }

    public CollectableItem(string name)
    {
        this.name = name;
    }

    public string Name => name;
    public int Amount => amount;

    public int Add(int value)
    {
        if (value <= 0)
        {
            return amount;
        }

        amount += value;
        return amount;
    }

    public int Subtract(int value)
    {
        if (value <= 0)
        {
            return 0;
        }

        int removedAmount = Mathf.Min(amount, value);
        amount -= removedAmount;
        return removedAmount;
    }

    public void ResetAmount()
    {
        amount = 0;
    }
}

public class PlayerCollectableTracker : MonoBehaviour
{
    [Header("Collectables")]
    [SerializeField] private CollectableItem recyclables = new("Recyclables");
    [SerializeField] private CollectableItem coins = new("Coins");

    [Header("Events")]
    [SerializeField] private CollectableItemChangedEvent onItemChanged;

    public event Action<CollectableType, int, int> ItemChanged;

    public int TotalCollected { get; private set; }

    public CollectableItem Recyclables => recyclables;
    public CollectableItem Coins => coins;

    private void Awake()
    {
        EnsureCollectables();
    }

    private void OnValidate()
    {
        EnsureCollectables();
    }

    public void Add(CollectableType type, int amount)
    {
        CollectableItem item = GetCollectable(type);
        int newAmount = item.Add(amount);

        if (amount > 0)
        {
            TotalCollected += amount;
        }

        ItemChanged?.Invoke(type, newAmount, TotalCollected);
        onItemChanged?.Invoke(type, newAmount, TotalCollected);
    }

    public void Subtract(CollectableType type, int amount)
    {
        CollectableItem item = GetCollectable(type);
        int removedAmount = item.Subtract(amount);

        if (removedAmount > 0)
        {
            TotalCollected -= removedAmount;
        }

        ItemChanged?.Invoke(type, item.Amount, TotalCollected);
        onItemChanged?.Invoke(type, item.Amount, TotalCollected);
    }

    public int GetAmount(CollectableType type)
    {
        return GetCollectable(type).Amount;
    }

    public CollectableItem GetCollectable(CollectableType type)
    {
        return type switch
        {
            CollectableType.Recyclables => recyclables,
            CollectableType.Coins => coins,
            _ => coins
        };
    }

    public void ClearCollectedItems()
    {
        recyclables.ResetAmount();
        coins.ResetAmount();
        TotalCollected = 0;
    }

    private void EnsureCollectables()
    {
        recyclables ??= new CollectableItem("Recyclables");
        coins ??= new CollectableItem("Coins");
    }
}
