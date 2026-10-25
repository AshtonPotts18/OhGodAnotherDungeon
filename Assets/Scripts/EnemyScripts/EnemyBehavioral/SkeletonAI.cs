using UnityEngine;
using UnityEngine.Events;
using static DamagingEnemy;

public class SkeletonAI : EnemyBehavior
{
    [Header("Attack")]
    [SerializeField] private float attackDistance = 1.5f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private UnityEvent onAttack;

    private Transform player;
    private Detection detection;
    private Enemy_Movement movement;
    private IDamageable playerHealth;
    private float nextAttackTime;
    private bool warnedMissingHealth;

    public override void Initialize( Transform target,
        Detection detector, Enemy_Movement mover)
    {
        player = target;
        detection = detector;
        movement = mover;
        playerHealth = FindDamageable(player);

        if (playerHealth == null && !warnedMissingHealth)
        {
            Debug.LogWarning(
                $"{name}: the player has no component implementing IDamageable.",
                this);
            warnedMissingHealth = true;
        }
    }

    private void Awake()
    {
        if (detection == null) { detection = GetComponentInChildren<Detection>(); }

        if (movement == null) { movement = GetComponentInChildren<Enemy_Movement>(); }
    }

    private void Update()
    {
        if (player == null || detection == null || movement == null) { return; }

        bool inAttackRange = detection.PlayerDetected &&
            Vector3.Distance(transform.position, player.position) <= attackDistance;

        movement.SetAttacking(inAttackRange);

        if (inAttackRange && Time.time >= nextAttackTime)
        {
            onAttack?.Invoke();

            if (playerHealth == null)
                playerHealth = FindDamageable(player);

            playerHealth?.TakeDamage(damageAmount);
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private IDamageable FindDamageable(Transform target)
    {
        if (target == null) { return null; }

        MonoBehaviour[] components =
            target.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour component in components)
        {
            if (component is IDamageable damageable) { return damageable; }
        }

        return null;
    }

    private void OnDisable()
    {
        if (movement != null)
            movement.SetAttacking(false);
    }
}