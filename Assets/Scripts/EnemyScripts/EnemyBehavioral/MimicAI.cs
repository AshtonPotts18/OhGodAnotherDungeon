using UnityEngine;
using UnityEngine.Events;

public class MimicAI : EnemyBehavior, IInteract
{
    [Header("Attack")]
    [SerializeField, Min(0f)] private float attackRange = 1.8f;
    [SerializeField, Min(0f)] private float damagePerTick = 10f;
    [SerializeField, Min(0.05f)] private float damageInterval = 1.5f;
    [SerializeField, Min(0f)] private float damageStartDelay = 0.4f;
    

    [Header("Events")]
    [SerializeField] private UnityEvent onAttackStarted;
    [SerializeField] private UnityEvent onDamageTick;
    [SerializeField] private UnityEvent onAttackFinished;

    private Transform player;
    private Detection detection;
    private Enemy_Movement movement;
    private DamagingEnemy.IDamageable playerHealth;

    private bool initialized;
    private bool activated;
    private bool isAttacking;
    private bool warnedMissingHealth;
    private float nextDamageTime;
   

    public override void Initialize(
        Transform target,
        Detection detector,
        Enemy_Movement mover)
    {
        player = target;
        detection = detector;
        movement = mover;

        if (player == null || detection == null || movement == null)
        {
            Debug.LogError(
                $"{name}: MimicAI needs a player, Detection, and Enemy_Movement.",
                this);
            initialized = false;
            return;
        }

        playerHealth = FindDamageable(player);
        WarnIfPlayerHasNoDamageable();

        // Keep the Mimic still until the player interacts with it.
        detection.enabled = false;
        movement.SetAttacking(true);

        isAttacking = false;
        nextDamageTime = 0f;
        initialized = true;

        Debug.Log($"{name}: waiting to be interacted with.", this);
    }

    // Called by the player's interaction system.
    public void Interact()
    {
        if (!initialized || activated)
            return;

        activated = true;

        // Keep detection off so losing sight cannot stop the chase.
        detection.enabled = false;
        movement.SetAttacking(false);
        movement.SetPlayerVisible(true, player);

        Debug.Log($"{name}: activated and chasing the player.", this);
    }

    private void Update()
    {
        if (!initialized || !activated ||
            player == null || movement == null)
            return;

        bool inAttackRange = IsPlayerInAttackRange();

        movement.SetAttacking(inAttackRange);

        if (!inAttackRange)
        {
            if (isAttacking)
                FinishAttack();

            return;
        }

        if (!isAttacking)
            BeginAttack();

        if (Time.time < nextDamageTime)
            return;

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
        if (damagePerTick <= 0f)
            return;

        if (playerHealth == null)
            playerHealth = FindDamageable(player);

        if (playerHealth == null)
        {
            WarnIfPlayerHasNoDamageable();
            return;
        }

        DealDamage(playerHealth, damagePerTick);
        onDamageTick?.Invoke();
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
            "Mimic damage will not reduce health.",
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