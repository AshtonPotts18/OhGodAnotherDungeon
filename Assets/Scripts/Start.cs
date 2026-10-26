using UnityEngine;

public class Start : MonoBehaviour
{
    public void Startup()
    {
        PlayerPrefs.SetFloat("Pausing", 0);
        //GameManager.Instance.StartGame();
        //PlayerPrefs.SetBool("Start up");
    }
}
