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
    [SerializeField] private Transform hallwaysRoot;
    [SerializeField] private string hallwayBoundsObjectName = "HallwayBounds";

    [Header("Level Scaling")]
    [SerializeField, Min(1)] public int currentLevel = 1;
    [SerializeField, Min(1)] private int enemiesAtLevelOne = 1;
    [SerializeField, Min(1)] private int extraEnemiesPerLevel = 1;
    [SerializeField, Min(1)] private int maxEnemyLimit = 20;

    [Header("Spawn Timing")]
    [SerializeField, Min(0f)] private float initialSpawnDelay = 3f;
    [SerializeField, Min(0.1f)] private float minimumSpawnInterval = 8f;
    [SerializeField, Min(0.1f)] private float maximumSpawnInterval = 14f;

    [Header("Spawn Validation")]
    [SerializeField, Min(0.1f)] private float navMeshSampleRadius = 2f;
    [SerializeField, Min(1)] private int spawnAttemptsPerRoom = 24;
    [SerializeField, Min(1f)] private float maximumSpawnDistanceFromPlayer = 40f;
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

    private void RefreshRoom()
    {
        roomAreas.Clear();

        AddBoundsUnderRoot(roomsRoot, roomBoundsObjectName);
        AddBoundsUnderRoot(hallwaysRoot, hallwayBoundsObjectName);
    }

    private void AddBoundsUnderRoot(Transform root, string boundsName)
    {
        if (root == null)
            return;

        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);

        foreach (Collider roomCollider in colliders)
        {
            if (roomCollider == null ||
                !roomCollider.enabled ||
                !roomCollider.gameObject.activeInHierarchy ||
                !roomCollider.isTrigger ||
                roomCollider.gameObject.name != boundsName)
            {
                continue;
            }

            if (!roomAreas.Contains(roomCollider))
                roomAreas.Add(roomCollider);
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
        if (prefabToSpawn == null)
            return false;

        RefreshRoom();

        if (roomAreas.Count == 0)
        {
            Debug.LogWarning(
                $"{name}: No room or hallway bounds were found.",
                this);
            return false;
        }

        Collider occupiedArea = FindPlayerArea();

        List<Collider> eligibleAreas = new List<Collider>();

        foreach (Collider area in roomAreas)
        {
            if (area == null ||
                area == occupiedArea ||
                IsPlayerSpawnerArea(area))
            {
                continue;
            }

            eligibleAreas.Add(area);
        }

        if (eligibleAreas.Count == 0)
        {
            Debug.LogWarning(
                $"{name}: No unoccupied room or hallway area is available for spawning.",
                this);
            return false;
        }

        // Shuffle the candidates so spawns don't always favor the same area.
        for (int i = eligibleAreas.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            Collider temporary = eligibleAreas[i];
            eligibleAreas[i] = eligibleAreas[swapIndex];
            eligibleAreas[swapIndex] = temporary;
        }

        foreach (Collider area in eligibleAreas)
        {
            if (!TryGetSpawnPoint(area, out Vector3 spawnPosition))
                continue;

            GameObject enemy = Instantiate(
                prefabToSpawn,
                spawnPosition,
                prefabToSpawn.transform.rotation);

            enemy.tag = "Enemy";
            Debug.Log($"Spawned {enemy.name} at {spawnPosition}", enemy);

            EnemyDespawn despawn = enemy.GetComponent<EnemyDespawn>();
            if (despawn == null)
                despawn = enemy.AddComponent<EnemyDespawn>();

            despawn.Initialize(player);

            Enemy_Movement movement =
                enemy.GetComponentInChildren<Enemy_Movement>();

            if (movement != null)
            {
                movement.Initialize(roomAreas.ToArray(), player);
            }
            else
            {
                Debug.LogError(
                    $"{enemy.name}: Enemy_Movement component not found.",
                    enemy);
            }

            Detection detection = enemy.GetComponentInChildren<Detection>();
            if (detection != null)
            {
                detection.Initialize(player, movement);
            }
            else
            {
                Debug.LogError(
                    $"{enemy.name}: Detection component not found.",
                    enemy);
            }

            EnemyBehavior behavior =
                enemy.GetComponentInChildren<EnemyBehavior>();

            if (behavior != null)
            {
                behavior.Initialize(player, detection, movement);
            }
            else
            {
                Debug.LogError(
                    $"{enemy.name}: No EnemyBehavior component found.",
                    enemy);
            }

            spawnedEnemies.Add(enemy);
            return true;
        }

        Debug.LogWarning(
            $"{name}: Couldn't find a clear NavMesh spawn point in any eligible room or hallway.",
            this);

        return false;
    }

    private Collider FindPlayerArea()
    {
        if (player == null)
        {
            Debug.LogError($"{name}: Player reference is null.", this);
            return null;
        }

        if (!player.gameObject.activeInHierarchy)
        {
            Debug.LogError($"{name}: Player '{player.name}' is inactive.", player);
            return null;
        }

        Vector3 position = player.position;
        Debug.Log(
            $"{name}: Player={player.name}, position={position}, " +
            $"collected areas={roomAreas.Count}",
            player);

        foreach (Collider area in roomAreas)
        {
            if (area == null)
                continue;

            bool containsPlayer = IsInsideAreaXZ(area, position);
            Debug.Log(
                $"Area={area.name}, parent={area.transform.parent?.name}, " +
                $"center={area.bounds.center}, size={area.bounds.size}, " +
                $"containsPlayer={containsPlayer}",
                area);

            if (containsPlayer)
                return area;
        }

        return null;
    }

    private bool IsPlayerSpawnerArea(Collider area)
    {
        if (area == null || roomsRoot == null)
            return false;

        Transform areaParent = area.transform.parent;

        return areaParent != null &&
               areaParent.name == "RoomsRoot" &&
               (areaParent == roomsRoot || areaParent.IsChildOf(roomsRoot));
    }

    private bool IsInsideAreaXZ(Collider area, Vector3 position)
    {
        Bounds bounds = area.bounds;

        return position.x >= bounds.min.x &&
               position.x <= bounds.max.x &&
               position.z >= bounds.min.z &&
               position.z <= bounds.max.z;
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
            IsSpawnBlocked(hit.position) ||
            Vector3.Distance(player.position, hit.position) >
            maximumSpawnDistanceFromPlayer)
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

        return Physics.CheckCapsule(lowerPoint, upperPoint, radius,
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