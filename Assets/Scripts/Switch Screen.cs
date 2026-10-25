using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScreen : MonoBehaviour
{
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void LoadScene(string sceneN)
    {
        Task.Delay(500);
        SceneManager.LoadScene(sceneN);
    }
}
