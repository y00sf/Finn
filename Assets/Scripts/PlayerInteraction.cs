using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Transform interactionTransform;
    [SerializeField] private float interactionDistance = 0.5f;
    [SerializeField] private float controllerSensitivity = 60f;
    [SerializeField] private float keyboardSensitivity = 15f;
    [SerializeField] [CanBeNull] private GameObject interactionUI;
    [SerializeField] [Range(0f, 1f)] private float forwardBias = 0.3f; // 0.3f = ~70 degree cone
    
    //private PlayerInputActions inputActions;
    public BaseInteractable interactable;
    
    
    private void Awake()
    {
        //inputActions = new PlayerInputActions();
    }
    
    private void OnEnable()
    {
        //inputActions.Player.Enable();
    }
    
    private void OnDisable()
    {
        //inputActions.Player.Disable();
    }
    
    private void Update()
    {
        SphereCheck();
        //DetectInputDevice();
        
        if (interactable != null && interactionUI != null)
        {
            interactable.interactable = interactable;
            // Show interaction UI for all interactables except InteractableItem
            if (interactable.interactionType != InteractionType.Item)
            {
                interactionUI?.SetActive(true);
            }
            else
            {
                interactionUI?.SetActive(false);
            }
        }
        else
        {
            if (interactionUI != null) interactionUI.SetActive(false);
        }
        /*
        if (inputActions.Player.Interact.WasPressedThisFrame())
        {
            interactable?.Interact();
        }
        */
        
        if(Input.GetKeyDown(KeyCode.E))
        {
            interactable?.Interact();
        }
    }
    
    /*
    private void DetectInputDevice()
    {
        if (cameraMove == null) return;
        
        // Check the last used device for the Look action
        var lookAction = inputActions.Player.Look;
        
        if (lookAction.activeControl != null)
        {
            var device = lookAction.activeControl.device;
            
            // Check if the device is a gamepad/controller
            if (device is Gamepad)
            {
                cameraMove.mouseSensitivity = controllerSensitivity;
            }
            // Check if the device is keyboard & mouse
            else if (device is Mouse || device is Keyboard)
            {
                cameraMove.mouseSensitivity = keyboardSensitivity;
            }
        }
    }
    */
    
    private void SphereCheck()
    {
        Collider[] hits = Physics.OverlapSphere(interactionTransform.position, interactionDistance);
        
        BaseInteractable closest = null;
        float closestDistance = float.MaxValue;
        
        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent(out BaseInteractable interactableComponent))
            {
                float distance = Vector3.Distance(interactionTransform.position, hit.transform.position);
                
                // Check if it's in front of the player (optional but recommended)
                Vector3 directionToTarget = (hit.transform.position - interactionTransform.position).normalized;
                float dotProduct = Vector3.Dot(interactionTransform.forward, directionToTarget);
                
                // Only consider objects in front and pick the closest one
                if (dotProduct > forwardBias && distance < closestDistance)
                {
                    closest = interactableComponent;
                    closestDistance = distance;
                }
            }
        }
        
        // Update interactable reference if it changed
        if (interactable != closest)
        {
            interactable?.UnInteract();
            interactable = closest;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (interactionTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(interactionTransform.position, interactionDistance);
            
            // Draw forward direction
            Gizmos.color = Color.red;
            Gizmos.DrawRay(interactionTransform.position, interactionTransform.forward * interactionDistance);
        }
    }
}