using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Transform interactionTransform;
    [SerializeField] private float interactionDistance = 0.5f;
    [SerializeField] [CanBeNull] private GameObject interactionUI;
    [SerializeField] private bool enableHeadTracking = true;
    [SerializeField] private Transform PlayerHead;
    [SerializeField] private float headRotationSpeed = 5f;
    [SerializeField] private float maxHeadTurnAngle = 70f;
    public BaseInteractable interactable;
    public InputAction PlayerInputActions;

    private void OnEnable() => PlayerInputActions.Enable();
    private void OnDisable() => PlayerInputActions.Disable();

    private void Update()
    { 
        SphereCheck();
        HandleInteractionUI(); 
        if (PlayerInputActions.WasPerformedThisFrame())
        {
            Debug.Log($"E KEY PRESSED! Interactable = {(interactable != null ? interactable.gameObject.name : "NULL")}");
            interactable?.Interact();
            if (interactable?.interactionType == InteractionType.NPC)
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                Animator anim = GetComponent<Animator>();
                rb.linearVelocity = Vector3.zero;
                anim.SetFloat("Speed", 0);
            }
            
        }
    }
    
    private void LateUpdate()
    {
        if (enableHeadTracking)
        {
            RotateHeadToInteractable();
        }
    }
    
    private void RotateHeadToInteractable()
    {
        if (PlayerHead == null) return;

        Quaternion finalTargetRotation;

        
        if (interactable != null)
        {
            
            Vector3 directionToTarget = interactable.transform.position - PlayerHead.position;
        
            
            directionToTarget.y = 0;
        
            
            if (directionToTarget.sqrMagnitude < 0.001f) directionToTarget = transform.forward;

            
            Quaternion idealLookRotation = Quaternion.LookRotation(directionToTarget);
            
            finalTargetRotation = Quaternion.RotateTowards(
                transform.rotation, 
                idealLookRotation, 
                maxHeadTurnAngle
            );
        }
       
        else
        {
           
            finalTargetRotation = transform.rotation;
        }
        
        PlayerHead.rotation = Quaternion.Slerp(
            PlayerHead.rotation, 
            finalTargetRotation, 
            Time.deltaTime * headRotationSpeed
        );
    }

    private void HandleInteractionUI()
    {
        if (interactable != null && interactionUI != null)
        {
            interactable.interactable = interactable;
            if (interactable.interactionType != InteractionType.Item)
            {
                interactionUI.SetActive(true);
            }
            else
            {
                interactionUI.SetActive(false);
            }
        }
        else
        {
            if (interactionUI != null) interactionUI.SetActive(false);
        }
    }
    
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
            
                if (distance < closestDistance)
                {
                    closest = interactableComponent;
                    closestDistance = distance;
                }
            }
        }
        
        if (interactable != closest)
        {
            if (interactable != null) interactable.UnInteract();
    
            interactable = closest;
            
            closest?.UnInteract();
            interactable = closest;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (interactionTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(interactionTransform.position, interactionDistance);
            
            Gizmos.color = Color.red;
            Gizmos.DrawRay(interactionTransform.position, interactionTransform.forward * interactionDistance);
        }
    }
}