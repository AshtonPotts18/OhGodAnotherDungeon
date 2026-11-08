using UnityEngine;

public class LevelContinue : MonoBehaviour
{
    public void Continue_Game()
    {
        PlayerPrefs.SetInt("Delete", 0);
        PlayerPrefs.SetInt("ResetL", 0);
    }
}
