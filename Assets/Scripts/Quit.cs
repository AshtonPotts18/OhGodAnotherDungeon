using UnityEngine;

public class Quit : MonoBehaviour
{
    private void Start()
    {
        if(Application.platform == RuntimePlatform.WebGLPlayer)
        {
            gameObject.SetActive(false);
        }
    }
   public void QuitButton()
    {

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
