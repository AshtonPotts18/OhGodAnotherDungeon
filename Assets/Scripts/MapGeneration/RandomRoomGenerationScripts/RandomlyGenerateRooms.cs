
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
//Temp
using Unity.AI.Navigation;
using Unity.Properties;

public class RandomlyGenerateRooms : MonoBehaviour
{
    [Header("Room Managers")]
    [SerializeField] private GameObject roomManager;
    [SerializeField] private GameObject hallwayManager;
    [Header("Prefab Assignment")]
    [SerializeField] private GameObject startingRoom_Prefab;
    [SerializeField] private GameObject handRoom_Prefab;
    [SerializeField] private GameObject hallway_prefab;

    [SerializeField] private GameObject[] room_prefabs;

    [Header("Map Generation")]
    //Temp
    [SerializeField] private NavMeshSurface navMeshSurface;
    [SerializeField] private Vector2Int roomRange;
    [SerializeField] private int roomSpawnRadius;

    [SerializeField] private Int16 roomCount;
    [SerializeField] private int roomOffset = 5;
    [SerializeField] const int roomSpawnAttemptCap = 50;

    [SerializeField, UnityEngine.Range(0f, 100f)] float randomPathChance = 12.3f;
    private Dictionary<Vector2, GameObject> generatedRooms = new Dictionary<Vector2, GameObject>(); // starting room // hand room 1 // hand room 2
    private HashSet<Vector3> occupiedGridSpaces = new HashSet<Vector3>();
    public GameObject startingRoom;


    public List<GameObject> GenerateMap()
    {
        int resultOutput = 0;
        List<GameObject> outputRooms = new List<GameObject>();
        do
        {
            generatedRooms.Clear();
            resultOutput = GenerateRooms();
        } while (resultOutput != 0);


        //Temp
        if (navMeshSurface == null)
        {
            Debug.LogError("RandomlyGenerateRooms: NavMeshSurface is not assigned.", this);
        }
        navMeshSurface.BuildNavMesh();

        foreach (var room in generatedRooms.Values)
        {
            if (room.CompareTag("Hallway")) break; // all rooms are generated before the hallways so the first hallway tag is the end of the rooms
            outputRooms.Add(room);
            
        }
        
        return outputRooms;

    }


    public int GenerateRooms()
    {

        GenerateStartingRoom(generatedRooms);
        GenerateHandRooms(generatedRooms);
        GenerateLootRooms(generatedRooms); // generates all the room cooridiantes and stores them in generatedRooms

        List<Vector2> delaunayTrianglePoints = generatedRooms.Keys.ToList(); // generates a list of the coords(keys) to calculates triangles
        List<Triangle> triangles = DelaunayTriangleAlgorithm.DelaunayTriangulation(delaunayTrianglePoints); // room are generating out of range for valid triangles
        List<Edge> paths = new List<Edge>();

        TrianglesToPaths(triangles, paths);

        if (triangles.Count == 0) return -1; // error not enough triangles in map

        // At this point paths has all generated edges the delaunay triangles created. Still need to create edged for the remaining points

        SpawnRooms(generatedRooms);
        generatedRooms.TryGetValue(new Vector2Int(0, 0), out startingRoom); // get the starting room

        List<Edge> path_MST = PrimsMST(new Vector2(0, 0), paths);
        RandomlyDeletePaths(paths, in path_MST);
        GenerateHallways(paths);


        return 0;
    }

    private void TrianglesToPaths(List<Triangle> triangles, List<Edge> paths)
    {
        for (int i = 0; i < triangles.Count; i++) // adds all the triangle edges to the paths list
        {
            AddEdgesFromTriangles(triangles[i], paths);
        }
    }

    private void AddEdgesFromTriangles(Triangle triangle, List<Edge> paths)
    {
        Edge edge = new Edge();

        for (int edgeCounter = 0; edgeCounter < 3; edgeCounter++)
        {
            edge = new Edge(triangle[edgeCounter], triangle[((edgeCounter + 1) % 3)]);
        }

        if (!paths.Contains(edge))  // checks if this path has already been added before adding it
        {
            paths.Add(edge);
        }
    }

    private void RandomlyDeletePaths(List<Edge> paths, in List<Edge> path_MST)
    {
        for (int i = 0; i < paths.Count; i++)
        {
            Edge edge = paths[i];
            if (path_MST.Contains(edge)) continue;
            else if (RandomDeletePath())
            {
                paths.Remove(edge);
                i--;
            }
        }
    }

    private void GenerateHallways(List<Edge> _paths)
    {
        foreach (Edge edge in _paths)
        {
            WalkPath(edge);
        }
    }

    private void WalkPath(Edge _path)
    {

        Vector2 startPoint = _path.a;
        Vector2 endPoint = _path.b;

        Vector2 randomStartDirection = Direction2D.GetRandomDirection() * 5; // 5 is grid / size all rooms are divisable by
        //randomStartDirection the direction that gets added to the starting room. This creates the position of the first hall tile
        //currPosition adds the starting point to this direction to create the first hallway tile


        Vector2Int currentPosition2D = Vector2Int.RoundToInt(startPoint);

        //RoomInformation.ConnectRooms(generatedRooms[startPoint], generatedRooms[currPosition]); // connects the starting room with the first hallway segment

        Vector2Int nextPosition = Vector2Int.RoundToInt(startPoint + randomStartDirection); // the position of the first hallway tile

        while (currentPosition2D != endPoint)
        {
            Vector3 nextPosition3D = new Vector3(nextPosition.x, 0, nextPosition.y);

            if (!occupiedGridSpaces.Contains(nextPosition3D))
            {
                SpawnHallway(nextPosition3D, nextPosition);

            }
            RoomInformation.ConnectRooms(generatedRooms[currentPosition2D], generatedRooms[nextPosition]);
            currentPosition2D = nextPosition;
            nextPosition = RandHallwayWalk(currentPosition2D, endPoint); // function call instanates the hallway
        }
        //Debug.Log("HallwayEndingPoint" + endPoint);
    }

    private Vector2Int RandHallwayWalk(Vector2Int currentPosition2D, Vector2 targetPosition) // iterates currentPosition2D
    { // separate the hallway creation and the walk


        Vector3 nextPosition = WalkRandomDirection(currentPosition2D, targetPosition);
        currentPosition2D = new Vector2Int(Mathf.RoundToInt(nextPosition.x), Mathf.RoundToInt(nextPosition.z));

        return currentPosition2D; // new currentPosition

    }

    private void SpawnHallway(Vector3 currentPosition3D, Vector2Int currentPosition2D)
    {
        occupiedGridSpaces.Add(currentPosition3D);

        GameObject hallway = Instantiate(hallway_prefab, currentPosition3D, Quaternion.identity, hallwayManager.transform);
        generatedRooms[currentPosition2D] = hallway;
        hallway.name = "hallway" + currentPosition2D;
    }

    private Vector3 WalkRandomDirection(Vector2Int currentPosition, Vector2 targetPosition)
    {
        bool xAxisPossible = (currentPosition.x != targetPosition.x);
        bool yAxisPossible = (currentPosition.y != targetPosition.y);
        Vector2 distanceFromTarget = targetPosition - currentPosition;

        if (currentPosition.x != targetPosition.x && currentPosition.y != targetPosition.y) // both need to be moved
        {
            bool xMoveAxis = RandomAxis(xAxisPossible, yAxisPossible); // true == x // false == y

            if (xMoveAxis)
            {
                currentPosition.x = MoveAxisX(currentPosition, distanceFromTarget.x > 0);
            }
            else if (!xMoveAxis)
            {
                currentPosition.y = MoveAxisY(currentPosition, distanceFromTarget.y > 0);
            }
        }
        else if (currentPosition.y == targetPosition.y) // move x axis
        {
            currentPosition.x = MoveAxisX(currentPosition, distanceFromTarget.x > 0);
        }
        else if (currentPosition.x == targetPosition.x) // move y Axis
        {
            currentPosition.y = MoveAxisY(currentPosition, distanceFromTarget.y > 0);
        }
        return new Vector3(currentPosition.x, 0, currentPosition.y);
    }

    private int MoveAxisY(Vector2Int currentPosition, bool positiveDirection)
    {
        if (positiveDirection)
            return currentPosition.y += 5;
        else
            return currentPosition.y -= 5;
    }

    private int MoveAxisX(Vector2Int currentPosition, bool positiveDirection)
    {
        if (positiveDirection)
            return currentPosition.x += 5;
        else
            return currentPosition.x -= 5;
    }

    private bool RandomAxis(bool xAxisPossible, bool yAxisPossible)
    {
        bool output = false;
        if (xAxisPossible && yAxisPossible)
        {
            output = (Convert.ToBoolean(UnityEngine.Random.Range(0, 1)));
        }
        else if (!xAxisPossible && yAxisPossible)
        {
            output = true;
        }

        return output;
    }

    private void GenerateStartingRoom(Dictionary<Vector2, GameObject> _generatedRooms)
    {
        Vector2Int roomPosition = new Vector2Int(0, 0);
        _generatedRooms.Add(roomPosition, startingRoom_Prefab);
        occupiedGridSpaces.Add(new Vector3(roomPosition.x, 0, roomPosition.y));
    }
    private void GenerateHandRooms(Dictionary<Vector2, GameObject> _generatedRooms)
    {
        Vector2 roomPosition;

        for (int i = 0; i < 2; i++)
        {

            roomPosition = FindValidHandRoomPosition(_generatedRooms);
            _generatedRooms.Add(roomPosition, handRoom_Prefab);
            occupiedGridSpaces.Add(new Vector3(roomPosition.x, 0, roomPosition.y));
        }
    }
    private Vector2 FindValidHandRoomPosition(Dictionary<Vector2, GameObject> _generatedRooms)
    {

        Int16 AttemptLog = 0;
        Vector2 roomPosition;
        bool validRoomPosition = false;

        do
        {
            roomPosition = GetRandomRoomPosition();
            Vector3 roomScale = handRoom_Prefab.transform.localScale;

            validRoomPosition = CheckBounds(new Bounds(new Vector3(roomPosition.x, 0, roomPosition.y), roomScale), _generatedRooms);

            //Debug.Log("Loading Room " + AttemptLog);
            AttemptLog++;
            if (AttemptLog == roomSpawnAttemptCap)
            {
                Debug.Log("Hand room too many spawn attempts");
                break;
            }

        } while (!validRoomPosition);

        return roomPosition;

    }

    private void GenerateLootRooms(Dictionary<Vector2, GameObject> _generatedRooms)
    {
        for (int i = 0; i < roomCount; i++)
        {
            GenerateRoom(_generatedRooms);
            //Debug.Log("All rooms generated");
        }
    }

    private void GenerateRoom(Dictionary<Vector2, GameObject> _generatedRooms)
    {

        int roomPrefabIndex = GetRandomPrefabPosition();
        Int16 AttemptLog = 0;
        Vector2 roomPosition;
        bool validRoomPosition = false;

        do
        {
            roomPosition = GetRandomRoomPosition();
            Vector3 roomScale = room_prefabs[roomPrefabIndex].transform.localScale;

            validRoomPosition = CheckBounds(new Bounds(new Vector3(roomPosition.x, 0, roomPosition.y), roomScale), _generatedRooms);


            //Debug.Log("Loading Room " + AttemptLog);
            AttemptLog++;
            if (AttemptLog == roomSpawnAttemptCap)
            {
                Debug.Log("Random room too many spawn attempts");
                break;
            }

        } while (!validRoomPosition);

        _generatedRooms.Add(roomPosition, room_prefabs[roomPrefabIndex]); // add this room and prefab to generatedRooms
        occupiedGridSpaces.Add(new Vector3(roomPosition.x, 0, roomPosition.y));

    }

    private void SpawnRooms(Dictionary<Vector2, GameObject> _generatedRooms)
    {
        List<Vector2> keys = _generatedRooms.Keys.ToList();
        for (int keyPosition = 0; keyPosition < keys.Count; keyPosition++)
        {
            Vector3 roomPosition = new Vector3(keys[keyPosition].x, 0, keys[keyPosition].y);
            _generatedRooms[keys[keyPosition]] = Instantiate(_generatedRooms[keys[keyPosition]], roomPosition, Quaternion.identity, roomManager.transform);
        }
    }

    private Vector2 GetRandomRoomPosition()
    {
        Vector2 randomLocation = (UnityEngine.Random.insideUnitCircle * roomSpawnRadius);

        int snappedX = Mathf.RoundToInt(randomLocation.x / roomOffset) * roomOffset;
        int snappedy = Mathf.RoundToInt(randomLocation.y / roomOffset) * roomOffset;

        Vector2 output = new Vector2(snappedX, snappedy);
        //Debug.Log("RandomPoint Generated");


        return output;
    }

    private int GetRandomPrefabPosition()
    {
        return UnityEngine.Random.Range(0, room_prefabs.Length);
    }

    private bool CheckBounds(Bounds room1, Dictionary<Vector2, GameObject> _generatedRooms)
    {
        bool intersects = false;
        foreach (KeyValuePair<Vector2, GameObject> placedRoom in _generatedRooms)
        {
            Bounds placedRoomBounds = new Bounds(new Vector3(placedRoom.Key.x, 0, placedRoom.Key.y), placedRoom.Value.transform.localScale);
            if (room1.Intersects(placedRoomBounds))
            {
                intersects = true;
            }
        }

        return !intersects;

    }

    private List<Edge> PrimsMST(Vector2 _startingRoom, List<Edge> _paths)
    {
        List<Edge> outMST = new List<Edge>();
        HashSet<Vector2> visitedRooms = new HashSet<Vector2>();

        visitedRooms.Add(_startingRoom);
        while (outMST.Count < roomCount)
        {
            Edge? bestPath = null; // the ? allows for variables to be null because c# doesn't like it otherwise

            foreach (Edge path in _paths)
            {
                bool roomAVisited = visitedRooms.Contains(path.a);
                bool roomBVisited = visitedRooms.Contains(path.b);

                if (roomAVisited == roomBVisited) continue; // this way the tree only branches from rooms already in the tree

                if (bestPath == null || bestPath.Value.weight > path.weight) bestPath = path; // if this path is a shorter distance it is the best path



            }
            Edge selectedPath = bestPath.Value;
            outMST.Add(selectedPath);

            visitedRooms.Add(selectedPath.a);
            visitedRooms.Add(selectedPath.b);
        }
        return outMST;
    }

    private bool RandomDeletePath()
    {
        bool output = false;
        if (UnityEngine.Random.Range(0, 100) <= randomPathChance)
        {
            output = true;
        }
        return output;
    }

}

public class Direction2D
{
    public static List<Vector2Int> directions = new List<Vector2Int>()
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };
    public static Vector2Int GetRandomDirection()
    {
        return directions[UnityEngine.Random.Range(0, directions.Count)];
    }
}


