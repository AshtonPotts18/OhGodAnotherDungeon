using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SlimeAI : EnemyBehavior
{
    [Header("Movement")]
    [Tooltip("Multiplier applied to the Slime's NavMeshAgent speed.")]
    [SerializeField, Range(0.1f, 1f)]
    private float speedMultiplier = 0.65f;

    [Header("Damage Over Time")]
    [SerializeField, Min(0f)] private float attackRange = 1.8f;
    [SerializeField, Min(0f)] private float damagePerTick = 2f;
    [SerializeField, Min(0.05f)] private float damageInterval = 0.75f;
    [SerializeField, Min(0f)] private float damageStartDelay = 0.4f;
    [SerializeField, Min(0f)] private float poisonDuration = 4f;

    [Header("Events")]
    [SerializeField] private UnityEvent onAttackStarted;
    [SerializeField] private UnityEvent onDamageTick;
    [SerializeField] private UnityEvent onAttackFinished;

    private Transform player;
    private Detection detection;
    private Enemy_Movement movement;
    private DamagingEnemy.IDamageable playerHealth;
    private bool initialized;
    private bool isAttacking;
    private bool hasOriginalAgentSpeed;
    private bool warnedMissingHealth;
    private float originalAgentSpeed;
    private float nextDamageTime;
    private bool poisonActive;
    private float poisonExpiresAt;

    public override void Initialize(Transform target, Detection detector, Enemy_Movement mover)
    {
        player = target;
        detection = detector;
        movement = mover;

        if(player == null || detection == null || movement == null)
        {
            Debug.LogError($"{name}: SlimeAI needs a player, Detection, and Enemy_Movement.", this);
            initialized = false;
            return;
        }

        playerHealth = FindDamageable(player);
        WarnIfPlayerHasNoDamageable();
        ApplySlowerMovementSpeed();

        movement.SetAttacking(false);
        isAttacking = false;
        nextDamageTime = 0f;
        initialized = true;

        Debug.Log($"{name}: slime behavior initialized.", this);
    }
    private void Update()
    {
        if (!initialized || player == null || detection == null || movement == null) { return; }

        bool inAttackRange = detection.PlayerDetected && IsPlayerInAttackRange();

        movement.SetAttacking(inAttackRange);

        if (inAttackRange)
        {
            if (!isAttacking) { BeginAttack(); }

            ApplyOrRefreshPoison();
        }
        else if (isAttacking)
        {
            FinishAttack();
        }

        // Poison keeps ticking even after the player leaves attack range.
        TickPoison();
    }

    private void ApplyOrRefreshPoison()
    {
        if (!poisonActive)
        {
            poisonActive = true;
            nextDamageTime = Time.time + damageStartDelay;
        }

        // Hitting again refreshes the duration; it does not stack another poison.
        poisonExpiresAt = Time.time + poisonDuration;
    }

    private void TickPoison()
    {
        if (!poisonActive) { return; }

        if (Time.time >= poisonExpiresAt)
        {
            poisonActive = false;
            return;
        }

        if (Time.time < nextDamageTime) { return; }

        DealDamageTick();
        nextDamageTime = Time.time + Mathf.Max(0.05f, damageInterval);
    }

    private void BeginAttack()
    {
        isAttacking = true;
        movement.SetAttacking(true);
        nextDamageTime = Time.time + damageStartDelay;

        onAttackStarted?.Invoke();
    }

    private void DealDamageTick()
    {
        if (damagePerTick <= 0f) { return; }

        if (playerHealth == null) { playerHealth = FindDamageable(player); }

        if (playerHealth == null)
        {
            WarnIfPlayerHasNoDamageable();
            return;
        }

        DealDamage(playerHealth, damagePerTick);
        onDamageTick?.Invoke();
    }

    private void ApplySlowerMovementSpeed()
    {
        if (movement == null || movement.agent == null)
        {
            Debug.LogWarning($"{name}: no NavMeshAgent found; slime speed was not adjusted.", this);
            return;
        }

        if (!hasOriginalAgentSpeed)
        {
            originalAgentSpeed = movement.agent.speed;
            hasOriginalAgentSpeed = true;
        }

        movement.agent.speed = originalAgentSpeed * speedMultiplier;
    }
    private bool IsPlayerInAttackRange()
    {
        if (player == null)
            return false;

        Vector3 offset = player.position - transform.position;
        offset.y = 0f;

        return offset.sqrMagnitude <= attackRange * attackRange;
    }

    private void WarnIfPlayerHasNoDamageable()
    {
        if (playerHealth != null || warnedMissingHealth)
            return;

        Debug.LogWarning(
            $"{name}: the player has no component implementing IDamageable. " +
            "Slime damage will not reduce health.",
            this);

        warnedMissingHealth = true;
    }

    private void FinishAttack()
    {
        if (!isAttacking)
            return;

        isAttacking = false;

        if (movement != null)
            movement.SetAttacking(false);

        onAttackFinished?.Invoke();
    }

    private void OnDisable()
    {
        FinishAttack();
    }
}
