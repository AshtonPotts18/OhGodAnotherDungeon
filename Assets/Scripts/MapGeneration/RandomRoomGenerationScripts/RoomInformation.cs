using NUnit.Framework.Constraints;
using UnityEngine;

public class RoomInformation : MonoBehaviour
{
    public Bounds roomSize = new Bounds(new Vector3(0,0,0), new Vector3 (5,5,5));
    public Vector2 floorSize = new Vector2(5,5);
    [Header("Doors: Front, Bottom, Left, Right")]
    public bool[] openDoors = new bool[4];



    static public void ConnectRooms(GameObject room1, GameObject room2)
    {
        // check positions of each room to determine the direction each room needs to connect

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

        Vector3 roomDirection = (room2.transform.position - room1.transform.position)/5; // the difference will indicate what direction room 2 is from room 1

        // connect the rooms
        // all positions are with respect to room 1
        if (roomDirection == Vector3.forward)
        {
            firstRoomInfo.openDoors[0] = true;
            secondRoomInfo.openDoors[1] = true;
        }
        else if (roomDirection == Vector3.back) 
        {
            firstRoomInfo.openDoors[1] = true;
            secondRoomInfo.openDoors[0] = true;
        }
        else if (roomDirection == Vector3.left)
        {
            firstRoomInfo.openDoors[2] = true;
            secondRoomInfo.openDoors[3] = true;
        }
        else if (roomDirection == Vector3.right)
        {
            firstRoomInfo.openDoors[3] = true;
            secondRoomInfo.openDoors[2] = true;
        }

        //update the prefabs
    }

}
