using UnityEngine;

public class Start : MonoBehaviour
{

    public void Startup()
    {
        PlayerPrefs.SetFloat("Pausing", 0);
        //GameManager.Instance.StartGame();
        //PlayerPrefs.SetInt("Start up", 1);

    }
}
