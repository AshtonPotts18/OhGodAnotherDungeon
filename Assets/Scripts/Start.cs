using UnityEngine;

public class Start : MonoBehaviour
{
    public void Startup()
    {
        GameManager.Instance.StartGame();
        //PlayerPrefs.SetBool("Start up");
        PlayerPrefs.SetFloat("Pausing", 0);
    }
}
