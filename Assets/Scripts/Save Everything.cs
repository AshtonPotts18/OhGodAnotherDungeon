using UnityEngine;

public class SaveEverything : MonoBehaviour
{
    public void SaveAll()
    {
        GameObject.Find("SaveData").GetComponent<SaveData>().SaveRemotly();
    }
}
