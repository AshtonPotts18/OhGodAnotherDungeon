using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IfSavedData : MonoBehaviour
{
    public GameObject Continue;
    public GameObject button;

    public GameObject NewGame;

    bool disabled = true;
    void Start()
    {
        string Path_Of_File = Path.Combine(Application.persistentDataPath, "SavedPlayer.json");
        if(File.Exists(Path_Of_File))
        {
            Continue.SetActive(true);
            EventSystem.current.SetSelectedGameObject(button);
            disabled = false;
        }
        else
        {
            Continue.SetActive(false);
            disabled = true;
        }
    }

    public void OnEnable()
    {
        if(disabled)
        { EventSystem.current.SetSelectedGameObject(NewGame); }
        else { EventSystem.current.SetSelectedGameObject(button); }
    }
}
