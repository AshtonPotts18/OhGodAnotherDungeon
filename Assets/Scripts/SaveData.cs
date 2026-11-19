using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;


public interface IVisitable
{
    void Accept(IVisitor visitor);
}

public interface IVisitor // for the visitor function here I want the player info to be put into this so it can visit the scripts
{
    void VisitHealth(HealthandPlus Health);
    void VisitShop(Shop Shop);

    void VisitLevel(LevelManager Level);
    void VisitItems(ItemPlacement it);
}

public class SaveData : MonoBehaviour, IVisitor
{
    //
    // in Health and Plus uncomment the work in it when finished
    //

    public bool Delete = false;
    GameData Player = new GameData();

    public static SaveData Instance { get; private set; }

   void Awake()
    {

        if (PlayerPrefs.HasKey("Delete"))
        {
            if (PlayerPrefs.GetInt("Delete") == 1)
            {
                EraseData(Player);
                PlayerPrefs.SetInt("Delete", 0);
            }
            else
            {
                Player = GameObject.Find("LoadData").GetComponent<LoadData>().LoadingData();
            }
        }
        else
        {
            Player = GameObject.Find("LoadData").GetComponent<LoadData>().LoadingData();
        }

       


        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

    }

    void Start()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    //This is the Way PlayerHP gets saved to Player Game Data
    public void VisitHealth(HealthandPlus Health)
    {
        Player.PlayerHP = Health.slider.value;
        //Debug.Log(Player.PlayerHP);
    }

    public void VisitShop(Shop Shop)
    {
        Player.PlayerMoney = Shop.PartyMoney;
    }

    public void VisitLevel(LevelManager Level)
    {
        Player.SavedLevel = Level.CurrentLevel;
    }

    public void VisitItems(ItemPlacement it)
    {
        Player.Inv = it.listofitems;
        if(Player.Inv.Count > 0)
        { 
            for(int i = 0; i < Player.Inv.Count; i++)
            {
                Player.Inv[i].ItemType = Player.Inv[i].ItemType.Replace("(Clone)", "");
            }
            string t = "Player items: " + Player.Inv[0].ItemType;
            Debug.Log(t);
        }
        
    }

    public void SavingData(GameData PlayerData) //This saves whatever is in the GameData class
    {
        string path_of_file = Path.Combine(Application.persistentDataPath, "SavedPlayer.json"); // SavedPlayer.json is the name of the file
        //if (System.IO.File.Exists(path_of_file))
        //{
        //    System.IO.File.Delete(path_of_file);
        //}

        Debug.Log(path_of_file);
        File.WriteAllText(path_of_file, JsonUtility.ToJson(PlayerData));
        Debug.Log(Player.PlayerHP);
        PlayerPrefs.SetInt("Delete", 0);

    }

    public void EraseData(GameData PlayerData)
    {
        string path_of_file = Path.Combine(Application.persistentDataPath, "SavedPlayer.json");
        if (File.Exists(path_of_file))
        {
            File.Delete(path_of_file);
        }
        PlayerData = new GameData();
    }

    public void SaveRemotly()
    {
        SavingData(Player);
    }

    private void OnSceneChanged(Scene current, Scene next)
    {

        if(next.name == "Game Over")
        { EraseData(Player); }
        else if(next.name == "LevelTransition")
        {
            SavingData(Player); 
        }
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }
}




[System.Serializable]
public class GameData
{
    public float PlayerHP = 100;
    public int PlayerMoney = 0;
    public int SavedLevel = 1;
    public List<InventoryData> Inv = new List<InventoryData>();
}


[System.Serializable]
public class InventoryData
{
    public string ItemType;
    public int Width;
    public int Height;
    public int x, y;
    public bool isPlaced;
    public bool isRotated;
}
