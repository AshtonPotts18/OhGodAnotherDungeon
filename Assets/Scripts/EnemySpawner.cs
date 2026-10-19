using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
public class EnemySpawner : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Transform roomsRoot;
    [SerializeField] private Transform player;
    [SerializeField] string roomBoundsObjectName = "RoomBounds";
    [SerializeField] private GameObject enemyPrefab;

    [Header("Level Scaling")]
    [SerializeField, Min(1)] private int currentLevel = 1;
    [SerializeField, Min(1)] private int enemiesAtLevelOne = 1;
    [SerializeField, Min(1)] private int extraEnemiesPerLevel = 1;
    [SerializeField, Min(1)] private int maxEnemyLimit = 10;

    [Header("Spawn Timing")]
    [SerializeField] private bool spawnImmediately = true;
    [SerializeField, Min(0.1f)] private float spawnInterval;

    [Header("Spawn Validation")]
    [SerializeField, Min(0.1f)] private float navMeshSampleRadius = 2f;
    [SerializeField, Min(1)] private int spawnAttemptsPerRoom = 24;
    [SerializeField] private int navMeshAreaMask = NavMesh.AllAreas;
    [SerializeField, Min(0.05f)] private float clearanceRadius = 0.4f;
    [SerializeField, Min(0.2f)] private float clearanceHeight = 1.8f;
    [SerializeField] private LayerMask blockingLayers = ~0;
    private readonly List<Collider> roomAreas = new List<Collider>(); // Stores all room-boundary trigger colliders found under Rooms Root.
    private readonly List<GameObject> spawnedEnemies = new List<GameObject>(); // Stores enemies created by this spawner so their count can be tracked.
    private float nextSpawnTime;

    private void Start()
    {
        if (roomsRoot == null || enemyPrefab == null)
        {
            Debug.LogError($"{name}: Assign Rooms Root, and Enemy Prefab.", this);
            enabled = false;
            return;
        }

        nextSpawnTime = spawnImmediately ? Time.time : Time.time + Mathf.Max(0.1f, spawnInterval);
    }

    private void Update()
    {
        UpdatePlayerReference();
        if(player == null) { return; }
        RemoveDestroyEnemies();

        if (Time.time < nextSpawnTime || spawnedEnemies.Count >= GetEnemyLimit())
        {
            return;
        }

        nextSpawnTime = Time.time + Mathf.Max(0.1f, spawnInterval);
        TrySpawnEnemy();
    }

    private void UpdatePlayerReference()
    {
        bool hasLivePlayer = player != null && player.gameObject.scene.IsValid() &&
            player.gameObject.activeInHierarchy;
        if(hasLivePlayer) { return; }
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) { player = playerObject.transform; }
    }

    //Tracks Level number to make sure the map is never the same
    public void SetLevel(int level) { currentLevel = Mathf.Max(1, level); }

    //Searches under Rooms Root for active trigger colliders whose GameObject is named Room Bounds.
    public void RefreshRoom()
    {
        roomAreas.Clear();

        if (roomsRoot == null) { return; }

        Collider[] colliders = roomsRoot.GetComponentsInChildren<Collider>(true);
        foreach( Collider roomCollider in colliders)
        {
            if (roomCollider != null && roomCollider.enabled &&
                roomCollider.gameObject.activeInHierarchy && roomCollider.isTrigger
                && roomCollider.gameObject.name == roomBoundsObjectName)
            { roomAreas.Add(roomCollider);  }
            
        }
    }
    //Lets another script or UnityEvent request an immediate spawn, if the enemy limit is not met.
    public void SpawnNow()
    {
        RemoveDestroyEnemies();
        if(spawnedEnemies.Count < GetEnemyLimit())
        {
            TrySpawnEnemy();
            nextSpawnTime = Time.time + Mathf.Max(0.1f, spawnInterval);
        }
    }

    //Calculates the enemy Limit for the current level.
    private int GetEnemyLimit()
    {
        int scaledLimit = enemiesAtLevelOne + (currentLevel - 1) * extraEnemiesPerLevel;
        return Mathf.Clamp(scaledLimit, 0, maxEnemyLimit);
    }

    //Refreshes the room list, finds the player's room, selects another room, finds a safe spawn point,
    // and creates the enemy
    private bool TrySpawnEnemy()
    {
        RefreshRoom();

        if (roomAreas.Count == 0)
        {
            Debug.LogWarning( $"{name}: No trigger colliders named '{roomBoundsObjectName}' found under Rooms Root.", this);
            return false;
        }

        Collider occupiedRoom = FindPlayerRoom();

        if (occupiedRoom == null)
        {
            Debug.LogWarning( $"{name}: Couldn't determine the player's room. No enemy was spawned.", this);
            return false;
        }

        List<Collider> eligibleRooms = new List<Collider>();

        foreach (Collider roomArea in roomAreas)
        {
            if (roomArea != occupiedRoom)
            {
                eligibleRooms.Add(roomArea);
            }
        }

        if (eligibleRooms.Count == 0)
        {
            Debug.LogWarning( $"{name}: No unoccupied room is available for spawning.", this);
            return false;
        }

        for (int i = eligibleRooms.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            Collider temporary = eligibleRooms[i];
            eligibleRooms[i] = eligibleRooms[swapIndex];
            eligibleRooms[swapIndex] = temporary;
        }

        foreach (Collider roomArea in eligibleRooms)
        {
            if (!TryGetSpawnPoint(roomArea, out Vector3 spawnPosition)) { continue; }
            GameObject enemy = Instantiate( enemyPrefab, spawnPosition, enemyPrefab.transform.rotation);
            Enemy_Movement movement = enemy.GetComponentInChildren<Enemy_Movement>();

            if (movement != null)
            {
                movement.Initialize(roomArea);
            }
            else
            {
                Debug.LogError($"{enemy.name}: Enemy_Movement component not found.", enemy);
            }

            Detection detection = enemy.GetComponentInChildren<Detection>();
            if (detection != null)
            {
                detection.player = player;
            }

            spawnedEnemies.Add(enemy);
            return true;
        }

        return false;
    }

    //Gets players current room using its position
    private Collider FindPlayerRoom()
    {
        if (player == null || !player.gameObject.activeInHierarchy) { return null; }
        return FindRoomContaining(player.position);
    }
    
    //Checks each room collider and returns the room containing the given position
    private Collider FindRoomContaining(Vector3 position)
    {
        foreach(Collider roomArea in roomAreas)
        {
            if (IsPointInside(roomArea,position)) { return roomArea; }
        }
        return null;
    }

    //Tries random positions in one room until it founds a point on the navmesh clear of obsticals
    private bool TryGetSpawnPoint(Collider roomArea, out Vector3 spawnPosition)
    {
        Bounds bounds = roomArea.bounds;
        float sampleRadius = Mathf.Max(0.1f, navMeshSampleRadius);

        for(int attempt = 0; attempt < spawnAttemptsPerRoom; attempt++)
        {
            Vector3 candidate = new Vector3(Random.Range(bounds.min.x, bounds.max.x),
            bounds.min.y + Mathf.Min(sampleRadius * 0.5f, bounds.size.y * 0.5f),
            Random.Range(bounds.min.z, bounds.max.z));

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, sampleRadius, navMeshAreaMask))
            { continue; }

            if (!IsPointInside(roomArea, hit.position) || IsSpawnBlocked(hit.position))
            { continue; }

            spawnPosition = hit.position;
            return true;
        }

        spawnPosition = Vector3.zero;
        return false;
    }

    //Checks whether a position is inside a room collider
    //private bool IsPointInside(Collider roomArea, Vector3 point)
    //{
    //    Vector3 closestPoint = roomArea.ClosestPoint(point);
    //    return (closestPoint - point).sqrMagnitude <= 0.01f;
    //}
    private bool IsPointInside(Collider roomArea, Vector3 point)
    {
        Bounds bounds = roomArea.bounds;

        return point.x >= bounds.min.x &&
               point.x <= bounds.max.x &&
               point.z >= bounds.min.z &&
               point.z <= bounds.max.z;
    }

    //checks whether an enemy sized capsul at the spawn point overlaps a collider on the selected blocking layer.
    private bool IsSpawnBlocked(Vector3 position)
    {
        float radius = Mathf.Max(0.05f, clearanceRadius);
        float height = Mathf.Max(clearanceHeight, radius * 2f);
        Vector3 lowerPoint = position + Vector3.up * radius;
        Vector3 upperPoint = position + Vector3.up * (height - radius);

        return Physics.CheckCapsule(lowerPoint, upperPoint, radius, blockingLayers, QueryTriggerInteraction.Ignore);
    }

    //Removes destroyes enemies from the list
    private void RemoveDestroyEnemies()
    {
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        { if (spawnedEnemies[i] == null) { spawnedEnemies.RemoveAt(i); } }
    }
}
