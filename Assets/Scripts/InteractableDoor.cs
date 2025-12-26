using System;
using UnityEngine;

public class InteractableDoor : BaseInteractable
{
    public Vector3 fromPosition  = new Vector3(0f, 0f, 0f);
    public Vector3 toPosition = new Vector3(0f, 90f, 0);
    
    public bool isMoving = false;
    public bool isOpen = false;
    float duration = 1f;

    private void Start()
    {
        interactionType = InteractionType.Door;
    }

    protected override void InteractDoor()
    {
        if (isMoving)
        {
            isOpen = !isOpen;
        }
        isMoving = true;
    }
    
    private void Update()
    {
        if (isMoving)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(isOpen ? fromPosition : toPosition), Time.deltaTime * (1 / duration));
            if (Quaternion.Angle(transform.localRotation, Quaternion.Euler(isOpen ? fromPosition : toPosition)) < 0.1f)
            {
                isMoving = false;
                isOpen = !isOpen;
                transform.localRotation = Quaternion.Euler(isOpen ? toPosition : fromPosition);
            }
        }
    }
}
