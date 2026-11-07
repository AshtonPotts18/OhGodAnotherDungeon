using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SwitchScreen : MonoBehaviour
{
    public void LoadScene(string sceneN)
    {
        //new WaitForSeconds(1f);
        SceneManager.LoadScene(sceneN);
    }
}
