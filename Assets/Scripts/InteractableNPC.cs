using System;
using UnityEngine;
using UnityEngine.Events;

public class InteractableNPC : BaseInteractable
{
    public UnityEvent onInteractNPC;

    private void Start()
    {
        interactionType = InteractionType.NPC;
    }

    protected override void InteractNPC()
    {
        onInteractNPC?.Invoke();
    }
}
