using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(NavMeshAgent))]
public class PacManController : MonoBehaviour
{
    public float wanderRadius = 10f; // Radius for random movement
    public float wanderTimer = 2f; // Time interval for wandering
    public int lives = 3; // Pac-Man lives
    public Transform spawnPoint; // Respawn point for Pac-Man

    private NavMeshAgent agent;
    private float timer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        SphereCollider sphereCollider = GetComponent<SphereCollider>();

        // Ensure SphereCollider is a trigger
        sphereCollider.isTrigger = true;

        timer = wanderTimer;
        Wander();
    }

    void Update()
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

    // Public method to handle losing a life
    public void LoseLife()
    {
        lives--; // Reduce lives

        if (lives > 0)
        {
            Debug.Log($"PacMan lost a life! Lives remaining: {lives}");
            // Respawn Pac-Man at the spawn point
            transform.position = spawnPoint.position;
            agent.ResetPath();
            Wander();
        }
        else
        {
            Debug.Log("PacMan has no lives left. Game Over!");
            // Load the GameOver scene
            SceneManager.LoadScene("GameOver"); // Replace "GameOver" with your actual scene name
        }
    }
}
