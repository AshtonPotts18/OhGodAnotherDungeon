using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
    [SerializeField] private string exitPrompt = "Press Left Click to Exit";

    private bool playerNearby;

    private void Awake()
    {
        ConfigurePromptCanvas();
    }

    private void Update()
    {
        if (Shop.Instance == null)
        {
            promptText.text = "Shop unavailable";
            return;
        }

        promptText.text =
            Shop.Instance.QuotaProgress >= Shop.Instance.QuotaTarget
                ? exitPrompt
                : promptText.text;
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
            ? GameManager.Instance.LevelElapsedSeconds
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

    private void ConfigurePromptCanvas()
    {
        if (promptObject == null)
            return;

        Canvas promptCanvas = promptObject.GetComponentInParent<Canvas>();
        if (promptCanvas == null)
        {
            Debug.LogError("LevelExit: Prompt is not under a Canvas.", this);
            return;
        }

        CanvasScaler scaler = promptCanvas.GetComponent<CanvasScaler>();
        if (scaler == null)
            scaler = promptCanvas.gameObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform promptRect = promptObject.GetComponent<RectTransform>();
        if (promptRect != null)
        {
            promptRect.anchorMin = new Vector2(0.5f, 0.5f);
            promptRect.anchorMax = new Vector2(0.5f, 0.5f);
            promptRect.pivot = new Vector2(0.5f, 0.5f);
        }
    }
}