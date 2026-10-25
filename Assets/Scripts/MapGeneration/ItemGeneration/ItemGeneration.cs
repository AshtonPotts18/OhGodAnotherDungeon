using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemGeneration : MonoBehaviour
{
    public List<GameObject> ItemPool_prefab { get; set; }
    public List<Transform> ItemSpawnLocations { get; set; }
    [SerializeField] private float quotaBuffer = 0.10f;

    private int targetValue = 0;
    private int currentMapItemValue = 0;
    

    public List<GameObject> GenerateItems(int quota)
    {
        targetValue = quota + Mathf.RoundToInt(quota * quotaBuffer);
        List<GameObject> spawnedItems = new List<GameObject>();

        do // this generates all items. Breaks out of loop if there are not enought item spawn location
        {
            GameObject item = GetRandomItemFromPool(out Item itemComponent);
            Transform spawnLocation = GetRandomSpawnLocation();

            item = SpawnItem(item, itemComponent, spawnLocation); //removes the item spawn location from valid spawn locations
            spawnedItems.Add(item);

            if (ItemSpawnLocations.Count < 0)
            {
                Debug.Log("No More Valid Spawn Locations");
                break;
            }

            Console.Write(item + " Spawned at " + item.transform.position);
        } while (targetValue >= currentMapItemValue);


        return spawnedItems;
    }


    private GameObject SpawnItem(GameObject _item, Item _itemComponent, Transform _spawnLocation)
    {
        _item = GameObject.Instantiate(_item, _spawnLocation);
        _item.transform.SetParent(transform, true);
        _item.name = _itemComponent.name;

        ItemSpawnLocations.Remove(_spawnLocation);

        return _item;
    }
    private Transform GetRandomSpawnLocation()
    {
        return ItemSpawnLocations[UnityEngine.Random.Range(0, ItemSpawnLocations.Count)];
    }

    private GameObject GetRandomItemFromPool(out Item itemComponent)
    {
        GameObject outputItem = ItemPool_prefab[UnityEngine.Random.Range(0, ItemPool_prefab.Count)];
        itemComponent = outputItem.GetComponent<Item>();
        return outputItem;
    }
}
