using System;
using UnityEngine;

public class BaitInteract : MonoBehaviour
{

    [SerializeField] private FishingManager fishingManager;
    [SerializeField] private int baitIndex;
    
    void Start()
    {
        fishingManager = FindAnyObjectByType<FishingManager>();
    }
    
    void Update()
    {
        
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            fishingManager.baits[baitIndex].ResetToFull();
            Destroy(gameObject);
        }
    }
}
