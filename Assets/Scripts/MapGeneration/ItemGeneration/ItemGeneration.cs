using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemGeneration
{
    [SerializeField] private List<GameObject> itemPool_prefab;
    [SerializeField] private List<Transform> itemSpawnLocations;

    //private int quota;
    private int targetValue = 0;
    private int currentMapItemValue = 0;


    public List<Item> GenerateItems(int quota)
    {
        targetValue = Mathf.RoundToInt(quota * 0.10f);

        do
        {
            GameObject item = GetRandomItemFromPool(out Item itemComponent);
            Transform spawnLocation = GetRandomSpawnLocation();

            SpawnItem(item, itemComponent, spawnLocation);
            




        } while (targetValue >= currentMapItemValue);

        return new List<Item>();
    }


    private void SpawnItem(GameObject _item, Item _itemComponent, Transform _spawnLocation)
    {
        //GameObject item = Instantiate();
    }
    private Transform GetRandomSpawnLocation()
    {
        return itemSpawnLocations[UnityEngine.Random.Range(0, itemSpawnLocations.Count)];
    }

    private GameObject GetRandomItemFromPool(out Item itemComponent)
    {
        GameObject outputItem = itemPool_prefab[UnityEngine.Random.Range(0, itemPool_prefab.Count)];
        itemComponent = outputItem.GetComponent<Item>();
        return outputItem;
    }
}
