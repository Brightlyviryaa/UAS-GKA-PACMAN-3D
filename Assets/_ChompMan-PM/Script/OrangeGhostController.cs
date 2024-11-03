using UnityEngine;
using UnityEngine.AI;

public class OrangeGhostController : MonoBehaviour
{
    public float wanderRadius = 10f; // Wander radius for AI movement
    public float wanderTimer = 2f; // Time interval for AI wandering
    public bool isPlayerControlled = false; // Determines if the ghost is controlled by the player

    private NavMeshAgent agent; // Reference to the NavMesh Agent
    private float timer; // Timer to track wandering
    public CameraFollow cameraFollow; // Reference to the CameraFollow script

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        timer = wanderTimer; // Initialize the timer

        if (!isPlayerControlled)
        {
            Wander(); // Start wandering immediately if AI-controlled
        }
    }

    void Update()
    {
        if (isPlayerControlled)
        {
            // Handle player control logic here
            HandlePlayerControl();
        }
        else
        {
            // Handle AI behavior
            timer += Time.deltaTime;

            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                if (timer >= wanderTimer)
                {
                    Wander(); // Call the Wander function
                    timer = 0; // Reset the timer
                }
            }
        }
    }

    void HandlePlayerControl()
    {
        // Implement player control logic (e.g., keyboard input)
        // Example: Move ghost with arrow keys
        float moveSpeed = 5f; // Adjust as needed
        float moveHorizontal = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float moveVertical = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

        Vector3 moveDirection = new Vector3(moveHorizontal, 0, moveVertical);

        // Only rotate if there is movement
        if (moveDirection != Vector3.zero)
        {
            // Calculate the new rotation
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            // Smoothly rotate towards the target rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            
            // Move the ghost in the desired direction
            transform.position += moveDirection;
        }
    }


    void Wander()
    {
        // Calculate a random position within the wander radius
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position; // Offset from the current position

        NavMeshHit hit; // This will store the result of the NavMesh hit
        NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas); // Sample a position on the NavMesh

        agent.SetDestination(hit.position); // Set the agent's destination to the new position
    }

    // Method to toggle control between player and AI

    public void ToggleControl(bool isControlled)
    {
        isPlayerControlled = isControlled; // Set the control state

        if (isPlayerControlled)
        {
            // Stop AI behavior if controlled by the player
            agent.isStopped = true; // Stop the NavMesh agent
            // Optionally, you might want to clear any existing AI destination
            agent.ResetPath();
        }
        else
        {
            // Start AI wandering if control is toggled off
            Wander(); // Start AI behavior
            agent.isStopped = false; // Resume NavMesh agent movement
        }

        // Set the camera target only if controlled
        CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.target = isPlayerControlled ? transform : null; // Set target only if controlled
        }
    }
}
