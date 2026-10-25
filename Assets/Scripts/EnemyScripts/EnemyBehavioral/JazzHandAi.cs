using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class JazzHandAI : EnemyBehavior
{
    [Header("Grab")]
    [SerializeField, Min(0.1f)] private float grabDistance = 1.25f;
    [SerializeField, Min(0.1f)] private float playerNavMeshSampleRadius = 3f;
    [SerializeField] private UnityEvent onGrabbed;

    private Transform player;
    private Detection detection;
    private Enemy_Movement movement;

    private bool initialized;
    private bool hasGrabbedPlayer;
    public bool IsReady { get; private set; }

    public override void Initialize(Transform target, Detection detector,
        Enemy_Movement mover)
    {
        player = target;
        detection = detector;
        movement = mover;

        if (player == null || detection == null || movement == null)
        {
            Debug.LogError( $"{name}: JazzHandAI needs a player, Detection, and Enemy_Movement.",
                this);
            return;
        }

        if (movement.agent == null || !movement.agent.isOnNavMesh)
        {
            Debug.LogError($"{name}: Jazz Hand is not on a NavMesh.", this);
            return;
        }

        detection.enabled = false;

        // Check that this spawn point has a complete path to the player.
        if (!HasCompletePathToPlayer())
        {
            Debug.LogWarning($"{name}: no complete NavMesh path from this spawn point to the player.",
                this);
            return;
        }

        movement.SetAttacking(false);
        movement.SetPlayerVisible(true, player);

        initialized = true;
        IsReady = true;

        Debug.Log($"{name}: pursuing the player.", this);
    }

    private void Update()
    {
        if (!initialized || hasGrabbedPlayer || player == null) { return; }

        // Keep the movement script's chase target current.
        movement.SetPlayerVisible(true, player);

        Vector3 difference = player.position - transform.position;
        difference.y = 0f;

        if (difference.magnitude > grabDistance)
        {
            movement.SetAttacking(false);
            return;
        }

        hasGrabbedPlayer = true;
        movement.SetAttacking(true);

        if (difference.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(difference);

        Debug.Log($"{name}: grabbed the player. No killing behavior is attached.", this);
        onGrabbed?.Invoke();
    }

    private bool HasCompletePathToPlayer()
    {
        if (!NavMesh.SamplePosition(
                player.position,
                out NavMeshHit playerNavMeshHit,
                playerNavMeshSampleRadius,
                movement.agent.areaMask))
        {
            return false;
        }

        NavMeshPath path = new NavMeshPath();

        return movement.agent.CalculatePath(playerNavMeshHit.position, path) &&
               path.status == NavMeshPathStatus.PathComplete;
    }
}