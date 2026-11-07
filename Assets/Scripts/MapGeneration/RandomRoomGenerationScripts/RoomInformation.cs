using System.Linq;
using UnityEngine;

public enum RoomDirection
{
    FRONT,
    BACK,
    LEFT,
    RIGHT,
    NONE
}
public class RoomInformation : MonoBehaviour
{
    public Bounds roomSize = new Bounds(new Vector3(0, 0, 0), new Vector3(5, 5, 5));
    public Vector2Int floorSize = new Vector2Int(5, 5);
    [Header("Doors: Front, Bottom, Left, Right")]
    public bool[] openDoors = new bool[4];

    [SerializeField] GameObject[] walls = new GameObject[4];
    [SerializeField] private GameObject[] doors = new GameObject[4];

    public void Awake()
    {
        foreach (GameObject door in doors)
        {
            door.SetActive(false);
        }
    }

    static public void ConnectRooms(GameObject room1, GameObject room2)
    {
        // check positions of each room to determine the direction each room needs to connect
        if (room1 == null || room2 == null)
        {
            Debug.Log("Connect Rooms are Null");
        }
        if (!room1.TryGetComponent<RoomInformation>(out RoomInformation firstRoomInfo))
        {
            Debug.Log("No First Room Information To Connect");
            return; // if no component found
        }
        if (!room2.TryGetComponent<RoomInformation>(out RoomInformation secondRoomInfo))
        {
            Debug.Log("No Second Room Information To Connect");
            return;
        }

        Vector3 roomDirection = (room2.transform.position - room1.transform.position) / 5; // the difference will indicate what direction room 2 is from room 1

        // connect the rooms
        // all positions are with respect to room 1
        //if (roomDirection == Vector3.forward)
        //{
        //    firstRoomInfo.openDoors[0] = true;
        //    firstRoomInfo.TryActivateDoor(0);
        //    secondRoomInfo.openDoors[1] = true;
        //    secondRoomInfo.TryActivateDoor(1);
        //}
        //else if (roomDirection == Vector3.back)
        //{
        //    firstRoomInfo.openDoors[1] = true;
        //    firstRoomInfo.TryActivateDoor(1);
        //    secondRoomInfo.openDoors[0] = true;
        //    secondRoomInfo.TryActivateDoor(0);
        //}
        //else if (roomDirection == Vector3.left)
        //{
        //    firstRoomInfo.openDoors[2] = true;
        //    firstRoomInfo.TryActivateDoor(2);
        //    secondRoomInfo.openDoors[3] = true;
        //    secondRoomInfo.TryActivateDoor(3);
        //}
        //else if (roomDirection == Vector3.right)
        //{
        //    firstRoomInfo.openDoors[3] = true;
        //    firstRoomInfo.TryActivateDoor(3);
        //    secondRoomInfo.openDoors[2] = true;
        //    firstRoomInfo.TryActivateDoor(2);
        //}

        // reworked direction
        if (Mathf.Abs(roomDirection.x) > Mathf.Abs(roomDirection.z)) // if the x axis is greater, the room is relative to the x axis
        {
            if (roomDirection.x > 0)
            {
                Debug.Log(firstRoomInfo + " Right");
                firstRoomInfo.openDoors[(int)RoomDirection.RIGHT] = true;
                firstRoomInfo.TryActivateDoor((int)RoomDirection.RIGHT);

                secondRoomInfo.openDoors[(int)RoomDirection.LEFT] = true;
                secondRoomInfo.TryActivateDoor((int)RoomDirection.LEFT);
            }
            else
            {
                Debug.Log(firstRoomInfo + " Left");
                firstRoomInfo.openDoors[(int)RoomDirection.LEFT] = true;
                firstRoomInfo.TryActivateDoor((int)RoomDirection.LEFT);

                secondRoomInfo.openDoors[(int)RoomDirection.RIGHT] = true;
                secondRoomInfo.TryActivateDoor((int)RoomDirection.RIGHT);
            }
        }
        else
        {
            if (roomDirection.z > 0)
            {
                Debug.Log(firstRoomInfo + " Front");
                firstRoomInfo.openDoors[(int)RoomDirection.FRONT] = true;
                firstRoomInfo.TryActivateDoor((int)RoomDirection.FRONT);

                secondRoomInfo.openDoors[(int)RoomDirection.BACK] = true;
                secondRoomInfo.TryActivateDoor((int)RoomDirection.BACK);
            }
            else
            {
                Debug.Log(firstRoomInfo + " Back");
                firstRoomInfo.openDoors[(int)RoomDirection.BACK] = true;
                firstRoomInfo.TryActivateDoor((int)RoomDirection.BACK);

                secondRoomInfo.openDoors[(int)RoomDirection.FRONT] = true;
                secondRoomInfo.TryActivateDoor((int)RoomDirection.FRONT);
            }
        }


        //update the prefabs
        firstRoomInfo.UpdateWalls();
        secondRoomInfo.UpdateWalls();
    }

    public static void ConnectRooms(RoomInformation _firstRoom, RoomInformation _secondRoom, RoomDirection _secondRoomDirection)
    {
        switch (_secondRoomDirection)
        {
            case (RoomDirection.FRONT):
                _firstRoom.openDoors[(int)RoomDirection.FRONT] = true;
                _firstRoom.TryActivateDoor((int)RoomDirection.FRONT);

                _secondRoom.openDoors[(int)RoomDirection.BACK] = true;
                _secondRoom.TryActivateDoor((int)RoomDirection.BACK);
                break;
            case(RoomDirection.BACK):
                _firstRoom.openDoors[(int)RoomDirection.BACK] = true;
                _firstRoom.TryActivateDoor((int)RoomDirection.BACK);

                _secondRoom.openDoors[(int)RoomDirection.FRONT] = true;
                _secondRoom.TryActivateDoor((int)RoomDirection.FRONT);
                break;
            case (RoomDirection.LEFT):
                _firstRoom.openDoors[(int)RoomDirection.LEFT] = true;
                _firstRoom.TryActivateDoor((int)RoomDirection.LEFT);

                _secondRoom.openDoors[(int)RoomDirection.RIGHT] = true;
                _secondRoom.TryActivateDoor((int)RoomDirection.RIGHT);
                break;
            case (RoomDirection.RIGHT):
                _firstRoom.openDoors[(int)RoomDirection.RIGHT] = true;
                _firstRoom.TryActivateDoor((int)RoomDirection.RIGHT);

                _secondRoom.openDoors[(int)RoomDirection.LEFT] = true;
                _secondRoom.TryActivateDoor((int)RoomDirection.LEFT);
                break;
        }
        _firstRoom.UpdateWalls();
        _secondRoom.UpdateWalls();
    }

    public void UpdateWalls()
    {
        for (int wallCount = 0; wallCount < walls.Count(); wallCount++)
        {
            if (openDoors[wallCount])
            {
                walls[wallCount].SetActive(!openDoors[wallCount]);
            }
        }
    }
    private void TryActivateDoor(int index)
    {
        if (doors.Length > index && doors[index].activeInHierarchy == false)
        {
            if (doors[index] != null) 
                doors[index].SetActive(true);
        }
    }
}
