using UnityEngine;
using UnityEngine.AI;

public class Enemy_Movement : MonoBehaviour
{
    public NavMeshAgent agent;
    public bool isChasing;

    [SerializeField] private float navMeshSampleRadius = 2f;
    [SerializeField] private int destinationAttempts = 20;
    [SerializeField] private float minimumWait = 1f;
    [SerializeField] private float maximumWait = 3f;

    private Collider patrolRoom;
    private bool hasDestination;
    private float waitUntil;

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
    }

    public void Initialize(Collider roomArea)
    {
        patrolRoom = roomArea;
        hasDestination = false;
        waitUntil = 0f;

        Debug.Log(
            $"{name}: patrol initialized. Room={(patrolRoom != null ? patrolRoom.name : "NULL")}, " +
            $"Agent on NavMesh={(agent != null && agent.isOnNavMesh)}",
            this);
    }

    private void Update()
    {
        if (isChasing || patrolRoom == null || agent == null ||
            !agent.enabled || !agent.isOnNavMesh || agent.pathPending)
            return;

        if (hasDestination &&
            (!agent.hasPath ||
             agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, 0.15f)))
        {
            hasDestination = false;
            waitUntil = Time.time + Random.Range(minimumWait, maximumWait);
        }

        if (!hasDestination && Time.time >= waitUntil)
            PickDestination();
    }

    private void PickDestination()
    {
        Debug.Log($"{name}: trying to pick a patrol destination.", this);

        Bounds bounds = patrolRoom.bounds;

        for (int i = 0; i < destinationAttempts; i++)
        {
            Vector3 candidate = new Vector3(
               Random.Range(bounds.min.x, bounds.max.x),
                agent.transform.position.y,
                Random.Range(bounds.min.z, bounds.max.z));

            if (!NavMesh.SamplePosition(
                    candidate,
                    out NavMeshHit hit,
                    navMeshSampleRadius,
                    NavMesh.AllAreas))
            {
                continue;
            }

            if (hit.position.x < bounds.min.x || hit.position.x > bounds.max.x ||
                hit.position.z < bounds.min.z || hit.position.z > bounds.max.z)
            {
                continue;
            }

            if (agent.SetDestination(hit.position))
            {
                hasDestination = true;
                Debug.Log(
                    $"{name}: destination set to {hit.position}; speed={agent.speed}; stopped={agent.isStopped}",
                    this);
                return;
            }
        }

        waitUntil = Time.time + 1f;
        Debug.LogWarning($"{name}: failed to pick a patrol destination in {patrolRoom.name}.", this);
    }
}