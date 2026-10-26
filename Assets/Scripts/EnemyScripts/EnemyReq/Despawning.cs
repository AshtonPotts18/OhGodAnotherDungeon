using UnityEngine;

public class EnemyDespawn : MonoBehaviour
{
    [Header("Despawn Settings")]
    [SerializeField, Min(0f)] private float lifetimeSeconds = 180f;
    [SerializeField, Min(0f)] private float maxDistanceFromPlayer = 60f;

    private Transform player;
    private float spawnedAt;
    private bool despawned;

    private void Awake()
    {
        spawnedAt = Time.time;
    }

    public void Initialize(Transform target)
    {
        player = target;
        spawnedAt = Time.time;
        despawned = false;
    }

    private void Update()
    {
        if (despawned) { return; }

        if (lifetimeSeconds > 0f &&
            Time.time - spawnedAt >= lifetimeSeconds)
        {
            Despawn("lifetime expired");
            return;
        }

        if (player != null && maxDistanceFromPlayer > 0f)
        {
            float maxDistanceSquared =
                maxDistanceFromPlayer * maxDistanceFromPlayer;

            if ((transform.position - player.position).sqrMagnitude >
                maxDistanceSquared)
            {
                Despawn("too far from the player");
            }
        }
    }

    private void Despawn(string reason)
    {
        despawned = true;
        Debug.Log($"{name}: despawned because it was {reason}.", this);
        Destroy(gameObject);
    }
}