using System;
using System.Collections.Generic;
using UnityEngine;

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
    }
    public void StartGame()
    {
        roomsList = mapGenerator.GenerateMap();
        playerSpawnPoint = mapGenerator.startingRoom.transform.Find("SpawnPoint");

        SpawnPlayer();

        itemGenerator.ItemPool_prefab = itemPool;
        
        spawnedItemList = itemGenerator.GenerateItems(100);

        PlayerCamera = (Player.transform.Find("Main Camera")).GetComponentInParent<Camera>();
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

        Player = Instantiate(Player,
            playerSpawnPoint.position, playerSpawnPoint.rotation, transform);
        Player.tag = "Player";
    }
}

