using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy_Movement : MonoBehaviour
{
    public NavMeshAgent agent;
    public bool isChasing { get; private set; }

    private enum State
    {
        Patrol,
        Chase,
        Search,
        SearchWait,
        Attack
    }

    [Header("Patrol")]
    [SerializeField] private float patrolRadius = 6f;
    [SerializeField] private float patrolSampleRadius = 0.75f;
    [SerializeField] private float maximumPatrolPathLength = 12f;
    [SerializeField] private int patrolAttempts = 20;
    [SerializeField] private float minimumWait = 1.5f;
    [SerializeField] private float maximumWait = 4f;

    [Header("Chase and Search")]
    [SerializeField] private float chaseStoppingDistance = 1.25f;
    [SerializeField] private float targetSampleRadius = 2f;
    [SerializeField] private float retargetDistance = 0.75f;
    [SerializeField] private float destinationRefreshRate = 0.6f;
    [SerializeField] private float searchWaitTime = 1.5f;
    [SerializeField] private float searchTravelTimeout = 10f;

    [Header("Movement")]
    [SerializeField] private float turnSpeed = 180f;
    [SerializeField] private float stuckTimeout = 2.5f;
    [SerializeField] private float progressCheckInterval = 0.5f;
    [SerializeField] private float minimumPositionProgress = 0.15f;

    private State state = State.Patrol;
    private Collider[] patrolAreas;
    private Transform playerTarget;

    private bool playerVisible;
    private bool hasDestination;
    private bool hasLastPatrolPoint;

    private Vector3 lastKnownPlayerPosition;
    private Vector3 lastChaseRequestPosition;
    private Vector3 currentDestination;
    private Vector3 lastPatrolPoint;
    private Vector3 previousPosition;

    private float defaultStoppingDistance;
    private float waitUntil;
    private float searchUntil;
    private float searchDeadline;
    private float nextDestinationUpdate;
    private float lastProgressCheck;
    private float noProgressTime;

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        defaultStoppingDistance = agent.stoppingDistance;
        agent.updatePosition = true;
        agent.angularSpeed = turnSpeed;
    }

    public void Initialize(Collider[] areas, Transform target)
    {
        patrolAreas = areas;
        playerTarget = target;

        playerVisible = false;
        isChasing = false;
        hasLastPatrolPoint = false;
        state = State.Patrol;
        waitUntil = Time.time;

        ClearPath();
    }

    // Called by Detection.
    public void SetPlayerVisible(bool visible, Transform target)
    {
        if (target != null)
            playerTarget = target;

        if (visible && playerTarget != null)
        {
            bool justSpottedPlayer = !playerVisible;

            playerVisible = true;
            isChasing = true;
            lastKnownPlayerPosition = playerTarget.position;

            if (state != State.Attack)
                state = State.Chase;

            if (justSpottedPlayer && state == State.Chase)
            {
                ClearPath();
                nextDestinationUpdate = Time.time;
            }
        }
        else if (playerVisible)
        {
            playerVisible = false;
            searchDeadline = Time.time + searchTravelTimeout;

            if (state != State.Attack)
            {
                state = State.Search;
                nextDestinationUpdate = Time.time;
                ClearPath();
            }
        }
    }

    // Call true when the attack animation starts and false when it ends.
    public void SetAttacking(bool attacking)
    {
        if (attacking)
        {
            state = State.Attack;
            PauseAgent();
            return;
        }

        if (state != State.Attack)
            return;

        if (playerVisible)
            state = State.Chase;
        else if (isChasing)
            state = State.Search;
        else
            state = State.Patrol;

        nextDestinationUpdate = Time.time;
        waitUntil = Time.time;
        ClearPath();
    }

    private void Update()
    {
        if (!AgentReady())
            return;

        switch (state)
        {
            case State.Patrol:
                UpdatePatrol();
                break;

            case State.Chase:
                UpdateChase();
                break;

            case State.Search:
                UpdateSearch();
                break;

            case State.SearchWait:
                PauseAgent();

                if (Time.time >= searchUntil)
                    FinishSearch();

                break;

            case State.Attack:
                PauseAgent();
                break;
        }

        CheckForStall();
    }

    private void UpdatePatrol()
    {
        agent.stoppingDistance = defaultStoppingDistance;

        if (hasDestination && !agent.pathPending)
        {
            if (ReachedDestination())
            {
                ClearPath();
                waitUntil = Time.time + Random.Range(
                    minimumWait,
                    Mathf.Max(minimumWait, maximumWait)
                );

                return;
            }

            if (agent.pathStatus != NavMeshPathStatus.PathComplete ||
                !agent.hasPath)
            {
                RecoverStalledRoute();
                return;
            }

            ResumeAgent();
            return;
        }

        if (hasDestination)
            return;

        if (Time.time >= waitUntil)
            PickPatrolPoint();
        else
            PauseAgent();
    }

    private void UpdateChase()
    {
        if (playerTarget == null)
        {
            state = State.Search;
            searchDeadline = Time.time + searchTravelTimeout;
            return;
        }

        agent.stoppingDistance =
            Mathf.Max(defaultStoppingDistance, chaseStoppingDistance);

        if (playerVisible)
            lastKnownPlayerPosition = playerTarget.position;

        if (hasDestination && !agent.pathPending)
        {
            if (ReachedDestination())
                PauseAgent();
            else if (agent.pathStatus != NavMeshPathStatus.PathComplete ||
                     !agent.hasPath)
            {
                RecoverStalledRoute();
                return;
            }
            else
                ResumeAgent();
        }

        bool targetMoved =
            Vector3.Distance(playerTarget.position, lastChaseRequestPosition)
            >= retargetDistance;

        if ((hasDestination && !targetMoved) ||
            Time.time < nextDestinationUpdate)
        {
            return;
        }

        if (SetPathNear(playerTarget.position, targetSampleRadius))
            lastChaseRequestPosition = playerTarget.position;
        else if (!hasDestination)
            PauseAgent();

        nextDestinationUpdate = Time.time + destinationRefreshRate;
    }

    private void UpdateSearch()
    {
        agent.stoppingDistance = defaultStoppingDistance;

        if (Time.time >= searchDeadline)
        {
            FinishSearch();
            return;
        }

        if (hasDestination && !agent.pathPending)
        {
            if (ReachedDestination())
            {
                ClearPath();
                state = State.SearchWait;
                searchUntil = Time.time + searchWaitTime;
                return;
            }

            if (agent.pathStatus != NavMeshPathStatus.PathComplete ||
                !agent.hasPath)
            {
                RecoverStalledRoute();
                return;
            }

            ResumeAgent();
            return;
        }

        if (hasDestination || Time.time < nextDestinationUpdate)
            return;

        if (!SetPathNear(lastKnownPlayerPosition, targetSampleRadius))
            PauseAgent();

        nextDestinationUpdate = Time.time + destinationRefreshRate;
    }

    private void FinishSearch()
    {
        isChasing = false;
        state = State.Patrol;
        waitUntil = Time.time;
        ClearPath();
    }

    private void PickPatrolPoint()
    {
        if (patrolAreas == null || patrolAreas.Length == 0)
        {
            PauseAgent();
            return;
        }

        for (int i = 0; i < patrolAttempts; i++)
        {
            Vector2 direction = Random.insideUnitCircle.normalized;

            if (direction.sqrMagnitude < 0.001f)
                continue;

            Vector3 candidate = transform.position + new Vector3(
                direction.x * patrolRadius,
                0f,
                direction.y * patrolRadius
            );

            if (!NavMesh.SamplePosition(
                    candidate,
                    out NavMeshHit hit,
                    patrolSampleRadius,
                    agent.areaMask))
            {
                continue;
            }

            if (!InsidePatrolArea(hit.position))
                continue;

            if (hasLastPatrolPoint &&
                Vector3.Distance(hit.position, lastPatrolPoint) < 1.5f)
            {
                continue;
            }

            if (!TryGetCompletePath(hit.position, out NavMeshPath path))
                continue;

            if (GetPathLength(path) > maximumPatrolPathLength)
                continue;

            if (!SetPath(path, hit.position))
                continue;

            lastPatrolPoint = hit.position;
            hasLastPatrolPoint = true;
            return;
        }

        PauseAgent();
        waitUntil = Time.time + 1f;
    }

    private bool SetPathNear(Vector3 position, float sampleRadius)
    {
        if (!NavMesh.SamplePosition(
                position,
                out NavMeshHit hit,
                sampleRadius,
                agent.areaMask))
        {
            return false;
        }

        if (!TryGetCompletePath(hit.position, out NavMeshPath path))
            return false;

        return SetPath(path, hit.position);
    }

    private bool TryGetCompletePath(Vector3 point, out NavMeshPath path)
    {
        path = new NavMeshPath();

        return agent.CalculatePath(point, path) &&
               path.status == NavMeshPathStatus.PathComplete;
    }

    private bool SetPath(NavMeshPath path, Vector3 destination)
    {
        if (!agent.SetPath(path))
            return false;

        currentDestination = destination;
        hasDestination = true;
        ResetProgressCheck();
        ResumeAgent();

        return true;
    }

    private void CheckForStall()
    {
        if (!hasDestination || agent.pathPending || agent.isStopped)
        {
            ResetProgressCheck();
            return;
        }

        float elapsed = Time.time - lastProgressCheck;

        if (elapsed < progressCheckInterval)
            return;

        float moved = Vector3.Distance(transform.position, previousPosition);
        bool destinationIsAhead =
            Vector3.Distance(transform.position, currentDestination) >
            Mathf.Max(agent.stoppingDistance + 0.25f, 0.5f);

        if (moved >= minimumPositionProgress || !destinationIsAhead)
            noProgressTime = 0f;
        else
            noProgressTime += elapsed;

        previousPosition = transform.position;
        lastProgressCheck = Time.time;

        if (noProgressTime >= stuckTimeout)
            RecoverStalledRoute();
    }

    private void RecoverStalledRoute()
    {
        if (state == State.Patrol)
        {
            lastPatrolPoint = currentDestination;
            hasLastPatrolPoint = true;
            waitUntil = Time.time + 0.2f;
        }
        else
        {
            nextDestinationUpdate = Time.time + destinationRefreshRate;
        }

        ClearPath();
    }

    private bool ReachedDestination()
    {
        if (agent.pathPending)
            return false;

        float threshold = Mathf.Max(agent.stoppingDistance, 0.15f);
        float remaining = agent.remainingDistance;

        if (!float.IsInfinity(remaining) &&
            !float.IsNaN(remaining) &&
            remaining <= threshold)
        {
            return true;
        }

        return Vector3.Distance(transform.position, currentDestination) <= threshold
               && agent.velocity.sqrMagnitude < 0.04f;
    }

    private bool InsidePatrolArea(Vector3 point)
    {
        foreach (Collider area in patrolAreas)
        {
            if (area != null &&
                area.enabled &&
                (area.ClosestPoint(point) - point).sqrMagnitude <= 0.0025f)
            {
                return true;
            }
        }

        return false;
    }

    private float GetPathLength(NavMeshPath path)
    {
        float length = 0f;
        Vector3[] corners = path.corners;

        for (int i = 1; i < corners.Length; i++)
            length += Vector3.Distance(corners[i - 1], corners[i]);

        return length;
    }

    private void ClearPath()
    {
        hasDestination = false;

        if (AgentReady())
        {
            agent.ResetPath();
            PauseAgent();
        }

        ResetProgressCheck();
    }

    private void PauseAgent()
    {
        if (!AgentReady())
            return;

        agent.isStopped = true;
        agent.updateRotation = false;
    }

    private void ResumeAgent()
    {
        if (!AgentReady())
            return;

        agent.updateRotation = true;
        agent.isStopped = false;
    }

    private void ResetProgressCheck()
    {
        previousPosition = transform.position;
        lastProgressCheck = Time.time;
        noProgressTime = 0f;
    }

    private bool AgentReady()
    {
        return agent != null &&
               agent.enabled &&
               agent.isOnNavMesh;
    }
}