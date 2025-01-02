using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class TEST_BlueGhostController : MonoBehaviour
{
    public float wanderTimer = 2f;
    public bool isPlayerControlled = false;
    public float moveSpeed = 5f;
    public int tabuListSize = 2;
    public float exitDelay = 5f; // Time to wait before exiting the cage
    public Transform cageExitPoint; // Reference to the exit point outside the cage
    public bool useTabuList = true; // Toggle to enable/disable the tabu list

    private float decisionChance = 0.7f;
    private NavMeshAgent agent;
    private float timer;
    private Vector3 bufferedDirection = Vector3.zero;
    private Vector3 currentDirection = Vector3.zero;
    private Queue<Vector3> tabuList = new Queue<Vector3>();
    private bool isExitingCage = false;
    private bool hasExitedCage = false;
    private float exitTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        timer = wanderTimer;
        exitTimer = exitDelay; // Initialize the timer to start counting down for cage exit
        agent.isStopped = true;
    }

    void Update()
    {
        if (isPlayerControlled)
        {
            HandlePlayerControl();
        }
        else
        {
            if (isExitingCage)
            {
                MoveTowardsExit();  // Only move towards the exit if still in cage
            }
            else
            {
                HandleAIControl();  // Otherwise, handle AI control
                CountDownExitTimer();  // Start the exit countdown once the ghost has exited
            }
        }
    }

    void HandlePlayerControl()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
            bufferedDirection = Vector3.forward;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            bufferedDirection = Vector3.back;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            bufferedDirection = Vector3.left;
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            bufferedDirection = Vector3.right;

        if (bufferedDirection != Vector3.zero && CanMove(bufferedDirection))
        {
            currentDirection = bufferedDirection;
        }

        if (currentDirection != Vector3.zero)
        {
            transform.position += currentDirection * moveSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(currentDirection);
        }
    }

bool CanMove(Vector3 direction)
{
    float rayLength = 1f;
    
    // Define rays with different vertical offsets
    Ray ray1 = new Ray(transform.position, direction);
    Ray ray2 = new Ray(transform.position + Vector3.up * 0.2f, direction);
    Ray ray3 = new Ray(transform.position + Vector3.down * 0.2f, direction);

    // Draw the rays in the scene view (cyan color for visibility)
    Debug.DrawRay(transform.position, direction * rayLength, Color.cyan);
    Debug.DrawRay(transform.position + Vector3.up * 0.2f, direction * rayLength, Color.cyan);
    Debug.DrawRay(transform.position + Vector3.down * 0.2f, direction * rayLength, Color.cyan);

    // Check if any of the rays hit an obstacle
    return !Physics.Raycast(ray1, rayLength) && 
           !Physics.Raycast(ray2, rayLength) && 
           !Physics.Raycast(ray3, rayLength);
}

    void HandleAIControl()
    {
        if (currentDirection != Vector3.zero && !CanMove(currentDirection))
        {
            timer = wanderTimer;
        }

        timer += Time.deltaTime;

        if (timer >= wanderTimer)
        {
            timer = 0;

            Vector3[] possibleDirections = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
            Vector3[] openDirections = System.Array.FindAll(possibleDirections, CanMove);

            if (openDirections.Length > 1)
            {
                Vector3 selectedDirection = currentDirection;

                if (Random.value < decisionChance)
                {
                    List<Vector3> validDirections = new List<Vector3>();
                    foreach (var direction in openDirections)
                    {
                        if (useTabuList && !tabuList.Contains(direction))  // Only check tabu list if enabled
                        {
                            validDirections.Add(direction);
                        }
                    }

                    if (validDirections.Count == 0)
                    {
                        validDirections.AddRange(openDirections);
                    }

                    selectedDirection = validDirections[Random.Range(0, validDirections.Count)];
                }

                if (selectedDirection != currentDirection)
                {
                    AddToTabuList(selectedDirection);
                }

                bufferedDirection = selectedDirection;
            }
            else if (openDirections.Length == 1)
            {
                bufferedDirection = openDirections[0];
            }
        }

        if (bufferedDirection != Vector3.zero && CanMove(bufferedDirection))
        {
            currentDirection = bufferedDirection;
        }

        if (currentDirection != Vector3.zero)
        {
            transform.position += currentDirection * moveSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(currentDirection);
        }
    }

    void MoveTowardsExit()
    {
        if (Vector3.Distance(transform.position, cageExitPoint.position) > 0.5f)
        {
            Vector3 directionToExit = (cageExitPoint.position - transform.position).normalized;
            if (CanMove(directionToExit))
            {
                currentDirection = directionToExit;
                transform.position += currentDirection * moveSpeed * Time.deltaTime;
                transform.rotation = Quaternion.LookRotation(currentDirection);
            }
            HandleAIControl();
        }
        else
        {
            // Once the ghost has exited the cage, update the state
            isExitingCage = false;
            hasExitedCage = true;

            // Resume AI control after exiting the cage
            agent.isStopped = false;
        }
    }

    void CountDownExitTimer()
    {
        if (!hasExitedCage)  // Only start the countdown if the ghost is still in the cage
        {
            exitTimer -= Time.deltaTime;
            if (exitTimer <= 0f)
            {
                isExitingCage = true;  // Start the exit behavior after the timer finishes
            }
        }
    }

    void AddToTabuList(Vector3 direction)
    {
        if (tabuList.Count >= tabuListSize)
        {
            tabuList.Dequeue();
        }
        tabuList.Enqueue(direction);
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
            timer = wanderTimer;
        }

        CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.target = isPlayerControlled ? transform : null;
        }
    }
}
