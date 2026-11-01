using UnityEngine;

public class EnemyDespawn : MonoBehaviour
{
    [Header("Despawn Settings")]
    [SerializeField, Min(0f)] private float lifetimeSeconds = 180f;
    [SerializeField, Min(0f)] private float maxDistanceFromPlayer = 100f;

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
        if (despawned)
            return;

        if (lifetimeSeconds > 0f &&
            Time.time - spawnedAt >= lifetimeSeconds)
        {
            Despawn("lifetime expired");
            return;
        }

        if (player == null ||
            !player.gameObject.activeInHierarchy ||
            maxDistanceFromPlayer <= 0f)
        {
            return;
        }

        float distanceSquared =
            (transform.position - player.position).sqrMagnitude;
        float maxDistanceSquared =
            maxDistanceFromPlayer * maxDistanceFromPlayer;

        if (distanceSquared > maxDistanceSquared)
        {
            float distance = Mathf.Sqrt(distanceSquared);
            Despawn(
                $"too far from the player " +
                $"(distance {distance:0.0}, limit {maxDistanceFromPlayer:0.0})");
        }
    }

    private void Despawn(string reason)
    {
        despawned = true;
        Debug.Log($"{name}: despawned because it was {reason}.", this);
        Destroy(gameObject);
    }
}