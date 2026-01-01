using System;
using UnityEngine;

public class Crab : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody rb;
    
    private Vector3 startPosition;
    private bool isResetting = false;
    
    private void Start()
    {
        startPosition = transform.position;
        
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (animator == null) animator = GetComponent<Animator>();
        
        InvokeRepeating(nameof(BackToStart), 10f, 10f);
    }
    
    private void Update()
    {
        CheckMovement();
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (isResetting) return;
        
        if (other.CompareTag("Player"))
        {
            Vector3 direction = other.transform.position - transform.position;
            Vector3 fleeDirection = new Vector3(-direction.x, 0, -direction.z).normalized;
            
            // Use velocity change with speed limiting
            if (rb.linearVelocity.magnitude < maxSpeed)
            {
                rb.AddForce(fleeDirection * speed, ForceMode.Acceleration);
            }
        }
    }
    
    private void CheckMovement()
    {
        animator.SetBool("Run", rb.linearVelocity.magnitude > 0.1f);
    }
    
    private void BackToStart()
    {
        if (isResetting) return;
        StartCoroutine(ResetPosition());
    }
    
    private System.Collections.IEnumerator ResetPosition()
    {
        isResetting = true;
        
        animator.SetTrigger("In");
        yield return new WaitForSeconds(0.5f); // Wait for animation
        
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = startPosition;
        
        yield return new WaitForSeconds(0.1f);
        
        animator.SetTrigger("Out");
        isResetting = false;
    }
    
    private void OnDestroy()
    {
        CancelInvoke();
    }
}