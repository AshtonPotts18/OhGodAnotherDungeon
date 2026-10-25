using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Transform roomsRoot;
    [SerializeField] private Transform player;
    [SerializeField] private string roomBoundsObjectName = "RoomBounds";
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject[] additionalEnemyPrefabs;

    [Header("Level Scaling")]
    [SerializeField, Min(1)] private int currentLevel = 1;
    [SerializeField, Min(1)] private int enemiesAtLevelOne = 1;
    [SerializeField, Min(1)] private int extraEnemiesPerLevel = 1;
    [SerializeField, Min(1)] private int maxEnemyLimit = 10;

    [Header("Spawn Timing")]
    [SerializeField, Min(0f)] private float initialSpawnDelay = 3f;
    [SerializeField, Min(0.1f)] private float minimumSpawnInterval = 8f;
    [SerializeField, Min(0.1f)] private float maximumSpawnInterval = 14f;

    [Header("Spawn Validation")]
    [SerializeField, Min(0.1f)] private float navMeshSampleRadius = 2f;
    [SerializeField, Min(1)] private int spawnAttemptsPerRoom = 24;
    [SerializeField] private int navMeshAreaMask = NavMesh.AllAreas;
    [SerializeField, Min(0.05f)] private float clearanceRadius = 0.4f;
    [SerializeField, Min(0.2f)] private float clearanceHeight = 1.8f;
    [SerializeField] private LayerMask blockingLayers = ~0;

    private readonly List<Collider> roomAreas = new List<Collider>();
    private readonly List<GameObject> spawnedEnemies = new List<GameObject>();

    private float nextSpawnTime;

    private void Start()
    {
        if (roomsRoot == null || GetEnemyPrefabCount() == 0)
        {
            Debug.LogError(
                $"{name}: Assign Rooms Root and at least one Enemy Prefab.",
                this
            );

            enabled = false;
            return;
        }

        nextSpawnTime = Time.time + Mathf.Max(0f, initialSpawnDelay);
            
    }

    private void Update()
    {
        UpdatePlayerReference();

        if (player == null)
        { return; }

        RemoveDestroyedEnemies();

        if (Time.time < nextSpawnTime || spawnedEnemies.Count >= GetEnemyLimit())
        { return; }

        TrySpawnEnemy();
        ScheduleNextSpawn();
    }

    private void ScheduleNextSpawn()
    {
        float minimum = Mathf.Max(0.1f, minimumSpawnInterval);
        float maximum = Mathf.Max(minimum, maximumSpawnInterval);

        nextSpawnTime = Time.time + Random.Range(minimum, maximum);
    }

    private void UpdatePlayerReference()
    {
        bool hasLivePlayer = player != null &&
            player.gameObject.scene.IsValid() &&
            player.gameObject.activeInHierarchy;

        if (hasLivePlayer) { return; }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null) { player = playerObject.transform; }
    }

    public void SetLevel(int level)
    {
        currentLevel = Mathf.Max(1, level);
    }

    public void RefreshRoom()
    {
        roomAreas.Clear();

        if (roomsRoot == null) { return; }

        Collider[] colliders =
            roomsRoot.GetComponentsInChildren<Collider>(true);

        foreach (Collider roomCollider in colliders)
        {
            if (roomCollider != null &&
                roomCollider.enabled &&
                roomCollider.gameObject.activeInHierarchy &&
                roomCollider.isTrigger &&
                roomCollider.gameObject.name == roomBoundsObjectName)
            {
                roomAreas.Add(roomCollider);
            }
        }
    }

    public void SpawnNow()
    {
        RemoveDestroyedEnemies();

        if (spawnedEnemies.Count < GetEnemyLimit())
        {
            TrySpawnEnemy();
            ScheduleNextSpawn();
        }
    }

    private int GetEnemyLimit()
    {
        int scaledLimit = enemiesAtLevelOne + (currentLevel - 1) 
            * extraEnemiesPerLevel;

        return Mathf.Clamp(scaledLimit, 0, maxEnemyLimit);
    }

    private int GetEnemyPrefabCount()
    {
        int count = enemyPrefab != null ? 1 : 0;

        if (additionalEnemyPrefabs == null)
            { return count; }

        foreach (GameObject prefab in additionalEnemyPrefabs)
        {
            if (prefab != null)
                count++;
        }

        return count;
    }

    private GameObject GetRandomEnemyPrefab()
    {
        int count = GetEnemyPrefabCount();

        if (count == 0) { return null; }

        int selectedIndex = Random.Range(0, count);

        if (enemyPrefab != null)
        {
            if (selectedIndex == 0) { return enemyPrefab; }

            selectedIndex--;
        }

        foreach (GameObject prefab in additionalEnemyPrefabs)
        {
            if (prefab == null) { continue; }

            if (selectedIndex == 0) { return prefab; }

            selectedIndex--;
        }

        return enemyPrefab;
    }

    private bool TrySpawnEnemy()
    {
        Debug.Log("TrySpawnEnemy was reached.", this);
        GameObject prefabToSpawn = GetRandomEnemyPrefab();

        if (prefabToSpawn == null) { return false; }

        RefreshRoom();

        if (roomAreas.Count == 0)
        {
            Debug.LogWarning(
                $"{name}: No trigger colliders named '{roomBoundsObjectName}' " +
                "found under Rooms Root.",
                this
            );

            return false;
        }

        Collider occupiedRoom = FindPlayerRoom();

        if (occupiedRoom == null)
        {
            Debug.LogWarning(
                $"{name}: Couldn't determine the player's room. No enemy was spawned.",
                this
            );

            return false;
        }

        List<Collider> eligibleRooms = new List<Collider>();

        foreach (Collider roomArea in roomAreas)
        {
            if (roomArea != occupiedRoom)
                eligibleRooms.Add(roomArea);
        }

        if (eligibleRooms.Count == 0)
        {
            Debug.LogWarning(
                $"{name}: No unoccupied room is available for spawning.",
                this
            );

            return false;
        }

        // Shuffle eligible rooms so the first usable room varies.
        for (int i = eligibleRooms.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            Collider temporary = eligibleRooms[i];
            eligibleRooms[i] = eligibleRooms[swapIndex];
            eligibleRooms[swapIndex] = temporary;
        }

        foreach (Collider roomArea in eligibleRooms)
        {
            if (!TryGetSpawnPoint(roomArea, out Vector3 spawnPosition))
            { continue; }

            GameObject enemy = Instantiate(prefabToSpawn,
            spawnPosition, prefabToSpawn.transform.rotation );

            enemy.tag = "Enemy";

            Debug.Log($"Spawned {enemy.name} at {spawnPosition}", enemy);

            EnemyDespawn despawn = enemy.GetComponent<EnemyDespawn>();

            if (despawn == null) { despawn = enemy.AddComponent<EnemyDespawn>(); }

            despawn.Initialize(player);

            // Movement is the only script that sets NavMesh destinations.
            Enemy_Movement movement =
            enemy.GetComponentInChildren<Enemy_Movement>();

            if (movement != null)
            { movement.Initialize(roomAreas.ToArray(), player); }
            else
            {
                Debug.LogError( $"{enemy.name}: Enemy_Movement component not found.",
                enemy);
            }

            Detection detection = enemy.GetComponentInChildren<Detection>();

            if (detection != null)
            { detection.Initialize(player, movement); }
            else
            {
                Debug.LogError($"{enemy.name}: Detection component not found.",
                enemy);
            }

            EnemyBehavior behavior = enemy.GetComponentInChildren<EnemyBehavior>();

            if (behavior != null)
            { behavior.Initialize(player, detection, movement); }
            else
            {
                Debug.LogError(
                    $"{enemy.name}: No EnemyBehavior component found. " +
                    "Add a behavior script derived from EnemyBehavior.",
                    enemy );
            }

            spawnedEnemies.Add(enemy);
            return true;
        }

        Debug.LogWarning( $"{name}: No clear NavMesh spawn point " +
            $"found in the eligible rooms.",this );

        return false;
    }

    private Collider FindPlayerRoom()
    {
        if (player == null || !player.gameObject.activeInHierarchy)
        { return null; }

        return FindRoomContaining(player.position);
    }

    private Collider FindRoomContaining(Vector3 position)
    {
        foreach (Collider roomArea in roomAreas)
        {
            if (IsPointInside(roomArea, position))
                return roomArea;
        }

        return null;
    }

    private bool TryGetSpawnPoint(
        Collider roomArea,
        out Vector3 spawnPosition)
    {
        Bounds bounds = roomArea.bounds;
        float sampleRadius = Mathf.Max(0.1f, navMeshSampleRadius);

        for (int attempt = 0; attempt < spawnAttemptsPerRoom; attempt++)
        {
            Vector3 candidate = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                bounds.min.y + Mathf.Min(sampleRadius * 0.5f, bounds.size.y * 0.5f),
                Random.Range(bounds.min.z, bounds.max.z)
            );

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit,
                    sampleRadius, navMeshAreaMask))
            { continue; }

            if (!IsPointInside(roomArea, hit.position) ||
                IsSpawnBlocked(hit.position))
            { continue; }

            spawnPosition = hit.position;
            return true;
        }

        spawnPosition = Vector3.zero;
        return false;
    }

    private bool IsPointInside(Collider roomArea, Vector3 point)
    {
        Bounds bounds = roomArea.bounds;

        return point.x >= bounds.min.x &&
               point.x <= bounds.max.x &&
               point.z >= bounds.min.z &&
               point.z <= bounds.max.z;
    }

    private bool IsSpawnBlocked(Vector3 position)
    {
        float radius = Mathf.Max(0.05f, clearanceRadius);
        float height = Mathf.Max(clearanceHeight, radius * 2f);

        Vector3 lowerPoint = position + Vector3.up * radius;
        Vector3 upperPoint = position + Vector3.up * (height - radius);

        return Physics.CheckCapsule( lowerPoint, upperPoint, radius,
            blockingLayers, QueryTriggerInteraction.Ignore);
    }

    private void RemoveDestroyedEnemies()
    {
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (spawnedEnemies[i] == null)
                spawnedEnemies.RemoveAt(i);
        }
    }
}