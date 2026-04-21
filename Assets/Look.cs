using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Look : MonoBehaviour
{ 
    [SerializeField] private bool enableHeadTracking = true;
  [SerializeField] private Transform Head;
  [SerializeField] private Transform player;
  [SerializeField] private float headRotationSpeed = 5f;
  [SerializeField] private float npcRotationSpeed = 360f;
  [SerializeField] private float maxHeadTurnAngle = 70f;
  [SerializeField] private bool IsplayerInside = false;
    private bool isRotatingNpcToPlayer = false;
    private bool isReturningNpcRotation = false;
    private bool wasDialogueRunning = false;
    private bool originalAgentUpdateRotation = true;
    private Quaternion originalNpcRotation;
    private NavMeshAgent agent;
    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void LateUpdate()
    {
        RotateHeadToInteractable();

        bool isDialogueRunning = ConversationManager.Instance != null && ConversationManager.Instance.IsDialogueRunning();

        if (wasDialogueRunning && !isDialogueRunning)
        {
            isRotatingNpcToPlayer = false;
            isReturningNpcRotation = true;
        }

        if (isRotatingNpcToPlayer)
        {
            RotateNpcTowardsPlayer();
        }
        else if (isReturningNpcRotation)
        {
            ReturnNpcToOriginalRotation();
        }

        wasDialogueRunning = isDialogueRunning;
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

    public void RotateNpcToPlayer()
    {
        if (player == null) return;

        originalNpcRotation = transform.rotation;

        if (agent != null)
        {
            originalAgentUpdateRotation = agent.updateRotation;
            agent.updateRotation = false;
        }

        isReturningNpcRotation = false;
        isRotatingNpcToPlayer = true;
    }

    private void RotateNpcTowardsPlayer()
    {
        if (player == null)
        {
            RestoreAgentRotation();
            isRotatingNpcToPlayer = false;
            return;
        }

        Vector3 directionToTarget = player.position - transform.position;
        directionToTarget.y = 0f;

        if (directionToTarget.sqrMagnitude < 0.001f)
        {
            isRotatingNpcToPlayer = false;
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            npcRotationSpeed * Time.deltaTime
        );

        if (Quaternion.Angle(transform.rotation, targetRotation) < 0.5f)
        {
            transform.rotation = targetRotation;
            isRotatingNpcToPlayer = false;
        }
    }

    private void ReturnNpcToOriginalRotation()
    {
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            originalNpcRotation,
            npcRotationSpeed * Time.deltaTime
        );

        if (Quaternion.Angle(transform.rotation, originalNpcRotation) < 0.5f)
        {
            transform.rotation = originalNpcRotation;
            isReturningNpcRotation = false;
            RestoreAgentRotation();
        }
    }

    private void RestoreAgentRotation()
    {
        if (agent != null)
        {
            agent.updateRotation = originalAgentUpdateRotation;
        }
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
