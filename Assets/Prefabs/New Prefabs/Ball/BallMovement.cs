using System;
using UnityEngine;

public class BallMovement : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float timeToRespawn;
    [SerializeField] private float farthestDistance;
    private float last;
    void Update()
    {
        last += Time.deltaTime;
        if (last > timeToRespawn)
        {
            if((transform.position - spawnPoint.position).magnitude > farthestDistance)
            {
                transform.position = spawnPoint.position;
            }
            last = 0;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            last = 0;
        }
    }
    
    
}
