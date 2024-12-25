using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using BT_PacMan;

[RequireComponent(typeof(NavMeshAgent))]
public class PacManController : MonoBehaviour
{
    public enum State { Aggressive, Defensive, Safe }

    [Header("Settings")]
    public float wanderRadius = 10f;
    public float regularGhostAvoidanceRadius = 5f;
    public float emergencyGhostAvoidanceRadius = 2f;
    public float predictionTime = 2f;
    public int lives = 3;
    public LayerMask pelletLayer;
    public LayerMask ghostLayer;
    public float stoppingDistance = 0.5f;
    public float ghostDetectionRadius = 8f; // Radius untuk deteksi ghost saat powered up

    [Header("Utility Weights")]
    public float distanceWeight = 1f;
    public float ghostProximityWeight = 2f;

    private NavMeshAgent agent;
    private BehaviorTree behaviorTree;
    private bool hasTarget = false;
    private bool isInEmergency = false;

    // Public agar dapat diakses dari luar jika diperlukan
    public bool isPoweredUp = false;
    private Coroutine powerRoutine;

    [SerializeField]
    public Transform spawnPoint;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        InitializeBehaviorTree();
        behaviorTree.Tick();
    }

    void Update()
    {
        bool emergencyGhostPresent = Physics.OverlapSphere(transform.position, emergencyGhostAvoidanceRadius, ghostLayer).Length > 0;

        if (emergencyGhostPresent && !isInEmergency)
        {
            Debug.Log("Emergency ghost detected. Re-evaluating target.");
            isInEmergency = true;
            behaviorTree.Tick();
        }
        else if (!emergencyGhostPresent && isInEmergency)
        {
            isInEmergency = false;
        }

        if (hasTarget && !agent.pathPending && agent.remainingDistance <= stoppingDistance && agent.destination != Vector3.zero)
        {
            Debug.Log("Reached target.");
            agent.ResetPath();
            hasTarget = false;
            behaviorTree.Tick();
        }

        if (!hasTarget && (agent.destination == Vector3.zero || agent.remainingDistance <= stoppingDistance))
        {
            behaviorTree.Tick();
        }
    }

    void InitializeBehaviorTree()
    {
        Selector root = new Selector();

        // Emergency Avoidance
        Sequence emergencyAvoidSequence = new Sequence();
        emergencyAvoidSequence.AddChild(new ConditionNode(() => IsGhostWithinRadius(emergencyGhostAvoidanceRadius)));
        emergencyAvoidSequence.AddChild(new ActionNode(AvoidGhostsEmergency));
        root.AddChild(emergencyAvoidSequence);

        // Regular Avoidance
        Sequence regularAvoidSequence = new Sequence();
        regularAvoidSequence.AddChild(new ConditionNode(() => IsGhostWithinRadius(regularGhostAvoidanceRadius)));
        regularAvoidSequence.AddChild(new ActionNode(AvoidGhostsRegular));
        root.AddChild(regularAvoidSequence);

        // Chase Ghost (hanya jika powered up)
        // Menambahkan perilaku baru agar PacMan mengejar ghost jika isPoweredUp = true dan ghost terdeteksi
        Sequence chaseGhostSequence = new Sequence();
        chaseGhostSequence.AddChild(new ConditionNode(() => isPoweredUp));
        chaseGhostSequence.AddChild(new ConditionNode(() => CanSeeGhost()));
        chaseGhostSequence.AddChild(new ActionNode(ChaseGhost));
        root.AddChild(chaseGhostSequence);

        // Chase Pellet
        Sequence chasePelletSequence = new Sequence();
        chasePelletSequence.AddChild(new ActionNode(ChasePellet));
        root.AddChild(chasePelletSequence);

        // Wander
        root.AddChild(new ActionNode(Wander));

        behaviorTree = new BehaviorTree(root);
    }

    bool IsGhostWithinRadius(float radius)
    {
        Collider[] ghosts = Physics.OverlapSphere(transform.position, radius, ghostLayer);
        return ghosts.Length > 0;
    }

    bool CanSeeGhost()
    {
        Collider[] ghosts = Physics.OverlapSphere(transform.position, ghostDetectionRadius, ghostLayer);
        return ghosts.Length > 0;
    }

    Node.NodeState ChaseGhost()
    {
        Collider[] ghosts = Physics.OverlapSphere(transform.position, ghostDetectionRadius, ghostLayer);
        if (ghosts.Length == 0) return Node.NodeState.Failure;

        // Pilih ghost terdekat
        Transform closestGhost = null;
        float closestDist = Mathf.Infinity;
        foreach (Collider g in ghosts)
        {
            float dist = Vector3.Distance(transform.position, g.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestGhost = g.transform;
            }
        }

        if (closestGhost != null)
        {
            if (NavMesh.SamplePosition(closestGhost.position, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                if (IsWithinMapBounds(hit.position))
                {
                    SetDestination(hit.position);
                    return Node.NodeState.Running;
                }
            }
        }

        return Node.NodeState.Failure;
    }

    Node.NodeState AvoidGhostsEmergency()
    {
        Collider[] ghosts = Physics.OverlapSphere(transform.position, emergencyGhostAvoidanceRadius, ghostLayer);
        if (ghosts.Length > 0)
        {
            Vector3 avoidanceDirection = Vector3.zero;

            foreach (Collider ghost in ghosts)
            {
                Vector3 directionAway = transform.position - ghost.transform.position;
                avoidanceDirection += directionAway.normalized;
            }

            if (avoidanceDirection == Vector3.zero)
            {
                avoidanceDirection = Random.insideUnitSphere;
            }

            avoidanceDirection = avoidanceDirection.normalized;
            float baseRadius = emergencyGhostAvoidanceRadius * 2f;
            float[] radiusScales = { 1f, 1.5f, 2f };

            foreach (float scale in radiusScales)
            {
                float tryRadius = baseRadius * scale;
                if (TryFindSafePosition(avoidanceDirection, tryRadius, out Vector3 safePos))
                {
                    SetDestination(safePos);
                    Debug.Log($"Emergency Avoiding ghosts, moving to {safePos} with scale {scale}");
                    return Node.NodeState.Success;
                }

                if (TryAlternateDirections(tryRadius, out Vector3 alternatePos, angleIncrement: 30f, maxAttempts: 24))
                {
                    SetDestination(alternatePos);
                    Debug.Log($"Emergency Avoiding ghosts, alternate pos {alternatePos} with scale {scale}");
                    return Node.NodeState.Success;
                }
            }
        }

        return Node.NodeState.Failure;
    }

    Node.NodeState AvoidGhostsRegular()
    {
        Collider[] ghosts = Physics.OverlapSphere(transform.position, regularGhostAvoidanceRadius, ghostLayer);
        if (ghosts.Length > 0)
        {
            Vector3 avoidanceDirection = Vector3.zero;
            foreach (Collider ghost in ghosts)
            {
                Vector3 directionAway = transform.position - ghost.transform.position;
                avoidanceDirection += directionAway.normalized;
            }

            if (avoidanceDirection == Vector3.zero)
            {
                avoidanceDirection = Random.insideUnitSphere;
            }

            avoidanceDirection = avoidanceDirection.normalized;

            float baseRadius = regularGhostAvoidanceRadius * 2f;
            float[] radiusScales = { 1f, 1.5f, 2f };

            foreach (float scale in radiusScales)
            {
                float tryRadius = baseRadius * scale;
                if (TryFindSafePosition(avoidanceDirection, tryRadius, out Vector3 safePos))
                {
                    SetDestination(safePos);
                    Debug.Log($"Regular Avoiding ghosts, moving to {safePos} with scale {scale}");
                    return Node.NodeState.Success;
                }

                if (TryAlternateDirections(tryRadius, out Vector3 alternatePos, angleIncrement: 30f, maxAttempts: 24))
                {
                    SetDestination(alternatePos);
                    Debug.Log($"Regular Avoiding ghosts, alternate pos {alternatePos} with scale {scale}");
                    return Node.NodeState.Success;
                }
            }
        }

        return Node.NodeState.Failure;
    }

    Node.NodeState ChasePellet()
    {
        Collider[] pellets = Physics.OverlapSphere(transform.position, wanderRadius, pelletLayer);
        if (pellets.Length > 0)
        {
            Transform bestPellet = null;
            float bestUtility = Mathf.NegativeInfinity;

            foreach (Collider pellet in pellets)
            {
                float distance = Vector3.Distance(transform.position, pellet.transform.position);
                float ghostPenalty = CalculateGhostPenalty(pellet.transform.position);
                float utility = (distanceWeight / distance) - (ghostProximityWeight * ghostPenalty);

                if (utility > bestUtility)
                {
                    bestUtility = utility;
                    bestPellet = pellet.transform;
                }
            }

            if (bestPellet != null)
            {
                if (NavMesh.SamplePosition(bestPellet.position, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
                {
                    if (IsWithinMapBounds(hit.position))
                    {
                        SetDestination(hit.position);
                        Debug.Log($"Chasing pellet at {hit.position}");
                        StartCoroutine(DestroyPelletAt(hit.position));
                        return Node.NodeState.Success;
                    }
                }
            }
        }

        return Node.NodeState.Failure;
    }

    IEnumerator DestroyPelletAt(Vector3 position)
    {
        yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= stoppingDistance);

        Collider[] hitPellets = Physics.OverlapSphere(position, stoppingDistance, pelletLayer);
        foreach (var pellet in hitPellets)
        {
            Destroy(pellet.gameObject);
            Debug.Log($"Pellet at {position} eaten.");
        }

        behaviorTree.Tick();
    }

    Node.NodeState Wander()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            if (IsWithinMapBounds(hit.position))
            {
                SetDestination(hit.position);
                Debug.Log($"Wandering to {hit.position}");
                return Node.NodeState.Success;
            }
        }
        else
        {
            Debug.LogWarning("Failed to find a valid wander position.");
        }

        return Node.NodeState.Failure;
    }

    float CalculateGhostPenalty(Vector3 targetPosition)
    {
        Collider[] ghosts = Physics.OverlapSphere(targetPosition, regularGhostAvoidanceRadius, ghostLayer);
        if (ghosts.Length == 0)
            return 0f;

        float penalty = 0f;
        foreach (Collider ghost in ghosts)
        {
            Vector3 predictedGhostPos = PredictGhostPosition(ghost.transform);
            float distanceToGhost = Vector3.Distance(targetPosition, predictedGhostPos);
            if (distanceToGhost < regularGhostAvoidanceRadius)
            {
                penalty += (regularGhostAvoidanceRadius - distanceToGhost) / regularGhostAvoidanceRadius;
            }
        }

        return penalty;
    }

    Vector3 PredictGhostPosition(Transform ghost)
    {
        NavMeshAgent ghostAgent = ghost.GetComponent<NavMeshAgent>();
        if (ghostAgent != null)
        {
            return ghost.position + ghostAgent.velocity * predictionTime;
        }
        return ghost.position;
    }

    void SetDestination(Vector3 target)
    {
        NavMeshPath path = new NavMeshPath();
        if (agent.CalculatePath(target, path) && path.status == NavMeshPathStatus.PathComplete)
        {
            if (agent.SetDestination(target))
            {
                hasTarget = true;
            }
        }
        else
        {
            hasTarget = false;
        }
    }

    public void LoseLife()
    {
        lives--;

        if (lives > 0)
        {
            Debug.Log($"PacMan lost a life! Lives remaining: {lives}");
            Debug.Log($"Respawning at {spawnPoint.position}");
            transform.position = spawnPoint.position;
            Debug.Log($"Current position: {transform.position}");
            if (agent != null)
            {
                agent.ResetPath();
                agent.Warp(spawnPoint.position); // Move agent directly to spawn point
            }
            hasTarget = false;
            isInEmergency = false;
            behaviorTree.Tick();
        }
        else
        {
            Debug.Log("PacMan has no lives left. Game Over!");
            SceneManager.LoadScene("GameOver");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Jika PacMan bersentuhan dengan Ghost (tag Ghost adalah "Player")
        if (other.CompareTag("Player"))
        {
            if (isPoweredUp)
            {
                // PacMan dalam mode powered-up, makan ghost (hancurkan ghost)
                Debug.Log("PacMan ate a ghost!");
                Destroy(other.gameObject);
            }
            else
            {
                // Jika tidak powered-up, PacMan kehilangan nyawa
                LoseLife();
            }
        }
    }

    bool IsWithinMapBounds(Vector3 position)
    {
        float mapMinX = 0f;
        float mapMaxX = 20f;
        float mapMinZ = 0f;
        float mapMaxZ = 20f;

        return position.x >= mapMinX && position.x <= mapMaxX && position.z >= mapMinZ && position.z <= mapMaxZ;
    }

    bool TryFindSafePosition(Vector3 baseDirection, float radius, out Vector3 safePos)
    {
        Vector3 avoidanceTarget = transform.position + baseDirection * radius;
        if (NavMesh.SamplePosition(avoidanceTarget, out NavMeshHit hit, radius, NavMesh.AllAreas))
        {
            if (IsWithinMapBounds(hit.position) && IsSafeFromGhosts(hit.position))
            {
                NavMeshPath path = new NavMeshPath();
                if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    safePos = hit.position;
                    return true;
                }
            }
        }

        safePos = Vector3.zero;
        return false;
    }

    bool TryAlternateDirections(float distance, out Vector3 alternatePos, float angleIncrement = 30f, int maxAttempts = 12)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            float angle = i * angleIncrement;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            Vector3 alternateTarget = transform.position + direction * distance;

            if (NavMesh.SamplePosition(alternateTarget, out NavMeshHit hit, distance, NavMesh.AllAreas))
            {
                if (IsWithinMapBounds(hit.position) && IsSafeFromGhosts(hit.position))
                {
                    NavMeshPath path = new NavMeshPath();
                    if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                    {
                        alternatePos = hit.position;
                        return true;
                    }
                }
            }
        }

        alternatePos = Vector3.zero;
        return false;
    }

    bool IsSafeFromGhosts(Vector3 pos)
    {
        Collider[] ghosts = Physics.OverlapSphere(pos, regularGhostAvoidanceRadius, ghostLayer);
        return ghosts.Length == 0;
    }

    public void ActivatePowerMode(float duration)
    {
        if (powerRoutine != null)
        {
            StopCoroutine(powerRoutine);
        }
        powerRoutine = StartCoroutine(PowerModeCoroutine(duration));
    }

    IEnumerator PowerModeCoroutine(float duration)
    {
        isPoweredUp = true;
        Debug.Log("PacMan is now powered up and can eat ghosts!");
        yield return new WaitForSeconds(duration);
        isPoweredUp = false;
        Debug.Log("Power mode ended. PacMan no longer can eat ghosts.");
    }
}
