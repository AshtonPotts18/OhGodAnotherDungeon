using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Rendering.CameraUI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] RandomlyGenerateRooms mapGenerator;
    [SerializeField] GameObject player;
    [SerializeField] ItemGeneration itemGenerator;
    [SerializeField] List<GameObject> itemPool;
    public static GameObject Player {get; private set; } // put prefab here to be instantiated in start
    private Transform playerSpawnPoint;
    public static Camera PlayerCamera { get; private set; }

    private List<GameObject> roomsList = new List<GameObject>();
    private List<GameObject> spawnedItemList = new List<GameObject>();



    private void Awake()
    {
        CheckInstance();
        Player = player;
    }
    private void Start()
    {

        StartGame();
        
        Console.Write("Map Generated");
    }

    
    public void StartGame()
    {
        GenerateLevel();
        InitiateItemGenerator();     
        SpawnPlayer();
    }
    public void EndGame()
    {
        Destroy(Player);
        ClearRooms();
        ClearHallways();
        ClearItems();
        Console.Write("Map Cleared");
    }

    
    private void CheckInstance()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void ClearItems()
    {
        Transform parent = transform.Find("ItemManager");
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject.Destroy(parent.GetChild(i).gameObject);
        }
    }

    private void ClearHallways()
    {
        Transform parent = transform.Find("MapManager").transform.Find("HallwayManager");
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject.Destroy(parent.GetChild(i).gameObject);
        }
    }

    private void ClearRooms()
    {
        Transform parent = transform.Find("MapManager").transform.Find("RoomManager");
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject.Destroy(parent.GetChild(i).gameObject);
        }
    }


    private void InitiateItemGenerator()
    {
        itemGenerator.ItemPool_prefab = itemPool;
        itemGenerator.ItemSpawnLocations = GetRoomItemSpawnPoints(roomsList);

        spawnedItemList = itemGenerator.GenerateItems(300); // replace 100 with quota from shop
    }

    private void GenerateLevel()
    {
        roomsList = mapGenerator.GenerateMap();
        playerSpawnPoint = mapGenerator.startingRoom.transform.Find("SpawnPoint");
    }
    private List<Transform> GetRoomItemSpawnPoints(List<GameObject> _spawnedRooms)
    {
        List<Transform> output = new List<Transform>(); // list of spawn points withing all rooms
        
        foreach (var room in _spawnedRooms)
        {
            CheckRoomForSpawnPoints(output, room); // if the ItemSpawnPoint is present in the children the transform is added to the output
        }
        return output;
    }

    private void CheckRoomForSpawnPoints(List<Transform> _listOfSpawnPoints, GameObject _room)
    {
        foreach (Transform child in _room.transform) // gets the childred of the room
        {
            if (child.CompareTag("ItemSpawnPoint"))
            {
                _listOfSpawnPoints.Add(child);
            }
        }
    }
    private void SpawnPlayer()
    {
        if (Player == null)
        {
            Debug.LogError($"{name}: Assign the Player Prefab.", this);
            return;
        }

        if (playerSpawnPoint == null)
        {
            Debug.LogError($"{name}: Assign the Player Spawn Point.", this);
            return;
        }

        Player = Instantiate(player,
            playerSpawnPoint.position, playerSpawnPoint.rotation, transform);
        Player.tag = "Player";
        PlayerCamera = (Player.transform.Find("Main Camera")).GetComponentInParent<Camera>();
    }
}

