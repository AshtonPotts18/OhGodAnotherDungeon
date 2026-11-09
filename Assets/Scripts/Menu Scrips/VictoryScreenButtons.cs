using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryScreenButtons : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] string mainMenuScreen = "Main Mainu";

    public void OnMainMenu()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.EndGame();
        SceneManager.LoadScene("Main Menu");
    }
}
