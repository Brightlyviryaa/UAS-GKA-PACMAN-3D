using UnityEngine;
using UnityEngine.AI;

public class BlueGhostController : MonoBehaviour
{
    public float wanderRadius = 10f;
    public float wanderTimer = 2f;
    public bool isPlayerControlled = false;
    public float moveSpeed = 5f;

    private NavMeshAgent agent;
    private float timer;
    private Vector3 bufferedDirection = Vector3.zero;
    private Vector3 currentDirection = Vector3.zero;

    void Start()
    {
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
            HandleAIControl();
        }
    }

    void HandlePlayerControl()
    {
        // Buffer new direction on key press
        if (Input.GetKeyDown(KeyCode.UpArrow))
            bufferedDirection = Vector3.forward;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            bufferedDirection = Vector3.back;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            bufferedDirection = Vector3.left;
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            bufferedDirection = Vector3.right;

        // Check if buffered direction is valid
        if (bufferedDirection != Vector3.zero && CanMove(bufferedDirection))
        {
            currentDirection = bufferedDirection; // Update to the new direction
        }

        // Continue moving in the current direction
        if (currentDirection != Vector3.zero)
        {
            transform.position += currentDirection * moveSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(currentDirection);
        }
    }

bool CanMove(Vector3 direction)
{
    float rayLength = 1f; // Adjust for wall proximity
    Ray ray1 = new Ray(transform.position, direction);
    Ray ray2 = new Ray(transform.position + Vector3.up * 0.2f, direction); // Offset slightly upwards to avoid the ground
    Ray ray3 = new Ray(transform.position + Vector3.down * 0.2f, direction); // Offset slightly downwards

    // Check if any of the rays hit an obstacle
    return !Physics.Raycast(ray1, rayLength) && 
           !Physics.Raycast(ray2, rayLength) && 
           !Physics.Raycast(ray3, rayLength);
}


    void HandleAIControl()
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

    void Wander()
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
            agent.isStopped = false;
            Wander();
        }

        CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.target = isPlayerControlled ? transform : null;
        }
    }
}
