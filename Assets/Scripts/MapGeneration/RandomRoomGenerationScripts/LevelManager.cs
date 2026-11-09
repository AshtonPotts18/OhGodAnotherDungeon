using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{

    public int CurrentLevel { get; set; } = 1;
    private List<Level> levelMap = new List<Level>();


    void Awake()
    {
        FillLevels();
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
        Level currentLevel = GetLevel(CurrentLevel);
        _roomGenerator.roomCount = currentLevel.roomSpawnCount;
        _roomGenerator.roomSpawnRadius = currentLevel.roomSpawnRadius;

        //Shop.Instance.QuotaTarget = currentLevel.quota;
    }
    public void CompleteLevel()
    {
        if (CurrentLevel < 11)
        {
            CurrentLevel += 1;
        }
        else
        {
            // you beat the game
        }
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