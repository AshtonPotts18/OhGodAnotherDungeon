using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionScreenUI : MonoBehaviour
{
    [Header("Summary Text")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text currentLevelText;
    [SerializeField] private TMP_Text levelTimeText;
    [SerializeField] private TMP_Text goldCollectedText;

    [Header("Scene Names")]
    [SerializeField] private string gameplaySceneName = "Ashtons WorkSpace";
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    private RunSummary summary;

    private void Start()
    {
        summary = RunSummarySave.Load();

        if(PlayerPrefs.GetInt("ResetL") == 1)
        {
            summary.level = 1;
            PlayerPrefs.SetInt("ResetL", 0);
        }

        if (summary == null)
        {
            Debug.LogError("TransitionScreenUI: No saved run summary was found.", this);
            return;
        }

        if (playerNameText != null)
            playerNameText.text = summary.playerName;

        if (currentLevelText != null)
            currentLevelText.text = summary.level.ToString();

        int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(summary.elapsedSeconds));
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        if (levelTimeText != null)
            levelTimeText.text = $"{minutes:00}:{seconds:00}";

        if (goldCollectedText != null)
            goldCollectedText.text = $"${summary.goldCollected}";
    }

    public void ContinueToNextLevel()
    {
        if (summary == null)
        {
            Debug.LogError("Cannot continue: no run summary was loaded.", this);
            return;
        }

        PlayerPrefs.SetInt("CurrentLevel", summary.level + 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene(gameplaySceneName);
    }

    public void ExitToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}