using UnityEngine;

public class Detection : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform visionPoint;
    public Enemy_Movement enemy;

    [Header("Vision")]
    public float viewDistance = 50f;
    [Range(0f, 280f)] public float viewAngle = 220f;

    [Tooltip("Select wall and obstacle layers only. Exclude Player and Enemy.")]
    public LayerMask obstacleMask;

    public bool PlayerDetected { get; private set; }

    public void Initialize(Transform target, Enemy_Movement movement)
    {
        player = target;
        enemy = movement;
        if (visionPoint == null) { visionPoint = transform; }
    }

    private void Awake()
    {
        if (enemy == null) { enemy = GetComponentInParent<Enemy_Movement>(); }
        if (visionPoint == null) { visionPoint = transform; }
    }

    private void Update()
    {
        if (player == null || enemy == null || visionPoint == null)
        {
            if (PlayerDetected)
            { 
                Debug.Log($"{name}: lost player because a Detection reference is missing.", this);
                PlayerDetected = false;
            }

            if (enemy != null) { enemy.SetPlayerVisible(false, player); }

            return;
        }

        bool wasDetected = PlayerDetected;
        string reason;
        float distance;

        PlayerDetected = CanSeePlayer(out reason, out distance);

        if (PlayerDetected && !wasDetected)
        {
            Debug.Log($"{name}: detected player at {distance:F1} units.", this);
        }
        else if (!PlayerDetected && wasDetected)
        {
            Debug.Log(
                $"{name}: lost player ({reason}; {distance:F1}/{viewDistance:F1} units).",
                this
            );
        }

        enemy.SetPlayerVisible(PlayerDetected, player);
    }

    private bool CanSeePlayer(out string reason, out float distance)
    {
        reason = "visible";
        distance = 0f;

        Vector3 direction = player.position - visionPoint.position;
        distance = direction.magnitude;

        if (distance > viewDistance)
        {
            reason = "outside view distance";
            return false;
        }

        float angle = Vector3.Angle(visionPoint.forward, direction);

        if (!enemy.isChasing && angle > viewAngle * 0.5f)
        {
            reason = "outside view angle";
            return false;
        }

        if (Physics.Raycast(
            visionPoint.position,
            direction.normalized,
            distance,
            obstacleMask,
            QueryTriggerInteraction.Ignore))
        {
            reason = "blocked by an obstacle";
            Debug.DrawRay(visionPoint.position, direction, Color.red);
            return false;
        }

        Debug.DrawRay(visionPoint.position, direction, Color.green);
        return true;
    }
}
