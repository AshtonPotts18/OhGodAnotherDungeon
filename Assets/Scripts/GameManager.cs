using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] RandomlyGenerateRooms mapGenerator;
    [SerializeField] GameObject player;
    public static GameObject Player {get; private set; } // put prefab here to be instantiated in start
    private Transform playerSpawnPoint;
    public static Camera PlayerCamera { get; private set; }



    private void Awake()
    {
        CheckInstance();

        Player = player;
        mapGenerator.GenerateMap();
        playerSpawnPoint = mapGenerator.startingRoom.transform.Find("SpawnPoint");
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnPlayer();
        PlayerCamera = (Player.transform.Find("Main Camera")).GetComponentInParent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        
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

