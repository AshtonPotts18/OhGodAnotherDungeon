using UnityEngine;
using System.IO;
using System.Collections;


public interface IVisitable
{
    void Accept(IVisitor visitor);
}

public interface IVisitor
{
    void Visit(HealthandPlus Health);
}

public class SaveData : MonoBehaviour, IVisitor
{
    //
    // in Health and Plus uncomment the work in it when finished
    //
    GameData Player = new GameData();

    //This is the Way PlayerHP gets saved to Player Game Data
    public void Visit(HealthandPlus Health)
    {
        Player.PlayerHP = Health.slider.value;
    }

    IEnumerator DelayedAction()
    {
        
        yield return new WaitForSeconds(0.5f); // Waits for 0.5 seconds
        SavingData(Player);
    }

    public void SavingData(GameData PlayerData)
    {
        string path_of_file = Path.Combine(Application.persistentDataPath, "SavedPlayer.json"); // SavedPlayer.json is the name of the file
        Debug.Log(path_of_file);
        File.WriteAllText(path_of_file, JsonUtility.ToJson(PlayerData));
        Debug.Log(LoadingData().PlayerHP);
    }


    public GameData LoadingData()
    {
        string Path_Of_File = Path.Combine(Application.persistentDataPath, "SavedPlayer.json"); // SavedPlayer.json is the name of the file
        if (File.Exists(Path_Of_File))
        {
            string json_file = File.ReadAllText(Path_Of_File);
            return JsonUtility.FromJson<GameData>(json_file); // returns the GameData
        }
        return null;
    }

    
    void Start()
    {
        Player.SavedLevel = 0;

        Player.PlayerMoney = 100;

        StartCoroutine(DelayedAction());
    }

}




[System.Serializable]
public class GameData
{
    public float PlayerHP;
    public int PlayerMoney;

    public int SavedLevel;
}