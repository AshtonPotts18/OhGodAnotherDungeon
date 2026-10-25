using UnityEngine;

public class Start : MonoBehaviour
{
    public void Startup()
    {
        PlayerPrefs.SetFloat("Pausing", 0);
    }
}
