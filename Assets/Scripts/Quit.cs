using System;
using UnityEngine;

public class Quit : MonoBehaviour
{
    [SerializeField] GameObject quitButton;
    private void Start()
    {
        if(Application.platform == RuntimePlatform.WebGLPlayer)
        {
            quitButton.SetActive(false);
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
