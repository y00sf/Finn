using System;
using UnityEngine;

public class InteractableItem : BaseInteractable
{
    [SerializeField] private GameObject[] itemsToEnable;
    [SerializeField] private float lookingTimeToEnable = 0.5f;

    private void Start()
    {
        interactionType = InteractionType.Item;
    }

    private void Update()
    {
        InteractItem();
    }

    protected override void InteractItem()
    {
        if (interactable != null)
        {
            lookingTimeToEnable -= Time.deltaTime;
            if (lookingTimeToEnable <= 0f)
            {
                foreach (GameObject item in itemsToEnable)
                {
                    item.SetActive(true);
                }
                enabled = false;
            }
        }
        else
        {
            lookingTimeToEnable = 0.5f;
        }
    }
}
