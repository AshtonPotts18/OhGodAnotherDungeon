using System.Collections;
using System.IO;
using UnityEngine;

public class LoadData : MonoBehaviour, IVisitor
{

    private GameData PlayerSaved = new GameData();

    private void OnEnable() // This should only turn on if the Player chooses to load a player so the object must be disabled if not
    {
        PlayerSaved = LoadingData();
    }
    public void VisitHealth(HealthandPlus Health)
    {
        new WaitForSeconds(0.5f);
        Health.slider.value = PlayerSaved.PlayerHP;
    }

    public void VisitShop(Shop shop) // With Norman Make a Load Money Method
    {
        new WaitForSeconds(0.5f);
        //shop.PartyMoney = PlayerSaved.PlayerMoney;
    }
    public GameData LoadingData() //This loads whatever is in the GameData class
    {
        string Path_Of_File = Path.Combine(Application.persistentDataPath, "SavedPlayer.json"); // SavedPlayer.json is the name of the file
        if (File.Exists(Path_Of_File))
        {
            string json_file = File.ReadAllText(Path_Of_File);
            return JsonUtility.FromJson<GameData>(json_file); // returns the GameData
        }
        return null;
    }


}
