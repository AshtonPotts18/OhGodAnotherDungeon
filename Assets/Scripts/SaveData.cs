using UnityEngine;
using System.IO;
using System.Collections;


public interface IVisitable
{
    void Accept(IVisitor visitor);
}

public interface IVisitor // for the visitor function here I want the player info to be put into this so it can visit the scripts
{
    void VisitHealth(HealthandPlus Health);
    void VisitShop(Shop Shop);
}

public class SaveData : MonoBehaviour, IVisitor
{
    //
    // in Health and Plus uncomment the work in it when finished
    //
    GameData Player = new GameData();

    //This is the Way PlayerHP gets saved to Player Game Data
    public void VisitHealth(HealthandPlus Health)
    {
        Player.PlayerHP = Health.slider.value;
    }

    public void VisitShop(Shop Shop)
    {
        Player.PlayerMoney = Shop.PartyMoney;
    }

    IEnumerator DelayedAction() //This delays the function so it can be saved correctly
    {
        
        yield return new WaitForSeconds(0.5f); // Waits for 0.5 seconds
        SavingData(Player);
    }

    public void SavingData(GameData PlayerData) //This saves whatever is in the GameData class
    {
        string path_of_file = Path.Combine(Application.persistentDataPath, "SavedPlayer.json"); // SavedPlayer.json is the name of the file
        Debug.Log(path_of_file);
        File.WriteAllText(path_of_file, JsonUtility.ToJson(PlayerData));
    }


    

    
    void Start()
    {
        Player.SavedLevel = 0;

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