using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour, IVisitable
{

    SaveData PlayerSave;  //Player save Data

    LoadData PlayerData;
    public int CurrentLevel { get; set; } = 1;
    private List<Level> levelMap = new List<Level>();

    public int finalLevel;

    void Awake()
    {
        FillLevels();
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GameObject.Find("SaveData"))
        {
            PlayerSave = GameObject.Find("SaveData").GetComponent<SaveData>(); //gets the Player Save Data

            var Level = gameObject.GetComponent<IVisitable>();
            if (PlayerSave != null)
            {  //Needs set condition to not be recorded every second!!
                Level.Accept(PlayerSave);                           // This row as well.
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FillLevels()
    {
        levelMap.Add(new Level(10, 3, 100));
        levelMap.Add(new Level(15, 5, 120));
        levelMap.Add(new Level(15, 7, 160));
        levelMap.Add(new Level(20, 7, 200));
        levelMap.Add(new Level(20, 9, 300));
        levelMap.Add(new Level(20, 10, 340));
        levelMap.Add(new Level(23, 12, 400));
        levelMap.Add(new Level(23, 12, 500));
        levelMap.Add(new Level(23, 14, 550));
        levelMap.Add(new Level(23, 15, 600));
        levelMap.Add(new Level(23, 15, 650));


        finalLevel = levelMap.Count + 1;
    }

    public Level GetLevel(int _level)
    {
        if (_level > levelMap.Count)
        {
            Debug.LogError("Attempt to Load Level Out Of Range");
            return new Level();
        }

        return levelMap[_level - 1];

    }
    public void LoadLevel(RandomlyGenerateRooms _roomGenerator)
    {
        if (GameObject.Find("LoadData"))
        {
            PlayerData = GameObject.Find("LoadData").GetComponent<LoadData>();
        }

        var Level = gameObject.GetComponent<IVisitable>();
        if (PlayerData != null)
        {
            Level.Accept(PlayerData);
        }

        Level currentLevel = GetLevel(CurrentLevel);
        _roomGenerator.roomCount = currentLevel.roomSpawnCount;
        _roomGenerator.roomSpawnRadius = currentLevel.roomSpawnRadius;

        Shop.Instance.QuotaPerLevel(currentLevel.quota);
        Shop.Instance.TotalReset();
    }
    public void CompleteLevel()
    {
        if (CurrentLevel < 11)
        {
            CurrentLevel += 1;

            if (GameObject.Find("SaveData"))
            {
                PlayerSave = GameObject.Find("SaveData").GetComponent<SaveData>(); //gets the Player Save Data

                var Level = gameObject.GetComponent<IVisitable>();
                if (PlayerSave != null)
                {  //Needs set condition to not be recorded every second!!
                    Level.Accept(PlayerSave);                           // This row as well.
                }
            }
        }
        else
        {
            // you beat the game
        }
    }

    public void Accept(IVisitor visitor)
    {
       visitor.VisitLevel(this);
    }
}
[System.Serializable]
public class Level
{
    public int roomSpawnRadius;
    public short roomSpawnCount;
    public int quota;

    public Level()
    {     }

    public Level(int _roomSpawnRadius, short _roomSpawnCount, int _quota)
    {
        roomSpawnRadius = _roomSpawnRadius;
        roomSpawnCount = _roomSpawnCount;
        quota = _quota;
    }





}