using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using BT_PacMan;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class GhostControllerBase : MonoBehaviour
{
    [Header("Movement Settings")]
    public float wanderRadius = 15f;
    public float detectionRadius = 1f;
    public float ghostSpeed = 1.5f; // Ubah menjadi 1.5f agar ghost lebih lambat
    public bool isPlayerControlled = false;

    [Header("Respawn Settings")]
    public Transform ghostSpawnPoint; // Assign melalui Inspector
    public float respawnCooldown = 3f;

    protected NavMeshAgent agent;
    private BehaviorTree behaviorTree;
    private PacManController pacMan;
    private Transform pacManTransform;

    // Simpan posisi current wander target
    private Vector3 currentWanderTarget;

    public GameObject[] allGhosts;

    public bool isInvisible = false;

    private Material originalMaterial;
    public Material scaredMaterial;
    private Renderer ghostRenderer;

    protected virtual void Start()
    {
        // Pastikan CapsuleCollider bukan trigger
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        capsuleCollider.isTrigger = false;

        // Inisialisasi NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        agent.speed = ghostSpeed; // Terapkan kecepatan ghost AI maupun player-controlled

        // Cari PacMan di scene
        GameObject pacManObj = GameObject.FindGameObjectWithTag("PacMan");
        if (pacManObj != null)
        {
            pacMan = pacManObj.GetComponent<PacManController>();
            pacManTransform = pacManObj.transform;
        }
        else
        {
            Debug.LogError("PacMan dengan tag 'PacMan' tidak ditemukan di scene!");
        }

        // Inisialisasi Behavior Tree hanya jika bukan player controlled
        if (!isPlayerControlled)
        {
            InitializeBehaviorTree();
        }

        // Handle collision with other ghosts
        IgnoreGhostCollisions();

        ghostRenderer = GetComponent<Renderer>();
        if (ghostRenderer != null)
        {
            originalMaterial = ghostRenderer.material;
        }
        else
        {
            Debug.LogError("Renderer not found on the ghost object!");
        }        
    }

    protected virtual void Update()
    {
        if (pacMan.isPoweredUp)
        {
            SetGhostMaterial(scaredMaterial); // Change to scared material
        }
        else
        {
            SetGhostMaterial(originalMaterial);
        }

        if (isPlayerControlled)
        {
            HandlePlayerControl();
        }
        else
        {
            behaviorTree.Tick();
        }
    }

    public void SetGhostMaterial(Material newMaterial)
    {
        // Set the material of the ghost
        if (ghostRenderer != null)
        {
            ghostRenderer.material = newMaterial;
        }
        else
        {
            Debug.LogError("Renderer not found on the ghost object!");
        }
    }

    void IgnoreGhostCollisions()
    {
        allGhosts = GameObject.FindGameObjectsWithTag("PlayerGhost");
        CapsuleCollider thisCollider = GetComponent<CapsuleCollider>();

        foreach (var ghost in allGhosts)
        {
            if (ghost != this)
            {
                CapsuleCollider otherCollider = ghost.GetComponent<CapsuleCollider>();
                if (otherCollider != null)
                {
                    Physics.IgnoreCollision(thisCollider, otherCollider);
                }
            }
        }
    }

    protected virtual void HandlePlayerControl()
    {
        // Gunakan ghostSpeed agar kecepatan player controlled ghost sama dengan AI (1.5f)
        float moveSpeed = ghostSpeed;
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
            behaviorTree.Tick(); // Mulai Behavior Tree kembali
        }

        CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.target = isPlayerControlled ? transform : null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PacMan"))
        {
            if (pacMan != null)
            {
                if (pacMan.isPoweredUp)
                {
                    // Ghost dimakan oleh PacMan
                    Debug.Log($"Ghost {gameObject.name} eaten by PacMan!");
                    StartCoroutine(EatAndRespawn());
                }
                else
                {
                    // PacMan kehilangan nyawa
                    Debug.Log($"Ghost {gameObject.name} collided with PacMan. PacMan loses a life.");
                    pacMan.LoseLife();
                }
            }
            else
            {
                Debug.LogError("PacManController script is missing on PacMan!");
            }
        }
    }

    void InitializeBehaviorTree()
    {
        Selector root = new Selector();

        // Sequence untuk fleeing saat PacMan powered-up
        Sequence fleePacManSequence = new Sequence();
        fleePacManSequence.AddChild(new ConditionNode(IsPacManPoweredUp));
        fleePacManSequence.AddChild(new ConditionNode(CanSeePacMan));
        fleePacManSequence.AddChild(new ActionNode(FleePacMan));
        root.AddChild(fleePacManSequence);

        // Sequence untuk mengejar PacMan normal (hanya jika tidak powered-up)
        Sequence chasePacManSequence = new Sequence();
        chasePacManSequence.AddChild(new ConditionNode(CanSeePacMan));
        chasePacManSequence.AddChild(new ActionNode(ChasePacMan));
        root.AddChild(chasePacManSequence);

        // ActionNode untuk wander
        root.AddChild(new ActionNode(WanderAction));

        behaviorTree = new BehaviorTree(root);
    }

    bool CanSeePacMan()
    {
        if (pacManTransform == null)
            return false;

        float distance = Vector3.Distance(transform.position, pacManTransform.position);
        return distance <= detectionRadius;
    }

    bool IsPacManPoweredUp()
    {
        if (pacMan != null)
        {
            return pacMan.isPoweredUp;
        }
        return false;
    }

    Node.NodeState ChasePacMan()
    {
        if (pacManTransform == null)
            return Node.NodeState.Failure;

        if (agent.SetDestination(pacManTransform.position))
        {
            // Selama PacMan terlihat, kembalikan Running
            return Node.NodeState.Running;
        }
        return Node.NodeState.Failure;
    }

    Node.NodeState FleePacMan()
    {
        if (pacManTransform == null)
            return Node.NodeState.Failure;

        Vector3 directionAway = transform.position - pacManTransform.position;
        if (directionAway == Vector3.zero)
        {
            directionAway = Random.insideUnitSphere;
        }
        directionAway.Normalize();

        Vector3 fleeTarget = transform.position + directionAway * wanderRadius;

        // Validasi fleeTarget agar tetap dalam NavMesh
        if (NavMesh.SamplePosition(fleeTarget, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            if (IsWithinMapBounds(hit.position))
            {
                agent.SetDestination(hit.position);
                //Debug.Log($"Ghost {gameObject.name} fleeing to {hit.position}");
                return Node.NodeState.Success;
            }
        }

        // Jika tidak valid, coba arah lain
        if (TryAlternateDirections(wanderRadius, out Vector3 alternatePos, angleIncrement: 30f, maxAttempts: 12))
        {
            agent.SetDestination(alternatePos);
            //Debug.Log($"Ghost {gameObject.name} fleeing to alternate {alternatePos}");
            return Node.NodeState.Success;
        }

        return Node.NodeState.Failure;
    }

    Node.NodeState WanderAction()
    {
        // Cek apakah kita sudah sampai ke tujuan wander saat ini atau belum punya tujuan
        if (!agent.pathPending && agent.remainingDistance <= 0.5f)
        {
            // Pilih tujuan wander baru
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
            {
                currentWanderTarget = hit.position;
                agent.SetDestination(currentWanderTarget);
                //Debug.Log($"Ghost {gameObject.name} wandering to {hit.position}");
            }
        }

        // Selalu kembalikan Running agar ghost terus wander
        return Node.NodeState.Running;
    }

    bool IsWithinMapBounds(Vector3 position)
    {
        // Ganti dengan logika yang sesuai untuk memeriksa batas peta Anda
        float mapMinX = -20f;
        float mapMaxX = 20f;
        float mapMinZ = -20f;
        float mapMaxZ = 20f;

        return position.x >= mapMinX && position.x <= mapMaxX && position.z >= mapMinZ && position.z <= mapMaxZ;
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
                if (IsWithinMapBounds(hit.position))
                {
                    alternatePos = hit.position;
                    return true;
                }
            }
        }

        alternatePos = Vector3.zero;
        return false;
    }

    IEnumerator EatAndRespawn()
    {
        // Hentikan agent
        agent.isStopped = true;

        // Hancurkan ghost
        gameObject.SetActive(false);
        Debug.Log($"Ghost {gameObject.name} has been eaten and will respawn in {respawnCooldown} seconds.");

        // Tunggu cooldown
        yield return new WaitForSeconds(respawnCooldown);

        // Respawn di GhostSpawnPoint
        transform.position = ghostSpawnPoint.position;
        gameObject.SetActive(true);
        agent.isStopped = false;
        Debug.Log($"Ghost {gameObject.name} has respawned at {ghostSpawnPoint.position}.");

        // Restart Behavior Tree jika bukan player controlled
        if (!isPlayerControlled)
        {
            behaviorTree.Tick();
        }
    }
}
