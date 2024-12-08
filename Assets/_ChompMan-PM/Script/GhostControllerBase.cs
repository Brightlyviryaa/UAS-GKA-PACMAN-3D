using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BT_PacMan;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class GhostControllerBase : MonoBehaviour
{
    public float wanderRadius = 15f;
    public float detectionRadius = 1f;
    public float ghostSpeed = 0.1f;
    public bool isPlayerControlled = false;

    private NavMeshAgent agent;
    private BehaviorTree behaviorTree;
    private PacManController pacMan;
    private Transform pacManTransform;

    // Simpan posisi current wander target
    private Vector3 currentWanderTarget;

    void Start()
    {
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        capsuleCollider.isTrigger = false;

        agent = GetComponent<NavMeshAgent>();
        agent.speed = ghostSpeed;

        // Cari PacMan
        GameObject pacManObj = GameObject.FindGameObjectWithTag("PacMan");
        if (pacManObj != null)
        {
            pacMan = pacManObj.GetComponent<PacManController>();
            pacManTransform = pacManObj.transform;
        }

        if (!isPlayerControlled)
        {
            InitializeBehaviorTree();
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
            behaviorTree.Tick();
        }
    }

    protected virtual void HandlePlayerControl()
    {
        float moveSpeed = 5f;
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
            Debug.Log($"Ghost {gameObject.name} collided with PacMan.");

            PacManController pacManScript = other.GetComponent<PacManController>();
            if (pacManScript != null)
            {
                pacManScript.LoseLife();
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

        Sequence chasePacManSequence = new Sequence();
        chasePacManSequence.AddChild(new ConditionNode(CanSeePacMan));
        chasePacManSequence.AddChild(new ActionNode(ChasePacMan));
        root.AddChild(chasePacManSequence);

        // ActionNode wander
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
            }
        }

        // Selalu kembalikan Running agar ghost terus wander
        return Node.NodeState.Running;
    }
}
