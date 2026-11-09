using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] RandomlyGenerateRooms mapGenerator;
    public LevelManager levelManager;
    [SerializeField] GameObject player;
    [SerializeField] ItemGeneration itemGenerator;
    [SerializeField] List<GameObject> itemPool;
    [SerializeField] BuyStation station;
    public static GameObject Player {get; set; } // put prefab here to be instantiated in start
    private Transform playerSpawnPoint;
    public static Camera PlayerCamera { get; private set; }

    private List<GameObject> roomsList = new List<GameObject>();
    private List<GameObject> spawnedItemList = new List<GameObject>();

    private BuyStation StationObject;
    public float LevelElapsedSeconds => Time.time - levelStartTime;
	public float levelStartTime;

    private bool isLoadingNextLevel = false;

    private void Awake()
    {
        CheckInstance();
        Player = player;
    }
    public void Start()
    {
        //StartGame();
        Console.Write("Map Generated");
    }

    public void LoadLevel()
    {
        levelManager.LoadLevel(mapGenerator);
    }
    public void StartGame()
    {
        levelStartTime = Time.time;

        LoadLevel();

        GenerateLevel();
        InitiateItemGenerator();     
        SpawnPlayer();
        StartShop();
    }
    public void EndGame()
    {
        //Player.SetActive(false);
        ClearRooms();
        ClearHallways();
        ClearItems();
        mapGenerator.ClearMap();
        Shop.Instance.ResetQuotaProgress();
        Console.Write("Map Cleared");
        DespawnPlayer();
    }

    public void LoadNextLevel()
    {
        if (isLoadingNextLevel) { return; }
        StartCoroutine(LoadNextLevelRoutine());
    }

    private IEnumerator LoadNextLevelRoutine()
    {
        isLoadingNextLevel = true;

        ClearRooms();
        ClearHallways();
        ClearItems();
        mapGenerator.ClearMap();

        // Let Unity finish clearing the old map.
        yield return null;

        levelManager.CurrentLevel++;
        levelManager.LoadLevel(mapGenerator);
        GenerateLevel(); // Also sets playerSpawnPoint from StartingRoom/SpawnPoint.

        // Let the new map finish generating.
        yield return null;

        MovePlayerToSpawn(playerSpawnPoint);

        InitiateItemGenerator();
        StartShop();

        isLoadingNextLevel = false;
    }


    private void DespawnPlayer()
    {
        Destroy(Player);
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
        roomsList.Clear();
        spawnedItemList.Clear();
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

    public void GenerateLevel()
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
        Player = player;
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

        Player = Instantiate(Player,
            playerSpawnPoint.position, playerSpawnPoint.rotation, transform);
        Player.tag = "Player";
        PlayerCamera = (Player.transform.Find("Main Camera")).GetComponentInParent<Camera>();
    }

    private void MovePlayerToSpawn(Transform spawnPoint)
    {
        if (Player == null || spawnPoint == null)
        {
            Debug.LogError("Player or SpawnPoint was not found.", this);
            return;
        }

        CharacterController controller = Player.GetComponent<CharacterController>();
        bool controllerWasEnabled = controller != null && controller.enabled;

        if (controller != null)
            controller.enabled = false;

        Player.transform.SetPositionAndRotation(
            spawnPoint.position,
            spawnPoint.rotation
        );
    }
    private void StartShop()
    {
        StationObject = station;
        StationObject.StartBuyStation();
    }
}

