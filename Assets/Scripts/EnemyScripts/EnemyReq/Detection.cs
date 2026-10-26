using UnityEngine;

public class Detection : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform visionPoint;
    public Enemy_Movement enemy;

    [Header("Vision")]
    public float viewDistance = 10f;
    [Range(0f, 180f)] public float viewAngle = 90f;

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
            PlayerDetected = false;
            if (enemy != null) { enemy.SetPlayerVisible(false, player); }

            return;
        }

        PlayerDetected = CanSeePlayer();
        enemy.SetPlayerVisible(PlayerDetected, player);
    }

    private bool CanSeePlayer()
    {
        Vector3 direction = player.position - visionPoint.position;
        float distance = direction.magnitude;

        if (distance > viewDistance) { return false; }
        if (Vector3.Angle(visionPoint.forward, direction) > viewAngle * 0.5f)
        { return false; }
        if (Physics.Raycast(visionPoint.position, direction.normalized,
            distance, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawRay(visionPoint.position, direction, Color.red);
            return false;
        }
        Debug.DrawRay(visionPoint.position, direction, Color.green);
        return true;
    }
}
