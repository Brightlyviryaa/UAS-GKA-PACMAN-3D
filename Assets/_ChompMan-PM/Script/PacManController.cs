using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using BT_PacMan;

[RequireComponent(typeof(NavMeshAgent))]
public class PacManController : MonoBehaviour
{
    // Enum untuk berbagai state dalam Behavior Tree
    public enum State { Aggressive, Defensive, Safe }

    [Header("Settings")]
    public float wanderRadius = 10f;
    public float regularGhostAvoidanceRadius = 5f;
    public float emergencyGhostAvoidanceRadius = 2f; // Radius untuk kondisi darurat
    public float predictionTime = 2f; // Waktu memprediksi posisi ghost
    public int lives = 3;
    public Transform spawnPoint;
    public LayerMask pelletLayer;
    public LayerMask ghostLayer;
    public float stoppingDistance = 0.5f;

    [Header("Utility Weights")]
    public float distanceWeight = 1f;
    public float ghostProximityWeight = 2f;

    private NavMeshAgent agent;
    private BehaviorTree behaviorTree;
    private bool hasTarget = false; // Menandakan apakah PacMan sedang punya tujuan
    private bool isInEmergency = false; // Menandakan apakah sedang dalam kondisi darurat

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        InitializeBehaviorTree();
        behaviorTree.Tick(); // Panggil Tick pertama kali untuk mencari target awal
    }

    void Update()
    {
        // Cek apakah ada ghost dalam Emergency Ghost Radius
        bool emergencyGhostPresent = Physics.OverlapSphere(transform.position, emergencyGhostAvoidanceRadius, ghostLayer).Length > 0;

        if (emergencyGhostPresent && !isInEmergency)
        {
            Debug.Log("Emergency ghost detected. Re-evaluating target.");
            isInEmergency = true;
            behaviorTree.Tick(); // Re-evaluate target untuk menghindar secara darurat
        }
        else if (!emergencyGhostPresent && isInEmergency)
        {
            // Jika sudah tidak dalam keadaan darurat, reset flag
            isInEmergency = false;
        }

        // Cek apakah sudah mencapai target
        if (hasTarget && !agent.pathPending && agent.remainingDistance <= stoppingDistance && agent.destination != Vector3.zero)
        {
            Debug.Log("Reached target.");
            agent.ResetPath();
            hasTarget = false;
            // Cari target baru setelah mencapai tujuan
            behaviorTree.Tick();
        }

        // Jika PacMan tidak punya target (misalnya setelah gagal menemukan posisi), cari tujuan baru
        if (!hasTarget && (agent.destination == Vector3.zero || agent.remainingDistance <= stoppingDistance))
        {
            behaviorTree.Tick();
        }
    }

    // Inisialisasi Behavior Tree
    void InitializeBehaviorTree()
    {
        Selector root = new Selector();

        // Emergency Avoidance memiliki prioritas tertinggi
        Sequence emergencyAvoidSequence = new Sequence();
        emergencyAvoidSequence.AddChild(new ConditionNode(() => IsGhostWithinRadius(emergencyGhostAvoidanceRadius)));
        emergencyAvoidSequence.AddChild(new ActionNode(AvoidGhostsEmergency));
        root.AddChild(emergencyAvoidSequence);

        // Regular Avoidance
        Sequence regularAvoidSequence = new Sequence();
        regularAvoidSequence.AddChild(new ConditionNode(() => IsGhostWithinRadius(regularGhostAvoidanceRadius)));
        regularAvoidSequence.AddChild(new ActionNode(AvoidGhostsRegular));
        root.AddChild(regularAvoidSequence);

        // Chase Pellet
        Sequence chasePelletSequence = new Sequence();
        chasePelletSequence.AddChild(new ActionNode(ChasePellet));
        root.AddChild(chasePelletSequence);

        // Wander
        root.AddChild(new ActionNode(Wander));

        behaviorTree = new BehaviorTree(root);
    }

    // Kondisi untuk mengecek apakah ada ghost dalam radius tertentu
    bool IsGhostWithinRadius(float radius)
    {
        Collider[] ghosts = Physics.OverlapSphere(transform.position, radius, ghostLayer);
        return ghosts.Length > 0;
    }

    // Fungsi untuk menghindari ghost dalam kondisi darurat
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

            // Mencoba beberapa skala radius untuk menemukan tempat aman
            float baseRadius = emergencyGhostAvoidanceRadius * 2f;
            float[] radiusScales = { 1f, 1.5f, 2f };

            foreach (float scale in radiusScales)
            {
                float tryRadius = baseRadius * scale;
                // Coba langsung pada arah avoidanceDirection
                if (TryFindSafePosition(avoidanceDirection, tryRadius, out Vector3 safePos))
                {
                    SetDestination(safePos);
                    Debug.Log($"Emergency Avoiding ghosts, moving to {safePos} with scale {scale}");
                    return Node.NodeState.Success;
                }

                // Jika gagal, coba alternate direction
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

    // Fungsi untuk menghindari ghost secara reguler
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

    // Fungsi untuk mengejar pellet dengan utility-based decision making
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

    // Coroutine untuk menghancurkan pellet setelah dicapai
    IEnumerator DestroyPelletAt(Vector3 position)
    {
        // Tunggu sampai PacMan sampai ke pellet
        yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= stoppingDistance);

        Collider[] hitPellets = Physics.OverlapSphere(position, stoppingDistance, pelletLayer);
        foreach (var pellet in hitPellets)
        {
            Destroy(pellet.gameObject);
            Debug.Log($"Pellet at {position} eaten.");
        }

        // Setelah makan pellet, cari target baru
        behaviorTree.Tick();
    }

    // Fungsi untuk wandering secara acak
    Node.NodeState Wander()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            // Pastikan tujuan berada dalam batas peta
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

    // Fungsi untuk menghitung penalti berdasarkan jarak dari ghost
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

    // Fungsi untuk memprediksi posisi ghost di masa depan
    Vector3 PredictGhostPosition(Transform ghost)
    {
        NavMeshAgent ghostAgent = ghost.GetComponent<NavMeshAgent>();
        if (ghostAgent != null)
        {
            return ghost.position + ghostAgent.velocity * predictionTime;
        }
        return ghost.position;
    }

    // Fungsi untuk mengatur tujuan NavMeshAgent
    void SetDestination(Vector3 target)
    {
        // Pastikan path bisa dicapai
        NavMeshPath path = new NavMeshPath();
        if (agent.CalculatePath(target, path) && path.status == NavMeshPathStatus.PathComplete)
        {
            if (agent.SetDestination(target))
            {
                hasTarget = true; // Setelah menetapkan tujuan, tandai bahwa kita punya target
            }
        }
        else
        {
            hasTarget = false;
        }
    }

    // Fungsi untuk mengurangi hidup PacMan
    public void LoseLife()
    {
        lives--;

        if (lives > 0)
        {
            Debug.Log($"PacMan lost a life! Lives remaining: {lives}");
            transform.position = spawnPoint.position; // Respawn
            agent.ResetPath();
            hasTarget = false;
            isInEmergency = false;
            behaviorTree.Tick(); // Cari target baru setelah respawn
        }
        else
        {
            Debug.Log("PacMan has no lives left. Game Over!");
            SceneManager.LoadScene("GameOver"); // Ganti "GameOver" sesuai scene
        }
    }

    // Menggambar Gizmos untuk radius tertentu di editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, regularGhostAvoidanceRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, emergencyGhostAvoidanceRadius);
    }

    // ---------------------------
    // Helper Methods
    // ---------------------------

    // Fungsi untuk memastikan posisi berada dalam batas peta
    bool IsWithinMapBounds(Vector3 position)
    {
        // Ganti dengan logika yang sesuai untuk memeriksa batas peta Anda
        // Misalnya, jika peta adalah kotak dari (-50, 0, -50) ke (50, 0, 50):
        float mapMinX = -50f;
        float mapMaxX = 50f;
        float mapMinZ = -50f;
        float mapMaxZ = 50f;

        return position.x >= mapMinX && position.x <= mapMaxX && position.z >= mapMinZ && position.z <= mapMaxZ;
    }

    // Fungsi untuk mendapatkan arah alternatif jika arah utama tidak valid
    Vector3 GetAlternateDirection(float distance)
    {
        // Coba beberapa arah alternatif (misalnya, 45 derajat offset)
        float angleIncrement = 45f;
        for (int i = 1; i <= 360 / (int)angleIncrement; i++)
        {
            float angle = i * angleIncrement;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            Vector3 alternateTarget = transform.position + direction * distance;

            // Validasi apakah alternateTarget berada dalam NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(alternateTarget, out hit, distance, NavMesh.AllAreas))
            {
                if (IsWithinMapBounds(hit.position))
                {
                    return hit.position;
                }
            }
        }

        // Jika tidak ada arah alternatif yang valid, kembali ke posisi saat ini
        return transform.position;
    }

    // Fungsi untuk mencoba menemukan posisi aman langsung pada arah avoidanceDirection
    bool TryFindSafePosition(Vector3 baseDirection, float radius, out Vector3 safePos)
    {
        Vector3 avoidanceTarget = transform.position + baseDirection * radius;
        if (NavMesh.SamplePosition(avoidanceTarget, out NavMeshHit hit, radius, NavMesh.AllAreas))
        {
            if (IsWithinMapBounds(hit.position) && IsSafeFromGhosts(hit.position))
            {
                // Cek jalur bisa dilalui
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

    // Fungsi ini mencoba banyak sudut dengan angleIncrement yang lebih kecil dan maxAttempts yang lebih banyak
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
                    // Check if path is reachable
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

    // Fungsi tambahan untuk mengecek apakah posisi cukup jauh dari ghost
    bool IsSafeFromGhosts(Vector3 pos)
    {
        Collider[] ghosts = Physics.OverlapSphere(pos, regularGhostAvoidanceRadius, ghostLayer);
        return ghosts.Length == 0;
    }
}
