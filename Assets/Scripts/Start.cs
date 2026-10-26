using UnityEngine;

public class Start : MonoBehaviour
{
    public void Startup()
    {
        GameManager.Instance.StartGame();
        PlayerPrefs.SetFloat("Pausing", 0);
    }
}
