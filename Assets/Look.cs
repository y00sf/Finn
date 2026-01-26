using Unity.VisualScripting;
using UnityEngine;

public class Look : MonoBehaviour
{ 
    [SerializeField] private bool enableHeadTracking = true;
  [SerializeField] private Transform Head;
  [SerializeField] private Transform player;
  [SerializeField] private float headRotationSpeed = 5f;
  [SerializeField] private float maxHeadTurnAngle = 70f;
  [SerializeField] private bool IsplayerInside = false;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void LateUpdate()
    {
        RotateHeadToInteractable();
    }
    
    private void RotateHeadToInteractable()
    {
        if (Head == null) return;

        Quaternion finalTargetRotation;

        
        if (IsplayerInside)
        {
            
            Vector3 directionToTarget = player.position - Head.position;
        
            
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
        
        Head.rotation = Quaternion.Slerp(
            Head.rotation, 
            finalTargetRotation, 
            Time.deltaTime * headRotationSpeed
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IsplayerInside = true;
        }
    }

   
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            
            IsplayerInside = false;
        }
    }
}
