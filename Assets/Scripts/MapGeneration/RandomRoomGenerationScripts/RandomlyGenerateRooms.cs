
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
//Temp
using Unity.AI.Navigation;
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
    [SerializeField] public int roomSpawnRadius;

    [SerializeField] public Int16 roomCount;
    [SerializeField] const int roomSpawnAttemptCap = 50;

    [SerializeField, UnityEngine.Range(0f, 100f)] float randomPathChance = 100f;
    private Dictionary<Vector2, GameObject> generatedRooms = new Dictionary<Vector2, GameObject>(); // starting room // hand room 1 // hand room 2
    private HashSet<Vector3> occupiedGridSpaces = new HashSet<Vector3>();
    public GameObject startingRoom;


    //Start of new Fields
    DungeonGrid dungeonGrid = new DungeonGrid();
    Dictionary<Vector2Int, DungeonRoom> dungeonRooms = new Dictionary<Vector2Int, DungeonRoom>();
    List<GameObject > roomList = new List<GameObject>();


    public List<GameObject> GenerateMap()
    {
        int resultOutput = 0;
        do
        {
            ClearMap();
            resultOutput = GenerateRooms();
        } while (resultOutput != 0);


        //Temp
        if (navMeshSurface == null)
        {
            Debug.LogError("RandomlyGenerateRooms: NavMeshSurface is not assigned.", this);
            //return;
        }


        navMeshSurface.BuildNavMesh();
        
        return roomList;

    }

    public void ClearMap()
    {
        dungeonGrid.Clear();
        dungeonRooms.Clear();
    }

    public int GenerateRooms()
    {

        GenerateStartingRoom(generatedRooms);
        GenerateHandRooms(generatedRooms);
        GenerateLootRooms(generatedRooms); // generates all the room cooridiantes and stores them in generatedRooms

        List<Vector2> delaunayTrianglePoints = ConvertListToVector2Int(dungeonRooms.Keys.ToList()); // generates a list of the coords(keys) to calculates triangles
        List<Triangle> triangles = DelaunayTriangleAlgorithm.DelaunayTriangulation(delaunayTrianglePoints); // room are generating out of range for valid triangles
        List<Edge> paths = new List<Edge>();

        if (triangles.Count == 0)
        {
            dungeonGrid = new DungeonGrid();
            dungeonRooms.Clear();
        
            return -1; // error not enough triangles in map
        }        

        TrianglesToPaths(triangles, paths); //adds triangles to paths

        SpawnDungeonRooms(dungeonRooms);

        GenerateHallways(paths);
        CheckOpenDoors();
        
        startingRoom = dungeonRooms[new Vector2Int(0, 0)].roomInstance;

        return 0;
    }

    private void CheckOpenDoors()
    {

        List<DungeonRoom> rooms = dungeonGrid.ListOfRooms();

        foreach (DungeonRoom room in rooms)
        {
            if (room.roomInstance == null) room.InstantiateRoom();

            RoomInformation roomInformation = room.roomInstance.GetComponent<RoomInformation>();
            bool[] openDoors = roomInformation.openDoors;

           for (int i = 0; i < openDoors.Length - 1; i++) // the index lead to the same doors. 
           {
                if (openDoors[i]) // if the door found at this index is open, check if there is a room in the door location, if not add a hallway and connect it
                {
                    if (!dungeonGrid.IsOccupied(room.doorPositions[i]))
                    {
                        Debug.Log("Filling Room At: " +  room.doorPositions[i]);
                        DungeonRoom fillerHallway = new DungeonRoom(room.doorPositions[i], new Vector2Int(1,1), hallway_prefab);
                        fillerHallway.InstantiateRoom(hallwayManager.transform);

                        dungeonGrid.ReserveHallway(room.doorPositions[i], fillerHallway);
                        RoomInformation.ConnectRooms(room.roomInstance.GetComponent<RoomInformation>(), fillerHallway.roomInstance.GetComponent<RoomInformation>(), room.DoorDirection(room.doorPositions[i]));
                        
                    }
                }




           }
        }
    }

    private void SpawnDungeonRooms(Dictionary<Vector2Int, DungeonRoom> dungeonRooms)
    {
        //dungeonGrid.InstantiateAllRooms(roomManager.transform);
        List<DungeonRoom> rooms = dungeonRooms.Values.ToList();
        
        foreach (DungeonRoom room in rooms)
        {
            room.InstantiateRoom(roomManager.transform);
            roomList.Add(room.roomInstance);
        }
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

    private void GenerateHallways(List<Edge> _paths)
    {
        RoomInformation hallwayInformation = hallway_prefab.GetComponent<RoomInformation>();
        //DungeonRoom hallway = new DungeonRoom(new Vector2Int(0, 0), hallwayInformation.floorSize, hallway_prefab);



        foreach (Edge edge in _paths)
        {
            Queue<Vector2Int?> walkedPath = new Queue<Vector2Int?>();

            //Vector2Int startingPoint = dungeonRooms[new Vector2Int(Mathf.RoundToInt(edge.a.x), Mathf.RoundToInt(edge.a.y))].ClosestDoor(Vector2Int.RoundToInt(edge.b));
            //Vector2Int targetPoint = dungeonRooms[new Vector2Int(Mathf.RoundToInt(edge.b.x ), Mathf.RoundToInt(edge.b.y))].ClosestDoor(startingPoint);

            //Vector2Int currentPosition = startingPoint;


            //ReservePath(startingPoint, targetPoint, walkedPath);
            DungeonRoom startingRoom = dungeonRooms[new Vector2Int(Mathf.RoundToInt(edge.a.x), Mathf.RoundToInt(edge.a.y))];
            DungeonRoom targetRoom = dungeonRooms[new Vector2Int(Mathf.RoundToInt(edge.b.x), Mathf.RoundToInt(edge.b.y))];

            Path(startingRoom, targetRoom, hallwayInformation, walkedPath);
            //walkedPath.Enqueue(targetPoint);
            



            // connect rooms here

            HashSet<GameObject> visitedRooms = new HashSet<GameObject>();

            RoomInformation.ConnectRooms(startingRoom.roomInstance, dungeonGrid.LivingRoomAtCell(walkedPath.Peek().Value));

            while (walkedPath.Count > 0)
            {
                Vector2Int firstRoom_delAfterLoop = walkedPath.Dequeue().Value;
                walkedPath.TryPeek(out Vector2Int? secondRoom);

                if (secondRoom == null) break;
                GameObject firstRoomObejct = dungeonGrid.LivingRoomAtCell(firstRoom_delAfterLoop);
                GameObject secondRoomObject = dungeonGrid.LivingRoomAtCell(secondRoom.Value);

                if (!visitedRooms.Contains(secondRoomObject)) // checks if the second room has aleady attempted to connect this room
                {
                    RoomInformation.ConnectRooms(dungeonGrid.LivingRoomAtCell(firstRoom_delAfterLoop), dungeonGrid.LivingRoomAtCell(secondRoom.Value));
                    visitedRooms.Add(firstRoomObejct);
                    visitedRooms.Add(secondRoomObject);
                }

                //if (dungeonRooms.ContainsKey(firstRoom_delAfterLoop) && dungeonRooms.ContainsKey(secondRoom))
                //{
                //    RoomInformation.ConnectRooms(dungeonRooms[firstRoom_delAfterLoop].roomInstance, dungeonRooms[secondRoom].roomInstance);
                //}

            }

        }
    }

    private void Path(DungeonRoom _startingRoom, DungeonRoom _endRoom, RoomInformation _hallInformation, Queue<Vector2Int?> _queue)
    {
        //RoomInformation hallInformation = hallway_prefab.GetComponent<RoomInformation>();

        Debug.Log( _startingRoom.roomInstance + "<color=red> -> </color>" + _endRoom.roomInstance);
        Vector2Int startOfHallway = _startingRoom.ClosestDoor(_endRoom.anchor); // hallway starts at a random door from starting room
        Vector2Int endOfHallway = _endRoom.ClosestDoor(startOfHallway);

        Vector2Int currentHall = startOfHallway;

        do
        {
            _queue.Enqueue(currentHall);

            if (dungeonGrid.CanPlaceRoom(currentHall, _hallInformation.floorSize)) // If there is not already a room or hallway, place the hallway
            {
                DungeonRoom hallway = new DungeonRoom(currentHall, _hallInformation.floorSize, hallway_prefab);
                hallway.InstantiateRoom(hallwayManager.transform);
                dungeonGrid.ReserveHallway(hallway.anchor, hallway);
                dungeonRooms.Add(hallway.anchor, hallway);
            }

            Vector2Int nextHallPosition = DungeonGridWalkRandomDirection(currentHall, endOfHallway); // walks to the next position

            if (dungeonGrid.IsRoomOccupied(nextHallPosition)) // validates the next position
            {
                int recursionCounter = 1;
                ConnectHallToRoom(currentHall, dungeonGrid.RoomAtCell(nextHallPosition).ClosestDoor(currentHall), _queue, endOfHallway, _hallInformation, dungeonGrid.RoomAtCell(nextHallPosition), recursionCounter); // if the next position is a room, connect hallway through the room and adds the path the the walked path
            }
            currentHall = nextHallPosition; // moves the currentHall to the validated next position
            //if (dungeonGrid.CanPlaceRoom(currentHall)
        } while (currentHall != endOfHallway); // walk until the current position is at the end of the hallway

        _queue.Enqueue(currentHall); // this adds the last hall segment to the queue
        if (dungeonGrid.CanPlaceRoom(currentHall, _hallInformation.floorSize)) // adds the last hallway to the room
        {
            DungeonRoom hallway = new DungeonRoom(currentHall, _hallInformation.floorSize, hallway_prefab);
            hallway.InstantiateRoom(hallwayManager.transform);
            dungeonGrid.ReserveHallway(hallway.anchor, hallway);
            dungeonRooms.Add(hallway.anchor, hallway);
        }



        // connect the first room with first hallway

        // get the direction
        RoomDirection startingRoom_DoorDirection = _startingRoom.DoorDirection(startOfHallway);

        RoomInformation.ConnectRooms(_startingRoom.roomInstance.GetComponent<RoomInformation>(), dungeonGrid.LivingRoomAtCell(startOfHallway).GetComponent<RoomInformation>(), startingRoom_DoorDirection); // connects the first visited room to the hallway

        RoomDirection endingRoom_DoorDirection = _endRoom.DoorDirection(endOfHallway);

        RoomInformation.ConnectRooms(_endRoom.roomInstance.GetComponent<RoomInformation>(), dungeonGrid.LivingRoomAtCell(endOfHallway).GetComponent<RoomInformation>(), endingRoom_DoorDirection); // connects the first visited room to the hallway



    }

    private void ConnectHallToRoom(Vector2Int _currentHallPosition, Vector2Int _roomClosestDoor, Queue<Vector2Int?> _queue, in Vector2Int _endOfHallway, RoomInformation _hallInformation, DungeonRoom _connectedRoom, int _connectRoomRecursionCounter) // in makes it readonly // does not add first point to the queue inclusively
    {
        if (_connectRoomRecursionCounter > 25) return;
        Debug.Log("ConnectHallwayToRoom Called");
        Vector2Int nextHallPosition = _currentHallPosition;

        //if (_currentHallPosition == _roomClosestDoor)
        //{

        //}

        if (dungeonGrid.CanPlaceRoom(nextHallPosition, _hallInformation.floorSize)) // this will make sure that a room instanstiates at the end of the rooom
        {
            DungeonRoom currHall = new DungeonRoom(nextHallPosition, _hallInformation.floorSize, hallway_prefab);
            currHall.InstantiateRoom(hallwayManager.transform);
            dungeonGrid.ReserveHallway(nextHallPosition, currHall);
            dungeonRooms.Add(nextHallPosition, currHall);
        }

        while (_currentHallPosition != _roomClosestDoor) // ROOM ENTRY: this moved the path the the position of the closest door where the hall can enter the room
        {
            if (dungeonGrid.IsRoomOccupied(_currentHallPosition))
            {
                break;
            }
            int attemptCount = 0;
            if (dungeonGrid.CanPlaceRoom(nextHallPosition, _hallInformation.floorSize)) // hard coded hallway size
            {
                DungeonRoom currHall = new DungeonRoom(nextHallPosition, _hallInformation.floorSize, hallway_prefab); // creates a hallway at next position (currentPosition)
                currHall.InstantiateRoom(hallwayManager.transform); // spawns the hallway
                dungeonGrid.ReserveHallway(nextHallPosition, currHall); // adds the hallway to grid
                dungeonRooms.Add(nextHallPosition, currHall);
            }

            nextHallPosition = DungeonGridWalkRandomDirection(_currentHallPosition, _roomClosestDoor);


            while (dungeonGrid.IsRoomOccupied(nextHallPosition) && _roomClosestDoor != nextHallPosition && _currentHallPosition != _roomClosestDoor) // validates the position of the next step
            {
                nextHallPosition = DungeonGridWalkRandomDirection(_currentHallPosition, _roomClosestDoor);
                attemptCount++;
                if (attemptCount > 1000)
                {

                    ConnectHallToRoom(_currentHallPosition, dungeonGrid.RoomAtCell(nextHallPosition).ClosestDoor(_currentHallPosition), _queue, _endOfHallway, _hallInformation, dungeonGrid.RoomAtCell(nextHallPosition), _connectRoomRecursionCounter++);
                }
            }


            //if (_currentHallPosition != _roomClosestDoor)
            //{
            //    int attemptCounter = 0;
            //    do // this walks the currentPosition to the closest door to current position if not already there.
            //    {
            //        nextHallPosition = DungeonGridWalkRandomDirection(_currentHallPosition, _roomClosestDoor); // if it cant move on because it is running into a room, connect that room
            //        attemptCounter++;
            //        if (attemptCounter > 1000)
            //        {
            //            ConnectHallToRoom(_currentHallPosition, dungeonGrid.RoomAtCell(nextHallPosition).ClosestDoor(_currentHallPosition), _queue, _endOfHallway, _hallInformation, dungeonGrid.RoomAtCell(nextHallPosition));
            //        }

            //    } while (!dungeonGrid.IsRoomOccupied(nextHallPosition) && _roomClosestDoor != nextHallPosition); // If there is a room and the next position is not the hallway end
            //}



            _currentHallPosition = nextHallPosition; // sets the position for the closest door (next position) to the current position
            _queue.Enqueue(_currentHallPosition); // this will add the closest door the the queue
            
        }

        if (dungeonGrid.CanPlaceRoom(nextHallPosition, _hallInformation.floorSize)) // this will make sure that a room instanstiates at the end of the rooom
        {
            DungeonRoom currHall = new DungeonRoom(nextHallPosition, _hallInformation.floorSize, hallway_prefab);
            currHall.InstantiateRoom(hallwayManager.transform);
            dungeonGrid.ReserveHallway(nextHallPosition, currHall);
            dungeonRooms.Add(nextHallPosition, currHall);
        }
        




        //Debug.Log("<color=green>ROOM ENTRY: </color?" + _currentHallPosition);


        // ROOM EXIT: this walks through the room towards the door closest to the wallways end point
        Vector2Int exitDoor = _connectedRoom.ClosestDoor(_endOfHallway);
        while (_currentHallPosition != exitDoor) // this will walk Through The room // exit door inclusive
        {
            nextHallPosition = DungeonGridWalkRandomDirection(_currentHallPosition, exitDoor);

             if (dungeonGrid.IsRoomOccupied(nextHallPosition) || nextHallPosition == exitDoor) // this if statements only moves the current position IF the path is going through the room or is the exit door
             {
                 _currentHallPosition = nextHallPosition;
                 _queue.Enqueue(_currentHallPosition);
             }
        }
        //Debug.Log("<color=red>ROOM EXIT: </color?" + _currentHallPosition);

        if (dungeonGrid.CanPlaceRoom(nextHallPosition, _hallInformation.floorSize)) // this will make sure that a room instanstiates at the end of the rooom
        {
            DungeonRoom currHall = new DungeonRoom(nextHallPosition, _hallInformation.floorSize, hallway_prefab);
            currHall.InstantiateRoom(hallwayManager.transform);
            dungeonGrid.ReserveHallway(nextHallPosition, currHall);
            dungeonRooms.Add(nextHallPosition, currHall);
        }


    }

    private void ReservePath(Vector2Int _startingPoint, Vector2Int _target, Queue<Vector2Int> _queue, bool _goThroughRoom = false, Vector2Int _hallwayEnd = new Vector2Int()) // starting point inclusive
    {
        RoomInformation hallwayInformation = hallway_prefab.GetComponent<RoomInformation>();

        //Vector2Int currentPosition = _startingPoint;
        int attemptCounter = 0;

        //do
        //{
        //    Vector2Int nextPosition;
        //    do
        //    {
        //        nextPosition = DungeonGridWalkRandomDirection(currentPosition, _target);

        //        attemptCounter++;
        //        if (attemptCounter > 1000)
        //        {
        //            ReservePath(nextPosition, dungeonGrid.RoomAtCell(nextPosition).ClosestDoor(nextPosition), _queue);

        //        }
        //    } while (dungeonGrid.IsRoomOccupied(nextPosition));
        //    currentPosition = nextPosition;
        //    _queue.Enqueue(currentPosition);

        //} while (currentPosition != _target);



        Vector2Int currentPosition = _startingPoint;


        do // spawns hallways to the target point (the targeted door)
        {
            if (dungeonGrid.CanPlaceRoom(currentPosition, hallwayInformation.floorSize)) // prevents hallway from being placed in the same place as a room but not a hallway
            {
                DungeonRoom hallway = new DungeonRoom(currentPosition, hallwayInformation.floorSize, hallway_prefab);
                hallway.InstantiateRoom(hallwayManager.transform);
                dungeonGrid.ReserveHallway(hallway.anchor, hallway);
                dungeonRooms.Add(hallway.anchor, hallway);
            }


            Vector2Int nextPosition;
            do
            {
                nextPosition = DungeonGridWalkRandomDirection(currentPosition, _target);
                attemptCounter++;

                if (attemptCounter > 1000) // if the next room is occupied then recurse to the next room first.
                {
                    ReservePath(nextPosition, dungeonGrid.RoomAtCell(nextPosition).ClosestDoor(nextPosition), _queue);
                }
            } while (!dungeonGrid.IsRoomOccupied(nextPosition)); // checks if the next position is valid before moving
            currentPosition = nextPosition;
            _queue.Enqueue(currentPosition);

            if (_goThroughRoom)
            {
                currentPosition = _hallwayEnd; //room closest door to target
            }

        } while (currentPosition != _target);

        Debug.Log(_startingPoint + " -> " + currentPosition + " -> " + _target + " Reached");
    }
    private Vector2Int DungeonGridWalkRandomDirection(Vector2Int currentPosition, Vector2Int targetPosition)
    {
        bool xAxisPossible = (currentPosition.x != targetPosition.x);
        bool yAxisPossible = (currentPosition.y != targetPosition.y);
        Vector2 distanceFromTarget = targetPosition - currentPosition;

        if (!yAxisPossible) // move x axis // if y not possible
        {
            currentPosition.x = DungeonGridMoveAxisX(currentPosition, distanceFromTarget.x > 0);
        }
        else if (!xAxisPossible) // move y Axis // if x not possible
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
            output = UnityEngine.Random.value > 0.5f;
        }

        return output;
    }

    private void GenerateStartingRoom(Dictionary<Vector2, GameObject> _generatedRooms)
    {
        Vector2Int startingRoomPos = new Vector2Int(0, 0);
        RoomInformation startingRoomInfo = startingRoom_Prefab.GetComponent<RoomInformation>();
        DungeonRoom startingRoom = new DungeonRoom(startingRoomPos, startingRoomInfo.floorSize, startingRoom_Prefab);

        dungeonRooms.Add(startingRoomPos, startingRoom);
        dungeonGrid.ReserveRoom(startingRoomPos, startingRoomInfo.floorSize, startingRoom); // adjust to be dynamic later
    }
    private void GenerateHandRooms(Dictionary<Vector2, GameObject> _generatedRooms)
    {
        //Vector2 roomPosition;
        RoomInformation handRoomInfo = handRoom_Prefab.GetComponent<RoomInformation>();
        //DungeonRoom handRoom = new DungeonRoom(new Vector2Int(), handRoomInfo.floorSize, handRoom_Prefab);

        for (int i = 0; i < 2; i++)
        {
            Vector2Int randomGridPosition = dungeonGrid.GetValidRandomRoomPosition(handRoomInfo.floorSize, roomSpawnRadius);
            DungeonRoom handRoom = new DungeonRoom(randomGridPosition, handRoomInfo.floorSize, handRoom_Prefab);

            dungeonGrid.ReserveRoom(randomGridPosition, handRoomInfo.floorSize, handRoom);
            dungeonRooms.Add(randomGridPosition, handRoom);
        }
    }


    private void GenerateLootRooms(Dictionary<Vector2, GameObject> _generatedRooms)
    {
        for (int i = 0; i < roomCount; i++)
        {
            GenerateRoom(_generatedRooms);
        }
    }

    private void GenerateRoom(Dictionary<Vector2, GameObject> _generatedRooms)
    {
        int roomPrefabIndex = GetRandomPrefabPosition();
        RoomInformation randomRoomInfo = room_prefabs[roomPrefabIndex].GetComponent<RoomInformation>();

        Vector2Int randGridPos = dungeonGrid.GetValidRandomRoomPosition(randomRoomInfo.floorSize, roomSpawnRadius);

        if(dungeonGrid.CanPlaceRoom(randGridPos, randomRoomInfo.floorSize))
        {
            DungeonRoom room = new DungeonRoom(randGridPos, randomRoomInfo.floorSize, room_prefabs[roomPrefabIndex]);
            dungeonGrid.ReserveRoom(room.anchor, room.size, room);
            dungeonRooms.Add(room.anchor, room);
        }
        

    }

  
    private int GetRandomPrefabPosition()
    {
        return UnityEngine.Random.Range(0, room_prefabs.Length);
    }

    private List<Edge> PrimsMST(Vector2 _startingRoom, List<Edge> _paths)
    {
        List<Edge> outMST = new List<Edge>();
        HashSet<Vector2> visitedRooms = new HashSet<Vector2>();

        visitedRooms.Add(_startingRoom);
        while (outMST.Count < dungeonRooms.Count - 1 || outMST.Count < _paths.Count)
        {
            Edge? bestPath = null; // the ? allows for variables to be null because c# doesn't like it otherwise

            foreach (Edge path in _paths)
            {
                bool roomAVisited = visitedRooms.Contains(path.a);
                bool roomBVisited = visitedRooms.Contains(path.b);

                if (roomAVisited == roomBVisited) continue; // this way the tree only branches from rooms already in the tree

                if (bestPath == null || bestPath.Value.weight > path.weight) 
                    bestPath = path; // if this path is a shorter distance it is the best path



            }
            if (bestPath != null)
            {
                Edge selectedPath = bestPath.Value;
                outMST.Add(selectedPath);

                visitedRooms.Add(selectedPath.a);
                visitedRooms.Add(selectedPath.b);
            }
            
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

    private int ManhattenDistance(Vector2Int _first, Vector2Int _second)
    {
        return Mathf.Abs(_first.x - _second.x) + Mathf.Abs(_first.y - _second.y);
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
    Dictionary<Vector2Int, DungeonRoom> cellToRoomKey = new Dictionary<Vector2Int, DungeonRoom>();

    public bool IsRoomOccupied(Vector2Int _cell)
    {
        return roomCells.Contains(_cell);
    }
    public bool IsHallwayOccupied(Vector2Int _cell)
    {
        return hallwayCells.Contains(_cell);
    }
    public bool IsOccupied(Vector2Int _cell)
    {
        return IsRoomOccupied(_cell) || IsHallwayOccupied(_cell);
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

    public void ReserveRoom(Vector2Int _anchor, Vector2Int _size, DungeonRoom _room)
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
                cellToRoomKey.Add(_anchor + new Vector2Int(row, column), _room);
            }
        }

    }

    bool CanPlaceHallway(Vector2Int _cell)
    {
        return !roomCells.Contains(_cell);
    }

    public bool ReserveHallway(Vector2Int _cell, DungeonRoom _room)
    {
        if (IsOccupied(_cell)) return false;

        hallwayCells.Add(_cell);
        cellToRoomKey.Add(_cell, _room);
        return true;
    }

    public void Clear()
    {
        roomCells.Clear();
        hallwayCells.Clear();
        cellToRoomKey.Clear();
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

    public DungeonRoom RoomAtCell(Vector2Int _cell)
    {
        return cellToRoomKey[_cell];
    }
    public bool TryGetRoomAtCell(Vector2Int _cell, out DungeonRoom _roomAtCell)
    {
        return cellToRoomKey.TryGetValue(_cell, out _roomAtCell);
    }
    public GameObject LivingRoomAtCell(Vector2Int _cell)
    {
        if (cellToRoomKey.Keys.Contains(_cell))
        {
            return cellToRoomKey[_cell].roomInstance;

        }
        return null;
    }
    public void InstantiateAllRooms(Transform parent = null)
    {
        //List<DungeonRoom> containedRooms = cellToRoomKey.Values.ToList();
        //foreach(DungeonRoom room in containedRooms)
        //{
        //    room.InstantiateRoom(parent);
        //}
        HashSet<DungeonRoom> visitedRooms = new HashSet<DungeonRoom>();

        foreach(Vector2Int coor in roomCells)
        {
            if (!visitedRooms.Contains(cellToRoomKey[coor]))
            {
                cellToRoomKey[coor].InstantiateRoom(parent);
                visitedRooms.Add(cellToRoomKey[coor]);
            }
        }
    }
    public List<DungeonRoom> ListOfRooms()
    {
        HashSet<DungeonRoom> visitedRooms = new HashSet<DungeonRoom>();
        List<DungeonRoom> outputListOfRooms = new List<DungeonRoom>();
        foreach(KeyValuePair<Vector2Int, DungeonRoom> room in cellToRoomKey)
        {
            if (!visitedRooms.Contains(room.Value))
            {
                outputListOfRooms.Add(room.Value);
                visitedRooms.Add(room.Value);
            }
        }
        return outputListOfRooms;
    }
    public void GetPathBounds(Vector2Int _start, Vector2Int _end, out Vector2Int _minBounds,  out Vector2Int _maxBounds)
    {
        _minBounds = default;
        _maxBounds = default;

        if (_start.x < _end.x)
        {
            _minBounds.x =_start.x;
            _maxBounds.x = _end.x;
        }
        else
        {
            _minBounds.x = _end.x;
            _maxBounds.x = _start.x;
        }

        if (_start.y < _end.y)
        {
            _minBounds.y = _start.y;
            _maxBounds.y = _end.y;
        }
        else
        {
            _minBounds.y = _end.y;
            _maxBounds.y = _start.y;
        }
    }
}

public class DungeonRoom
{
    public Vector2Int anchor; // this is the inital point of the room (3,4). a long room occupy (3,4) & (4,4) for example // bottom left pivot
    public Vector2Int size; // long room would be (2,1)
    public GameObject prefab = null;
    public GameObject roomInstance = null;
    public Vector2 CenterCell => anchor + new Vector2((size.x - 1) * 0.5f, (size.y - 1) * 0.5f); // starts on a 0 scale not a 1 scale // world space
     
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
        float yCenterCell = (size.y - 1) * 0.5f; // measure from the cell centers
        float xCenterCell = (size.x -1) * 0.5f;
        Vector2Int frontDoor = Vector2Int.FloorToInt(new Vector2(anchor.x + xCenterCell, anchor.y + (size.y - 1))); 
        frontDoor.y += 1;
        Vector2Int backDoor = Vector2Int.CeilToInt(new Vector2(anchor.x + xCenterCell, anchor.y)); 
        backDoor.y -= 1;
        Vector2Int leftDoor = Vector2Int.FloorToInt(new Vector2(anchor.x, anchor.y + yCenterCell)); 
        leftDoor.x -= 1;
        Vector2Int rightDoor = Vector2Int.FloorToInt(new Vector2(anchor.x + (size.x - 1), anchor.y + yCenterCell)); 
        rightDoor.x += 1;

        //Vector2Int frontDoor = Vector2Int.FloorToInt(new Vector2(anchor.x + xCenterCell, anchor.y + (size.y - 1)));
        //Vector2Int backDoor = Vector2Int.FloorToInt(new Vector2(anchor.x + xCenterCell, anchor.y));
        //Vector2Int leftDoor = Vector2Int.FloorToInt(new Vector2(anchor.x, anchor.y + yCenterCell));
        //Vector2Int rightDoor = Vector2Int.FloorToInt(new Vector2(anchor.x + (size.x - 1), anchor.y + yCenterCell));


        doorPositions.Add(frontDoor); // front
        doorPositions.Add(backDoor); // back
        doorPositions.Add(leftDoor); // left
        doorPositions.Add(rightDoor); // right
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
    public RoomDirection DoorDirection(Vector2Int _cell)
    {
        if (doorPositions.Count > 4)
        {
            Debug.Log("Too ManyDoors to Get Direction");
        }
        
        for (int i = 0; i < doorPositions.Count; i++)
        {
            if (doorPositions[i] == _cell)
            {
                return (RoomDirection)i;
            }
        }
        return RoomDirection.NONE;
    }
}
