using UnityEngine;
using System;
using UnityEngine.Events;
public class SnapHandAI : EnemyBehavior
{
    [Header("Snap")]
    [SerializeField, Min(0f)] private float snapDelay = 20f;
    [SerializeField] private UnityEvent onSnap;

    private Transform player;
    private Detection detection;
    private Enemy_Movement movement;

    private float snapAtTime;
    private bool initialized;
    private bool hasSnapped;

    public event Action<SnapHandAI> Snapped;

    public override void Initialize(Transform target, Detection detector, 
        Enemy_Movement mover)
    {
        player = target;
        detection = detector;
        movement = mover;

        if (player == null || detection == null || movement == null)
        {
            Debug.LogError($"{name}: SnapHandAI needs player, detection, or Enemy_Movement."
                , this);
            return;
        }

        detection.enabled = false;

        movement.SetAttacking(false);

        snapAtTime = Time.time + snapDelay;
        hasSnapped = false;
        initialized = true;

        Debug.Log($"{name}: patrolling until it snaps.", this);

    }
    private void Update()
    {
        if (!initialized || hasSnapped) { return; }

        if (Time.time < snapAtTime) { return; }

        hasSnapped = true;

        // Detection now drives the existing Enemy_Movement chase/search behavior.
        detection.enabled = true;

        Debug.Log($"{name}: snapped and is now searching for the player.", this);

        int listenerCount = onSnap.GetPersistentEventCount();

        if (listenerCount == 0)
        {
            Debug.LogWarning(
                $"{name}: snap event fired, but On Snap has no Inspector listeners.",
                this);
        }
        else
        {
            Debug.Log(
                $"{name}: firing On Snap ({listenerCount} Inspector listener(s)).",
                this);
        }

        onSnap?.Invoke();
        Snapped?.Invoke(this);
    }
}
