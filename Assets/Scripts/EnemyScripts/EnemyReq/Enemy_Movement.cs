using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy_Movement : MonoBehaviour
{
    public NavMeshAgent agent;
    public bool isChasing { get; private set; }

    [Header("Patrol")]
    [SerializeField] private float navMeshSampleRadius = 2f;
    [SerializeField] private int destinationAttempts = 20;
    [SerializeField] private float minimumWait = 1.5f;
    [SerializeField] private float maximumWait = 4f;

    [Header("Chase")]
    [SerializeField] private float lostPlayerSearchTime = 1.5f;
    [SerializeField] private float destinationRefreshRate = 0.6f;

    private Collider[] patrolAreas;
    private Transform playerTarget;
    private bool playerVisible;
    private bool isAttacking;
    private bool hasDestination;
    private Vector3 lastKnownPlayerPosition;
    private float waitUntil;
    private float searchUntil;
    private float nextDestinationUpdate;

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
    }

    public void Initialize(Collider[] areas, Transform target)
    {
        patrolAreas = areas;

        playerTarget = target;

        playerVisible = false;

        isChasing = false;

        isAttacking = false;

        hasDestination = false;

        waitUntil = 0f;

        searchUntil = 0f;

        Debug.Log( $"{name}: initialized with {patrolAreas?.Length ?? 0} patrol areas. " +
            $"On NavMesh={agent.isOnNavMesh}", this);
    }

    public void SetPlayerVisible(bool visible, Transform target)
    {
        if (target != null) { playerTarget = target; }

        if (visible && playerTarget != null)
        {
            bool justSpottedPlayer = !playerVisible;

            playerVisible = true;
            isChasing = true;
            lastKnownPlayerPosition = playerTarget.position;

            if (justSpottedPlayer) { nextDestinationUpdate = 0f; }
        }
        else if (playerVisible)
        {
            playerVisible = false;
            searchUntil = Time.time + lostPlayerSearchTime;
            nextDestinationUpdate = 0f;
        }
    }

    public void SetAttacking(bool attacking)
    {
        isAttacking = attacking;
        if (agent != null && agent.enabled && agent.isOnNavMesh)
            { agent.isStopped = attacking; }
    }

    private void Update()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) { return; }
        if (isAttacking) { agent.isStopped = true; return; }
        if (isChasing) { UpdateChase(); if (isChasing) { return; } }
        UpdatePatrol();
    }

    private void UpdateChase()
    {
        agent.isStopped = false;

        Vector3 destination = playerVisible && playerTarget != null
                ? playerTarget.position : lastKnownPlayerPosition;

        if (Time.time >= nextDestinationUpdate)
        {
            agent.SetDestination(destination);
            nextDestinationUpdate = Time.time + destinationRefreshRate;
        }

        if (!playerVisible &&
            (Time.time >= searchUntil || ReachedDestination()))
        {
            isChasing = false;
            hasDestination = false;
            waitUntil = 0f;
        }
    }

    private void UpdatePatrol()
    {
        if (patrolAreas == null || patrolAreas.Length == 0) { return; }

        if (hasDestination && ReachedDestination())
        {
            hasDestination = false;
            waitUntil = Time.time + Random.Range(minimumWait, maximumWait);
        }

        if (!hasDestination && Time.time >= waitUntil) { PickDestination(); }
    }

    private bool ReachedDestination()
    {
        if (agent.pathPending) { return false; }

        float remainingDistance = agent.remainingDistance;

        // Infinity means Unity has not produced a path yet.
        return !float.IsInfinity(remainingDistance) && !float.IsNaN(remainingDistance)
            && remainingDistance <= Mathf.Max(agent.stoppingDistance, 0.15f);
    }

    private void PickDestination()
    {
        if (patrolAreas == null || patrolAreas.Length == 0)
        {
            Debug.LogWarning($"{name}: no patrol areas were assigned.", this);
            return;
        }

        Debug.Log($"{name}: searching for a patrol destination.", this);

        // Shuffle the areas so the search doesn't keep picking the same ones.
        Collider[] areasToTry = (Collider[])patrolAreas.Clone();

        for (int i = areasToTry.Length - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);

            Collider temp = areasToTry[i];
            areasToTry[i] = areasToTry[swapIndex];
            areasToTry[swapIndex] = temp;
        }

        // Try two points in every area, or destinationAttempts times
        // if that setting is higher.
        int attempts = Mathf.Max(destinationAttempts, areasToTry.Length * 2);

        for (int i = 0; i < attempts; i++)
        {
            Collider area = areasToTry[i % areasToTry.Length];

            if (area == null || !area.enabled) { continue; }

            Bounds bounds = area.bounds;

            Vector3 candidate = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                agent.transform.position.y,
                Random.Range(bounds.min.z, bounds.max.z)
            );

            if (!NavMesh.SamplePosition( candidate, out NavMeshHit hit,
                    navMeshSampleRadius, NavMesh.AllAreas))
            {
                continue;
            }

            if (!IsInsideBounds(area.bounds, hit.position))
                continue;

            NavMeshPath path = new NavMeshPath();

            if (!agent.CalculatePath(hit.position, path) ||
                path.status != NavMeshPathStatus.PathComplete)
            {
                continue;
            }

            if (Vector3.Distance(transform.position, hit.position) < 1f)
                continue;

            if (agent.SetPath(path))
            {
                hasDestination = true;
                Debug.Log($"{name}: patrol destination set to {hit.position}.", this);
                return;
            }
        }

        Debug.LogWarning(
            $"{name}: couldn't find a reachable patrol destination after {attempts} attempts.",
            this
        );

        waitUntil = Time.time + 1f;
    }

    private bool IsInsideBounds(Bounds bounds, Vector3 point)
    {
        return point.x >= bounds.min.x
            && point.x <= bounds.max.x
            && point.z >= bounds.min.z
            && point.z <= bounds.max.z;
    }
}