using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;


public class LoadData : MonoBehaviour, IVisitor
{

    private GameData PlayerSaved = new GameData();

    public static LoadData Instance { get; private set; }
    void Awake() // This should only turn on if the Player chooses to load a player so the object must be disabled if not
    {
        string Path_Of_File = Path.Combine(Application.persistentDataPath, "SavedPlayer.json"); // SavedPlayer.json is the name of the file
        if (File.Exists(Path_Of_File)) 
        {
            if(PlayerPrefs.HasKey("Delete"))
            {
                if(PlayerPrefs.GetInt("Delete") == 1)
                {
                    PlayerSaved = new GameData();
                }
                else
                {
                    PlayerSaved = LoadingData();
                }
            }
            else
            {
                PlayerSaved = LoadingData();
            }


        }
    }
    public void VisitHealth(HealthandPlus Health)
    {
        Health.slider.value = PlayerSaved.PlayerHP;
    }

    public void VisitLevel(LevelManager Level)
    {
        string Path_Of_File = Path.Combine(Application.persistentDataPath, "SavedPlayer.json");

        if (File.Exists(Path_Of_File))
        { PlayerSaved = LoadingData(); }

        Level.CurrentLevel = PlayerSaved.SavedLevel;
    }

    public void VisitItems(ItemPlacement Inv)
    {
        for(int i = 0; i < PlayerSaved.Inv.Count; i++)
        {
            InventoryItemDimensions temp = new InventoryItemDimensions();
            GameObject Object = Resources.Load<GameObject>("Prefabs/Items/" + PlayerSaved.Inv[i].ItemType);
            temp.worldPrefab = Object;
            temp.width = PlayerSaved.Inv[i].Width;
            temp.height = PlayerSaved.Inv[i].Height;
            temp.gridX = PlayerSaved.Inv[i].x;
            temp.gridY = PlayerSaved.Inv[i].y;
            temp.isPlaced = false;
            temp.isRotated = PlayerSaved.Inv[i].isRotated;

            Inv.PlaceItem(temp, PlayerSaved.Inv[i].x, PlayerSaved.Inv[i].y, PlayerSaved.Inv[i].isPlaced);
        }
    }

    public void VisitShop(Shop shop) // With Norman Make a Load Money Method
    {
        shop.AddMoney(PlayerSaved.PlayerMoney);
    }
    public GameData LoadingData() //This loads whatever is in the GameData class
    {
        string Path_Of_File = Path.Combine(Application.persistentDataPath, "SavedPlayer.json"); // SavedPlayer.json is the name of the file
        if (File.Exists(Path_Of_File))
        {
            string json_file = File.ReadAllText(Path_Of_File);
            return JsonUtility.FromJson<GameData>(json_file); // returns the GameData
        }
        return new GameData();
    }


}
