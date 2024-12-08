using UnityEngine;
using UnityEngine.AI;

public class DebuggingNavMeshAgent : MonoBehaviour
{
    private NavMeshAgent agent;

    public Transform target; // Assign a target GameObject in the Inspector

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Log initial agent configuration
        Debug.Log($"NavMeshAgent initialized. Speed: {agent.speed}, Radius: {agent.radius}, Stopping Distance: {agent.stoppingDistance}");

        // Set initial destination
        if (target != null)
        {
            SetNewDestination(target.position);
        }
        else
        {
            Debug.LogWarning("No target assigned! Please assign a target in the Inspector.");
        }
    }

    void Update()
    {
        // Log current path status and remaining distance
        if (agent.hasPath)
        {
            Debug.Log($"Path Status: {agent.pathStatus}, Remaining Distance: {agent.remainingDistance}");
        }
        else if (!agent.hasPath && agent.pathPending)
        {
            Debug.Log("Path is pending...");
        }
        else
        {
            Debug.LogWarning("Agent has no path!");
        }

        // Draw the current path for debugging
        DrawPath(agent);
    }

    private void SetNewDestination(Vector3 position)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(position, out hit, 10f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            Debug.Log($"Setting destination to: {hit.position}");
        }
        else
        {
            Debug.LogError($"Failed to find a valid NavMesh position near: {position}");
        }
    }

    private void DrawPath(NavMeshAgent navAgent)
    {
        if (navAgent.hasPath)
        {
            Gizmos.color = Color.green;
            Vector3 prevCorner = transform.position;

            foreach (var corner in navAgent.path.corners)
            {
                Debug.DrawLine(prevCorner, corner, Color.green);
                Gizmos.DrawSphere(corner, 0.1f);
                prevCorner = corner;
            }
        }
    }

    void OnDrawGizmos()
    {
        // Visualize the agent's radius
        if (agent != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, agent.radius);
        }
    }

    // Test setting a new destination via key press
    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.T) && target != null)
        {
            SetNewDestination(target.position);
        }
    }
}
