using UnityEngine;
using UnityEngine.AI;

// Base class for ghost behavior
public class GhostControllerBase : MonoBehaviour
{
    public float wanderRadius = 10f; // Wander radius for AI movement
    public float wanderTimer = 2f; // Time interval for AI wandering
    public bool isPlayerControlled = false; // Determines if the ghost is controlled by the player

    protected NavMeshAgent agent; // Reference to the NavMesh Agent
    private float timer; // Timer to track wandering

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
            HandlePlayerControl();
        }
        else
        {
            HandleAIBehavior();
        }
    }

    // Common player control logic
    protected virtual void HandlePlayerControl()
    {
        float moveSpeed = 5f; // Adjust as needed
        float moveHorizontal = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float moveVertical = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

        Vector3 moveDirection = new Vector3(moveHorizontal, 0, moveVertical);

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            transform.position += moveDirection;
        }
    }

    // Common AI behavior logic
    private void HandleAIBehavior()
    {
        timer += Time.deltaTime;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (timer >= wanderTimer)
            {
                Wander();
                timer = 0;
            }
        }
    }

    // Common wandering logic
    protected void Wander()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas);

        agent.SetDestination(hit.position);
    }

    public void ToggleControl(bool isControlled)
    {
        isPlayerControlled = isControlled;

        if (isPlayerControlled)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
        else
        {
            Wander();
            agent.isStopped = false;
        }

        CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.target = isPlayerControlled ? transform : null;
        }
    }
}