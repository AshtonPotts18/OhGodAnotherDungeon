using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{

    [SerializeField] private GameObject PauseMenu;
    [SerializeField] private GameObject OtherUI;


    [SerializeField] private InputActionAsset ActionMap;
    [SerializeField] private float timer = 1;

    [SerializeField] private bool Paused;

    private bool tap = true;
    private float falsesafe; // this is for the quit when so the Pause won't always be false in the update;
    private InputAction Escape;

    private void Awake()
    {
        Escape = ActionMap.FindAction("Escape");
    }

    IEnumerator WaitAndDoSomething()
    {
        // Pause execution for set seconds
        tap = false;
        if (PauseMenu.activeSelf)
        {
            PauseMenu.SetActive(false);
            OtherUI.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1;
            Paused = false;
        }
        else
        {
            PauseMenu.SetActive(true);
            OtherUI.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0;
            Paused = true;
        }
        yield return new WaitForSecondsRealtime(timer);

        tap = true;
    }
    // Update is called once per frame
    void Update()
    {
        if (Escape.IsPressed() && tap == true)
        {

            StartCoroutine(WaitAndDoSomething());

        }

        if(Paused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (PlayerPrefs.HasKey("Pausing"))
        {
            float val = PlayerPrefs.GetFloat("Pausing");

            falsesafe = val;
        }
        else
        {

        }

        if(falsesafe == 0)
        {
            Paused = false;
            PlayerPrefs.SetFloat("Pausing", 1);
            Time.timeScale = 1;

        }

    }


    public void ClickPause()
    {
        PauseMenu.SetActive(false);
        OtherUI.SetActive(true);
        falsesafe = 0;
    }

    
}
