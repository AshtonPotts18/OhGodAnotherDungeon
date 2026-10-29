
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
//Temp
using Unity.AI.Navigation;
using Unity.Properties;
using Newtonsoft.Json.Bson;

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

    [SerializeField, UnityEngine.Range(0f, 100f)] float randomPathChance = 100f;
    private Dictionary<Vector2, GameObject> generatedRooms = new Dictionary<Vector2, GameObject>(); // starting room // hand room 1 // hand room 2
    private HashSet<Vector3> occupiedGridSpaces = new HashSet<Vector3>();
    public GameObject startingRoom;


    //Start of new Fields
    DungeonGrid dungeonGrid = new DungeonGrid();
    Dictionary<Vector2Int, DungeonRoom> dungeonRooms = new Dictionary<Vector2Int, DungeonRoom>();


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
            //return;
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

        List<Vector2> delaunayTrianglePoints = ConvertListToVector2Int(dungeonRooms.Keys.ToList()); // generates a list of the coords(keys) to calculates triangles
        List<Triangle> triangles = DelaunayTriangleAlgorithm.DelaunayTriangulation(delaunayTrianglePoints); // room are generating out of range for valid triangles
        List<Edge> paths = new List<Edge>();

        TrianglesToPaths(triangles, paths); //adds triangles to paths

        if (triangles.Count == 0)
        {
            dungeonGrid = new DungeonGrid();
            dungeonRooms.Clear();
        
            return -1; // error not enough triangles in map
        }

        // At this point paths has all generated edges the delaunay triangles created. Still need to create edged for the remaining points

        GenerateHallways(paths);

        foreach (KeyValuePair<Vector2Int, DungeonRoom> room in dungeonRooms)
        {
            room.Value.InstantiateRoom(transform);
        }
        





        //SpawnRooms(generatedRooms);
        //generatedRooms.TryGetValue(new Vector2Int(0, 0), out startingRoom); // get the starting room

        //List<Edge> path_MST = PrimsMST(new Vector2(0, 0), paths);
        //RandomlyDeletePaths(paths, in path_MST);
        //SpawnHallways(paths);


        return 0;
    }

    private List<Vector2> ConvertListToVector2Int(List<Vector2Int> vector2Ints)
    {
        List<Vector2> output = new List<Vector2>();
        for (int i = 0; i < vector2Ints.Count; i++)
        {
            output.Add(vector2Ints[i]);
        }
        return output;
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

    private void SpawnHallways(List<Edge> _paths)
    {
        foreach (Edge edge in _paths)
        {
            WalkPath(edge);
        }
    }

    private void GenerateHallways(List<Edge> _paths)
    {
        RoomInformation hallwayInformation = hallway_prefab.GetComponent<RoomInformation>();
        DungeonRoom hallway = new DungeonRoom(new Vector2Int(0, 0), hallwayInformation.floorSize, hallway_prefab);

        Queue<Vector2Int> walkedPath = new Queue<Vector2Int>();


        foreach (Edge edge in _paths)
        {
            Vector2Int startingPoint = dungeonRooms[new Vector2Int(Mathf.RoundToInt(edge.a.x), Mathf.RoundToInt(edge.a.y))].RandomDoor();
            Vector2Int targetPoint = dungeonRooms[new Vector2Int(Mathf.RoundToInt(edge.b.x ), Mathf.RoundToInt(edge.b.y))].ClosestDoor(startingPoint);

            Vector2Int currentPosition = startingPoint;


            do
            {
                hallway.anchor = currentPosition;
                if (!dungeonGrid.CanPlaceRoom(hallway.anchor, hallway.size))
                {

                }
                if (dungeonGrid.CanPlaceRoom(hallway.anchor, hallway.size)) // prevents hallway from being placed in the same place as a room but not a hallway
                {
                    dungeonGrid.ReserveHallway(hallway.anchor);
                    dungeonRooms.Add(hallway.anchor, new DungeonRoom(hallway));
                }

                walkedPath.Enqueue(currentPosition);

                currentPosition = DungeonGridWalkRandomDirection(currentPosition, targetPoint);

            } while (currentPosition != targetPoint);

            // connect rooms here

        }
    }
    private void WalkPath(Edge _path)
    {

        Vector2 startPoint = _path.a;
        Vector2 endPoint = _path.b;
        //Debug.Log("HallwayStartingPoint" + startPoint);

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

    private Vector2Int DungeonGridWalkRandomDirection(Vector2Int currentPosition, Vector2Int targetPosition)
    {
        bool xAxisPossible = (currentPosition.x != targetPosition.x);
        bool yAxisPossible = (currentPosition.y != targetPosition.y);
        Vector2 distanceFromTarget = targetPosition - currentPosition;

        if (currentPosition.y == targetPosition.y) // move x axis // if y not possible
        {
            currentPosition.x = DungeonGridMoveAxisX(currentPosition, distanceFromTarget.x > 0);
        }
        else if (currentPosition.x == targetPosition.x) // move y Axis // if x not possible
        {
            currentPosition.y = DungeonGridMoveAxisY(currentPosition, distanceFromTarget.y > 0);
        }
        else // both need to be moved // 
        {
            bool xMoveAxis = RandomAxis(xAxisPossible, yAxisPossible);

            if (xMoveAxis)
            {
                currentPosition.x = DungeonGridMoveAxisX(currentPosition, distanceFromTarget.x > 0);
            }
            else if (!xMoveAxis)
            {
                currentPosition.y = DungeonGridMoveAxisY(currentPosition, distanceFromTarget.y > 0);
            }
        }

        return currentPosition;
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
    private int DungeonGridMoveAxisY(Vector2Int currentPosition, bool positiveDirection)
    {
        if (positiveDirection)
            return currentPosition.y += 1;
        else
            return currentPosition.y -= 1;
    }

    private int DungeonGridMoveAxisX(Vector2Int currentPosition, bool positiveDirection)
    {
        if (positiveDirection)
            return currentPosition.x += 1;
        else                            
            return currentPosition.x -= 1;
    }

    private bool RandomAxis(bool xAxisPossible, bool yAxisPossible) // true == X
    {
        bool output = true;
        if (!xAxisPossible)
        {
            output = false;
        }
        else if (!yAxisPossible)
        {
            output = true;
        }
        else if (xAxisPossible && yAxisPossible)
        {
            output = (Convert.ToBoolean(UnityEngine.Random.Range(0, 1)));
        }

        return output;
    }

    private void GenerateStartingRoom(Dictionary<Vector2, GameObject> _generatedRooms)
    {
        Vector2Int startingRoomPos = new Vector2Int(0, 0);

        dungeonGrid.ReserveRoom(startingRoomPos, new Vector2Int(1, 1)); // adjust to be dynamic later

        _generatedRooms.Add(startingRoomPos, startingRoom_Prefab);
        occupiedGridSpaces.Add(new Vector3(startingRoomPos.x, 0, startingRoomPos.y));

        //DungeonGrid
        //DungeonRoom startingRoom = new DungeonRoom(startingRoomPos, new Vector2Int (1,1), startingRoom_Prefab);
        dungeonRooms.Add(startingRoomPos, new DungeonRoom(startingRoomPos, new Vector2Int(1, 1), startingRoom_Prefab));
    }
    private void GenerateHandRooms(Dictionary<Vector2, GameObject> _generatedRooms)
    {
        Vector2 roomPosition;
        RoomInformation handRoomInfo = handRoom_Prefab.GetComponent<RoomInformation>();
        DungeonRoom handRoom = new DungeonRoom(new Vector2Int(), handRoomInfo.floorSize, handRoom_Prefab);

        for (int i = 0; i < 2; i++)
        {

            roomPosition = FindValidHandRoomPosition(_generatedRooms);
            Vector2Int randomGridPosition = dungeonGrid.GetValidRandomRoomPosition(handRoomInfo.floorSize, roomSpawnRadius);

            _generatedRooms.Add(roomPosition, handRoom_Prefab);
            occupiedGridSpaces.Add(new Vector3(roomPosition.x, 0, roomPosition.y));


            dungeonGrid.ReserveRoom(randomGridPosition, handRoomInfo.floorSize);
            dungeonRooms.Add(randomGridPosition, new DungeonRoom(randomGridPosition, handRoom.size, handRoom.prefab));
        }
    }
    private Vector2 FindValidHandRoomPosition(Dictionary<Vector2, GameObject> _generatedRooms)
    {

        Int16 AttemptLog = 0;
        Vector2 roomPosition;
        bool validRoomPosition = false;
        //RoomInformation handRoomInfo = handRoom_Prefab.GetComponent<RoomInformation>();
        //DungeonRoom handRoom = new DungeonRoom(new Vector2Int(), handRoomInfo.floorSize, handRoom_Prefab);
        

        do
        {
            roomPosition = GetRandomRoomPosition();
            //Vector2Int randomRoomPos_DungeonGrid = dungeonGrid.GetValidRandomRoomPosition(handRoom.size, roomSpawnRadius);
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

        RoomInformation randomRoomInfo = room_prefabs[roomPrefabIndex].GetComponent<RoomInformation>();

        Int16 AttemptLog = 0;
        Vector2 roomPosition;
        bool validRoomPosition = false;
        DungeonRoom room = new DungeonRoom(new Vector2Int(), randomRoomInfo.floorSize, room_prefabs[roomPrefabIndex]);

        do
        {

            roomPosition = GetRandomRoomPosition();
            Vector2Int randGridPos = dungeonGrid.GetValidRandomRoomPosition(randomRoomInfo.floorSize, roomSpawnRadius);
            Vector3 roomScale = room_prefabs[roomPrefabIndex].transform.localScale;

            validRoomPosition = CheckBounds(new Bounds(new Vector3(roomPosition.x, 0, roomPosition.y), roomScale), _generatedRooms);

            room.anchor = Vector2Int.RoundToInt(roomPosition);
            
            if(dungeonGrid.CanPlaceRoom(room.anchor, room.size))
            {
                //room.AddRoomDoors();
                dungeonGrid.ReserveRoom(room.anchor, room.size);
                dungeonRooms.Add(room.anchor,new DungeonRoom(room));

            }
            

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

public class DungeonGrid
{
    HashSet<Vector2Int> roomCells = new HashSet<Vector2Int>(); // cells containing rooms
    HashSet<Vector2Int> hallwayCells = new HashSet<Vector2Int>();
    int cellSize = 5;

    bool IsRoomOccupied(Vector2Int _cell)
    {
        return roomCells.Contains(_cell);
    }
    bool IsHallwayOccupied(Vector2Int _cell)
    {
        return hallwayCells.Contains(_cell);
    }
    bool IsOccupied(Vector2Int _cell)
    {
        return roomCells.Contains(_cell) || hallwayCells.Contains(_cell);
    }

    public bool CanPlaceRoom(Vector2Int _anchor, Vector2Int _size)
    {
        for (int row = 0; row < _size.x; row++) // iterates through every space the room would occupy to see if any grid is already occupied
        {
            for (int column = 0; column < _size.y; column++)
            {
                Vector2Int cell = _anchor + new Vector2Int(row, column);

                if (IsOccupied(cell))
                    return false;
            }
        }
        return true;
    }

    public void ReserveRoom(Vector2Int _anchor, Vector2Int _size)
    {
        if(!CanPlaceRoom(_anchor, _size))
        {
            Debug.Log("Room Overlap Found");
            return;
        }

        for (int row = 0; row < _size.x; row++)
        {
            for (int column = 0; column < _size.y; column++)
            {
                roomCells.Add(_anchor + new Vector2Int(row, column));
            }
        }

    }

    bool CanPlaceHallway(Vector2Int _cell)
    {
        return !roomCells.Contains(_cell);
    }

    public void ReserveHallway(Vector2Int _cell)
    {
        if (CanPlaceHallway(_cell))
        {
            hallwayCells.Add(_cell);
        }
    }

    public Vector2Int GetValidRandomRoomPosition(Vector2Int _size, int spawnRadius)
    {
        Vector2Int randomPosition;
        int attemptCounter = 0;
        do
        {
            Vector2 roomPos = UnityEngine.Random.insideUnitCircle * spawnRadius;
            randomPosition = Vector2Int.RoundToInt(roomPos);

            attemptCounter++;
        } while (!CanPlaceRoom(randomPosition, _size) && attemptCounter < 1000);

        return randomPosition;

    }

    public static Vector3 GridToWorldSpace(Vector2 _roomCenter)
    {
        return new Vector3(_roomCenter.x, 0, _roomCenter.y) * 5;
    }

}

public class DungeonRoom
{
    public Vector2Int anchor; // this is the inital point of the room (3,4). a long room occupy (3,4) & (4,4) for example
    public Vector2Int size; // long room would be (2,1)
    public GameObject prefab = null;
    public GameObject roomInstance = null;
    public Vector2 CenterCell => anchor + new Vector2((size.x - 1) * 0.5f, (size.y - 1) * 0.5f);
     
    public List<Vector2Int> doorPositions = new List<Vector2Int>();
    public DungeonRoom()
    {

    }

    public DungeonRoom(Vector2Int _anchor,  Vector2Int _size,  GameObject _prefab = null)
    {
        anchor = _anchor;
        size = _size; 
        prefab = _prefab;
        if(anchor  != null|| size != null)
            AddRoomDoors();
    }
    public DungeonRoom(DungeonRoom _other)
    {
        anchor = _other.anchor;
        size = _other.size;
        prefab = _other.prefab;
        //doorPositions = _other.doorPositions;
        if (anchor != null || size != null)
            AddRoomDoors();
    }

    public void InstantiateRoom(Transform parent = null)
    {
        roomInstance = GameObject.Instantiate(prefab, DungeonGrid.GridToWorldSpace(CenterCell), Quaternion.identity, parent);
    }

    Vector3 GridToCellPosition()
    {
        return new Vector3(CenterCell.x, 0, CenterCell.y);
    }

    
    public void AddRoomDoors()
    {
        //Vector2Int doorLocation = new Vector2Int(0, anchor.y + (size.y / 2));
        doorPositions.Add(Vector2Int.FloorToInt(new Vector2(CenterCell.x, CenterCell.y + (size.y / 2)))); // front
        doorPositions.Add(Vector2Int.FloorToInt(new Vector2(CenterCell.x, CenterCell.y - (size.y / 2)))); // back
        doorPositions.Add(Vector2Int.FloorToInt(new Vector2(CenterCell.x - (size.x / 2), CenterCell.y))); // left
        doorPositions.Add(Vector2Int.FloorToInt(new Vector2(CenterCell.x + (size.x / 2), CenterCell.y))); // right
    }

    public Vector2Int ClosestDoor(Vector2Int _targetPosition)
    {
        Vector2Int closestDoor = new Vector2Int();
        int bestDistance = int.MaxValue;

        foreach (Vector2Int doorPosition in doorPositions)
        {
            int distance = Mathf.Abs(_targetPosition.x - doorPosition.x + _targetPosition.y - doorPosition.y);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                closestDoor = doorPosition;
            }
        }
        return closestDoor;
    }
    public Vector2Int RandomDoor()
    {
        return doorPositions[UnityEngine.Random.Range(0, doorPositions.Count)];
    }
}
