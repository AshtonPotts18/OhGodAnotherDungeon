using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;

    private GameObject spawnedPlayer;

    private void Start()
    {
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        if (spawnedPlayer != null) { return; }

        if (playerPrefab == null)
        {
            Debug.LogError($"{name}: Assign the Player Prefab.", this);
            return;
        }

        if (playerSpawnPoint == null)
        {
            Debug.LogError($"{name}: Assign the Player Spawn Point.", this);
            return;
        }

        spawnedPlayer = Instantiate( playerPrefab,
            playerSpawnPoint.position, playerSpawnPoint.rotation);
        spawnedPlayer.tag = "Player";
    }
}
