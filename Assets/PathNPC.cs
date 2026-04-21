using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public struct Paths
{
    public string pathName;
    public WayPoint[] waypoints;
}

public class PathNPC : MonoBehaviour
{
    [SerializeField] private Paths[] paths;
    [SerializeField] private float NPCspeed;
    [SerializeField] private int currentPathIndex = 0;
    [SerializeField] private int currentWaypointIndex = 0;
    [SerializeField] private bool CanWalk = true;
    [SerializeField] private NavMeshAgent agent;

    private void Start()
    {
        if (paths == null || paths.Length == 0)
        {
            Debug.LogError("No paths assigned.", this);
            enabled = false;
            return;
        }

        if (paths[currentPathIndex].waypoints == null || paths[currentPathIndex].waypoints.Length == 0)
        {
            Debug.LogError($"Path '{paths[currentPathIndex].pathName}' has no waypoints assigned.", this);
            enabled = false;
        }

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
        
        agent.speed = NPCspeed;
        
        SetNextWaypoint();
    }

    private void Update()
    {
        if (CanWalk)
        {
            CheckWaypointProgress();
        }
    }

    private void CheckWaypointProgress()
    {
        if (currentWaypointIndex >= paths[currentPathIndex].waypoints.Length)
        {
            return;
        }
        
        if (agent.pathPending)
        {
            
            return;
        }
        if (agent.remainingDistance < 0.5f)
        {
            currentWaypointIndex++;
            SetNextWaypoint();
        }
        float random =  Random.Range(NPCspeed -0.5f, NPCspeed+ 0.5f);
        agent.speed = random;
    }

    private void SetNextWaypoint()
    {
        WayPoint[] waypoints = paths[currentPathIndex].waypoints;

        if (currentWaypointIndex >= waypoints.Length)
        {
            return;
        }

        agent.SetDestination(waypoints[currentWaypointIndex].transform.position);
        WayPoint waypoint = paths[currentPathIndex].waypoints[currentWaypointIndex];
        if (waypoint.WaitTime > 0)
        {
            WaitOnWaypoint(waypoint);
        }
        
    }

    
    public void SetCanWalk(bool canWalk)
    {
        CanWalk = canWalk;
        agent.isStopped = !canWalk;
    }
    public bool GetCanWalk()
    {
        return CanWalk;
    }

    private void WaitOnWaypoint(WayPoint waypoint)
    {
        StartCoroutine(Wait(waypoint.WaitTime));
    }

    private IEnumerator Wait(float waitTime)
    {
        SetCanWalk(false);
        yield return new WaitForSeconds(waitTime);
        SetCanWalk(true);
    }

    

    private void OnDrawGizmos()
    {
        if (paths == null || paths.Length == 0)
        {
            return;
        }

        if (currentPathIndex < 0 || currentPathIndex >= paths.Length)
        {
            return;
        }

        WayPoint[] waypoints = paths[currentPathIndex].waypoints;
        if (waypoints == null || waypoints.Length == 0)
        {
            return;
        }

        Gizmos.color = Color.magenta;
        

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null)
            {
                continue;
            }

            Vector3 currentPosition = waypoints[i].transform.position;
            Gizmos.DrawSphere(currentPosition, 0.2f);

            if (i + 1 < waypoints.Length && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(currentPosition, waypoints[i + 1].transform.position);
            }
        }
    }
}
