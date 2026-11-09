using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour, IInteract
{
    [Header("Scene References")]
    private EnemySpawner enemySpawner;

    [SerializeField]
    private string transitionSceneName = "LevelTransition";

    [Header("Prompt")]
    [SerializeField] private GameObject promptObject;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string exitPrompt = "Press F to Exit";

    private bool playerNearby;

    private void Awake()
    {
        if (promptObject != null)
            promptObject.SetActive(false);
    }

    private void Update()
    {
        //if (playerNearby &&
        //    Keyboard.current != null &&
        //    Keyboard.current.fKey.wasPressedThisFrame)
        //{
        //    Interact();
        //    return;
        //}

        if (!playerNearby || promptText == null)
            return;

        if (Shop.Instance == null)
        {
            promptText.text = "Shop unavailable";
            return;
        }

        promptText.text =
            Shop.Instance.QuotaProgress >= Shop.Instance.QuotaTarget
                ? exitPrompt
                : "Meet the quota to exit";
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("LevelExit trigger detected the player.", this);
        if (!IsPlayer(other))
            return;

        playerNearby = true;

        if (promptObject != null)
            promptObject.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
            return;

        playerNearby = false;

        if (promptObject != null)
            promptObject.SetActive(false);
    }

    private bool IsPlayer(Collider other)
    {
        return other.CompareTag(playerTag) ||
               other.transform.root.CompareTag(playerTag);
    }

    private EnemySpawner FindEnemySpawner()
    {
        if (enemySpawner == null)
            enemySpawner = FindAnyObjectByType<EnemySpawner>();

        return enemySpawner;
    }

    public void Interact()
    {
        if (Shop.Instance == null)
        {
            Debug.LogError("LevelExit: No Shop instance found.", this);
            return;
        }

        if (Shop.Instance.QuotaProgress < Shop.Instance.QuotaTarget)
        {
            Debug.Log("The quota has not been met yet.", this);
            return;
        }

        EnemySpawner spawner = FindEnemySpawner();

        if (spawner == null)
        {
            Debug.LogError(
                "LevelExit: No EnemySpawner found in the gameplay scene.",
                this);
            return;
        }

        float levelTime = GameManager.Instance != null
            ? GameManager.Instance.levelManager.CurrentLevel
            : 0f;

        int moneyCollected =
            Shop.Instance.QuotaProgress + Shop.Instance.PartyMoney;

        GameManager.Instance.levelManager.CompleteLevel();

        RunSummarySave.Save(new RunSummary
        {
            playerName = PlayerPrefs.GetString("PlayerName", "Player 1"),
            level = spawner.currentLevel,
            elapsedSeconds = levelTime,
            goldCollected = moneyCollected
        });

        SceneManager.LoadScene(transitionSceneName);
        GameManager.Instance.EndGame();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}