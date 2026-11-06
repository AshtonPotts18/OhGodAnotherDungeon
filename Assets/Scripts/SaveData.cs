using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;


public interface IVisitable
{
    void Accept(IVisitor visitor);
}

public interface IVisitor // for the visitor function here I want the player info to be put into this so it can visit the scripts
{
    void VisitHealth(HealthandPlus Health);
    void VisitShop(Shop Shop);

    void VisitItems(ItemPlacement it);
}

public class SaveData : MonoBehaviour, IVisitor
{
    //
    // in Health and Plus uncomment the work in it when finished
    //
    GameData Player = new GameData();

    private static SaveData Instance;

   void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    //This is the Way PlayerHP gets saved to Player Game Data
    public void VisitHealth(HealthandPlus Health)
    {
        Player.PlayerHP = Health.slider.value;
        Debug.Log(Player.PlayerHP);
    }

    public void VisitShop(Shop Shop)
    {
        Player.PlayerMoney = Shop.PartyMoney;
    }

    public void VisitItems(ItemPlacement it)
    {
        Player.Inv = it.listofitems;
        if(Player.Inv.Count > 0)
        { 
            string t = "Player items: " + Player.Inv[0].ItemType;
            Debug.Log(t);
        }
        
    }

    public void SavingData(GameData PlayerData) //This saves whatever is in the GameData class
    {
        Task.Delay(500);
        string path_of_file = Path.Combine(Application.persistentDataPath, "SavedPlayer.json"); // SavedPlayer.json is the name of the file
        Debug.Log(path_of_file);
        File.WriteAllText(path_of_file, JsonUtility.ToJson(PlayerData));
        Debug.Log(Player.PlayerHP);

        Destroy(gameObject, 1.0f);
    }

    void Start()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;
        
    }

    private void OnSceneChanged(Scene current, Scene next)
    {
        if(next.name == "Main Menu" && current.name == "SampleScene")
        { SavingData(Player); }
    }
}




[System.Serializable]
public class GameData
{
    public float PlayerHP;
    public int PlayerMoney;
    public int SavedLevel;
    public List<InventoryData> Inv;
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
