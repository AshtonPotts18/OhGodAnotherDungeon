using UnityEngine;

public class Quit : MonoBehaviour
{
   public void QuitButton()
    {

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
