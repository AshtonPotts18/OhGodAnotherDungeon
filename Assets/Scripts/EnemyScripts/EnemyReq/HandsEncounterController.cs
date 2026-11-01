using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


//
public class HandEncounterController : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Transform roomsRoot;
    [SerializeField] private Transform player;
    [SerializeField] private GameObject snapHandPrefab;
    [SerializeField] private GameObject jazzHandPrefab;
    [SerializeField] private string roomBoundsObjectName = "RoomBounds";

    [Header("Encounter Timing")]
    [SerializeField, Min(0f)] private float delayAfterPlayerAppears = 60f;

    [Header("Spawn Search")]
    [SerializeField, Min(0.1f)] private float navMeshSampleRadius = 3f;
    [SerializeField, Min(1)] private int attemptsPerRoom = 20;
    [SerializeField, Min(1)] private int jazzSpawnTrials = 10;

    [Header("Paired Hand Kill")]
    [SerializeField, Min(0f)] private float killDistance = 1.5f;

    private Collider snapHandRoom;
    private Transform snapHandTransform;
    private Transform jazzHandTransform;
    private bool hasKilledPlayer;

    private IEnumerator Start()
    {
        if (roomsRoot == null)
        {
            Debug.LogError(
                $"{name}: assign the outer RoomManager to Rooms Root.",
                this);
            yield break;
        }

        if (snapHandPrefab == null || jazzHandPrefab == null)
        {
            Debug.LogError($"{name}: assign both hand prefabs.", this);
            yield break;
        }

        while (player == null)
        {
            FindPlayer();
            yield return new WaitForSeconds(0.25f);
        }

        float handsActivateAt = Time.time + delayAfterPlayerAppears;

        while (Time.time < handsActivateAt)
        {
            if (player == null)
                FindPlayer();

            yield return null;
        }

        List<Collider> roomAreas = GetRoomAreas();

        while (player == null || roomAreas.Count < 2)
        {
            FindPlayer();
            roomAreas = GetRoomAreas();
            yield return new WaitForSeconds(0.25f);
        }

        Collider playerRoom =
            FindRoomContainingOrNearest(player.position, roomAreas);

        if (!TryFindSpawnPoint(
                roomAreas, playerRoom,
                null, out snapHandRoom,
                out Vector3 snapPosition))
        {
            Debug.LogError(
                $"{name}: couldn't find a Snap Hand spawn point outside the player's room.",
                this);
            yield break;
        }

        GameObject snapHand = Instantiate(
            snapHandPrefab,
            snapPosition,
            snapHandPrefab.transform.rotation);

        SetEnemyTag(snapHand);

        SnapHandAI snapAI =
            snapHand.GetComponentInChildren<SnapHandAI>();

        if (snapAI == null)
        {
            Debug.LogError($"{snapHand.name}: SnapHandAI is missing.", snapHand);
            Destroy(snapHand);
            yield break;
        }

        if (!InitializeHand(snapHand, roomAreas.ToArray()))
        {
            Destroy(snapHand);
            yield break;
        }

        snapHandTransform = snapHand.transform;
        Debug.Log($"{snapHand.name}: spawned and is patrolling.", snapHand);

        // Spawn Jazz during this same encounter start.
        yield return StartCoroutine(SpawnJazzHand());
    }


    private void FindPlayer()
    {
        if (player != null) { return; }

        try
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            { player = playerObject.transform; }
        }
        catch (UnityException)
        {
            Debug.LogError(
                "The 'Player' tag does not exist. Add it in Tags and Layers.",
                this);
        }
    }

    //snap delay needs to get replaced
    private IEnumerator SpawnJazzHand()
    {
        FindPlayer();

        if (player == null)
        {
            Debug.LogError(
                $"{name}: player was missing when the Jazz Hand spawned.",
                this);
            yield break;
        }

        List<Collider> roomAreas = GetRoomAreas();

        Collider playerRoom = FindRoomContainingOrNearest(player.position, roomAreas);

        for (int trial = 0; trial < jazzSpawnTrials; trial++)
        {
            bool foundSpawn = TryFindSpawnPoint(
                roomAreas, playerRoom,
                snapHandRoom, out _,
                out Vector3 jazzPosition);

            if (!foundSpawn)
            {
                foundSpawn = TryFindSpawnPoint(
                    roomAreas, playerRoom,
                    null, out _,
                    out jazzPosition);
            }

            if (!foundSpawn)
                break;

            GameObject jazzHand = Instantiate(
                jazzHandPrefab,
                jazzPosition,
                jazzHandPrefab.transform.rotation);

            SetEnemyTag(jazzHand);

            JazzHandAI jazzAI =
                jazzHand.GetComponentInChildren<JazzHandAI>();

            if (jazzAI == null)
            {
                Debug.LogError(
                    $"{jazzHand.name}: JazzHandAI is missing.",
                    jazzHand);
                Destroy(jazzHand);
                yield break;
            }

            if (!InitializeHand(jazzHand, roomAreas.ToArray()))
            {
                Destroy(jazzHand);
                yield break;
            }

            if (jazzAI.IsReady)
            {
                jazzHandTransform = jazzHand.transform;

                Debug.Log(
                    $"{jazzHand.name}: spawned alongside Snap and is pursuing the player.",
                    jazzHand);
                yield break;
            }

            Destroy(jazzHand);
            yield return null;
        }

        Debug.LogError(
            $"{name}: couldn't find a reachable Jazz Hand spawn point. " +
            "Check that your NavMesh connects rooms and hallways.",
            this);
    }

    // This follows the same initialization order as EnemySpawner.
    private bool InitializeHand(GameObject hand, Collider[] areas)
    {
        Enemy_Movement movement =
            hand.GetComponentInChildren<Enemy_Movement>();

        Detection detection =
            hand.GetComponentInChildren<Detection>();

        EnemyBehavior behavior =
            hand.GetComponentInChildren<EnemyBehavior>();

        if (movement == null || detection == null || behavior == null)
        {
            Debug.LogError(
                $"{hand.name} needs Enemy_Movement, Detection, and an EnemyBehavior-derived AI.",
                hand);
            return false;
        }

        movement.Initialize(areas, player);
        detection.Initialize(player, movement);
        behavior.Initialize(player, detection, movement);

        return true;
    }

    private List<Collider> GetRoomAreas()
    {
        List<Collider> areas = new List<Collider>();

        if (roomsRoot == null)
            return areas;

        Collider[] colliders =
            roomsRoot.GetComponentsInChildren<Collider>(true);

        foreach (Collider area in colliders)
        {
            if (area != null &&
                area.enabled &&
                area.isTrigger &&
                area.gameObject.activeInHierarchy &&
                area.gameObject.name == roomBoundsObjectName)
            {
                areas.Add(area);
            }
        }

        return areas;
    }

    private Collider FindRoomContainingOrNearest(
        Vector3 position,
        List<Collider> roomAreas)
    {
        Collider nearestArea = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider area in roomAreas)
        {
            Bounds bounds = area.bounds;

            bool insideXZ =
                position.x >= bounds.min.x &&
                position.x <= bounds.max.x &&
                position.z >= bounds.min.z &&
                position.z <= bounds.max.z;

            if (insideXZ)
                return area;

            Vector3 closestPoint = bounds.ClosestPoint(position);
            closestPoint.y = position.y;

            float distance = (closestPoint - position).sqrMagnitude;

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestArea = area;
            }
        }

        // If the player is in a hallway without a RoomBounds collider,
        // use the nearest room to avoid failing the spawn sequence.
        return nearestArea;
    }

    private bool TryFindSpawnPoint(
        List<Collider> roomAreas,
        Collider excludedRoomA,
        Collider excludedRoomB,
        out Collider selectedRoom,
        out Vector3 spawnPosition)
    {
        selectedRoom = null;
        spawnPosition = Vector3.zero;

        List<Collider> candidates = new List<Collider>();

        foreach (Collider area in roomAreas)
        {
            if (area != excludedRoomA && area != excludedRoomB)
                candidates.Add(area);
        }

        // Shuffle room order so the hand's starting room varies.
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            Collider temporary = candidates[i];
            candidates[i] = candidates[swapIndex];
            candidates[swapIndex] = temporary;
        }

        foreach (Collider area in candidates)
        {
            Bounds bounds = area.bounds;

            for (int attempt = 0; attempt < attemptsPerRoom; attempt++)
            {
                Vector3 candidate = new Vector3(
                    Random.Range(bounds.min.x, bounds.max.x),
                    bounds.center.y,
                    Random.Range(bounds.min.z, bounds.max.z));

                if (!NavMesh.SamplePosition(
                        candidate,
                        out NavMeshHit hit,
                        navMeshSampleRadius,
                        NavMesh.AllAreas))
                {
                    continue;
                }

                // Keep the sampled point inside the selected room's bounds.
                if (hit.position.x < bounds.min.x ||
                    hit.position.x > bounds.max.x ||
                    hit.position.z < bounds.min.z ||
                    hit.position.z > bounds.max.z)
                {
                    continue;
                }

                selectedRoom = area;
                spawnPosition = hit.position;
                return true;
            }
        }

        return false;
    }

    private void SetEnemyTag(GameObject hand)
    {
        try
        {
            hand.tag = "Enemy";
        }
        catch (UnityException)
        {
            Debug.LogError(
                "The 'Enemy' tag does not exist. Add it in Tags and Layers.",
                hand);
        }
    }

    private void Update()
    {
        if (hasKilledPlayer || player == null ||
        snapHandTransform == null || jazzHandTransform == null)
        {
            return;
        }

        float snapDistance = Vector3.Distance(snapHandTransform.position, player.position);
        float jazzDistance = Vector3.Distance(jazzHandTransform.position, player.position);

        if(snapDistance > killDistance || jazzDistance > killDistance) { return; }

        hasKilledPlayer = true;

        Debug.Log($"Both hands reached player; destroying {player.name}.", jazzHandTransform);

        Destroy(player.gameObject);
    }
}