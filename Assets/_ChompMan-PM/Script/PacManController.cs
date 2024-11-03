using UnityEngine;
using UnityEngine.AI;

public class PacManController : MonoBehaviour
{
    public float wanderRadius = 10f; // How far Pac-Man can wander
    public float wanderTimer = 2f; // How long between each wander (lowered for quicker movements)

    private NavMeshAgent agent; // Reference to the NavMesh Agent
    private float timer; // Timer to track when to wander

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        timer = wanderTimer; // Initialize the timer
        Wander(); // Start wandering immediately
    }

    void Update()
    {
        // Increment timer by the time passed since the last frame
        timer += Time.deltaTime;

        // Check if the agent is close enough to its destination
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (timer >= wanderTimer)
            {
                Wander(); // Call the Wander function
                timer = 0; // Reset the timer
            }
        }
    }

    void Wander()
    {
        // Calculate a random position within a certain radius
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position; // Offset from the current position

        NavMeshHit hit; // This will store the result of the NavMesh hit
        NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas); // Sample a position on the NavMesh

        agent.SetDestination(hit.position); // Set the agent's destination to the new position
    }
}
