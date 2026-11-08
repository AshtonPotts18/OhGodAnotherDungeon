using UnityEngine;

public class NewGame : MonoBehaviour
{
   public void New_Game()
    {
        PlayerPrefs.SetInt("Delete", 1);
        PlayerPrefs.SetInt("ResetL", 1);
    }
}
