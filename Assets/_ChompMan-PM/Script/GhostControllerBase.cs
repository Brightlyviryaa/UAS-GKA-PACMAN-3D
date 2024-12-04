using UnityEngine;
using UnityEngine.AI;

// Base class for ghost behavior
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class GhostControllerBase : MonoBehaviour
{
    public float wanderRadius = 10f; // Wander radius for AI movement
    public float wanderTimer = 2f; // Time interval for AI wandering
    public bool isPlayerControlled = false; // Determines if the ghost is controlled by the player

    protected NavMeshAgent agent; // Reference to the NavMesh Agent
    private float timer; // Timer to track wandering

    void Start()
    {
        // Add this to ensure CapsuleCollider is a trigger
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        capsuleCollider.isTrigger = false;

        // Rest of the Start logic
        agent = GetComponent<NavMeshAgent>();
        timer = wanderTimer;

        if (!isPlayerControlled)
        {
            Wander();
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PacMan")) // Ensure the tag is correct
        {
            Debug.Log($"Ghost {gameObject.name} collided with PacMan.");

            // Get PacManController script to call the LoseLife method
            PacManController pacMan = other.GetComponent<PacManController>();
            if (pacMan != null)
            {
                pacMan.LoseLife(); // Call the method to reduce life
            }
            else
            {
                Debug.LogError("PacManController script is missing on PacMan!");
            }
        }
        else
        {
            Debug.Log($"Collision ignored with {other.name}, tag: {other.tag}");
        }
    }

}