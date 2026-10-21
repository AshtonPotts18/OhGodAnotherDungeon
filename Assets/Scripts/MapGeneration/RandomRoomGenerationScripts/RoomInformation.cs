using UnityEngine;

public class RoomInformation : MonoBehaviour
{
    public Bounds roomSize = new Bounds(new Vector3(0,0,0), new Vector3 (5,5,5));
    public Vector2 floorSize;
    public bool[] openDoors;

    private void Awake()
    {
        Vector3 floorBounds = new Vector3(floorSize.x, roomSize.extents.y, floorSize.y);
        roomSize.extents = floorBounds;
    }


}
