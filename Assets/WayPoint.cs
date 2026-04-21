using UnityEngine;

public class WayPoint : MonoBehaviour
{
    public Transform waypoint;
    [SerializeField] private float waitTime = 0f;
    public float WaitTime
    {
        get => waitTime;
        set => waitTime = value;
    }
  

    private void Awake()
    {
        waypoint = transform;
        if (waypoint == null)
        {
            Debug.LogError("Waypoint not assigned.", this);
           
        }
    }
}
