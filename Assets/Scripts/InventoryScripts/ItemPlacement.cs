using System.Collections.Generic;
using UnityEngine;

public class ItemPlacement : MonoBehaviour, IVisitable
{
    //Saves
    SaveData PlayerSave;  //Player save Data

    LoadData PlayerData;  // Load Player Data;


    //Grid settings
    [SerializeField] private int columns = 3;
    [SerializeField] private int rows = 3;
    public Vector2 cellSize = new Vector2(50, 50);
    public Vector2 spacing = new Vector2(2, 2);

    //references
    [SerializeField] private RectTransform itemLayer;

    private InventoryItemDimensions[,] fullCells;
    public static ItemPlacement Instance { get; private set; }

    public List<InventoryData> listofitems;

    private void OnEnable()
    {
        Instance = this;
    }

    private void OnDisable()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    private void Awake()
    {
        columns = Mathf.Max(1, columns);
        rows = Mathf.Max(1, rows);

        fullCells = new InventoryItemDimensions[columns, rows];

        if (GameObject.Find("SaveData") != null)
        {
            PlayerSave = GameObject.Find("SaveData").GetComponent<SaveData>(); //gets the Player Save Data
        }

        if (PlayerPrefs.GetInt("Deleted") == 0)
        { 
            Load();
        }
    }

    public bool CanPlace(InventoryItemDimensions item, int x, int y, bool rotated)
    {
        if (item == null || fullCells == null)
        {
            return false;
        }

        int width = rotated ? item.height : item.width;
        int height = rotated ? item.width : item.height;

        if (width < 1 || height < 1)
        {
            return false;
        }

        if (x < 0 || y < 0 || x + width > columns || y + height > rows)
        {
            return false;
        }

        for (int dx = 0; dx < width; dx++)
        {
            for (int dy = 0; dy < height; dy++)
            {
                InventoryItemDimensions occupant = fullCells[x + dx, y + dy];

                if (occupant != null && occupant != item)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public bool PlaceItem(InventoryItemDimensions item, int x, int y, bool rotated = false)
    {
        if (item == null || itemLayer == null || fullCells == null)
        {
            return false;
        }

        if (item.RectTransform == null)
        {
            Debug.LogError("Cannot place item");
            return false;
        }

        if (!CanPlace(item, x, y, rotated))
        {
            return false;
        }

        if (item.isPlaced)
        {
            ClearItemCells(item);
        }

        item.SetGridPosition(x, y);
        item.SetRotation(rotated);
        item.isPlaced = true;

        int width = item.Width;
        int height = item.Height;

        for (int dx = 0; dx < width; dx++)
        {
            for (int dy = 0; dy < height; dy++)
            {
                fullCells[x + dx, y + dy] = item;
            }
        }

        RectTransform rect = item.RectTransform;

        rect.SetParent(itemLayer, false);

        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.localScale = Vector3.one;

        rect.anchoredPosition = new Vector2(x * (cellSize.x + spacing.x), -y * (cellSize.y + spacing.y));

        rect.sizeDelta = new Vector2(width * cellSize.x + (width - 1) * spacing.x, height * cellSize.y + (height - 1) * spacing.y);

        return true;
    }

    private void ClearItemCells(InventoryItemDimensions item)
    {
        if (item == null || fullCells == null) return;

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                if (fullCells[x, y] == item)
                {
                    fullCells[x, y] = null;
                }
            }
        }
    }

    public bool RotatedItem(InventoryItemDimensions item)
    {
        if (item == null || !item.isPlaced)
        {
            return false;
        }

        return PlaceItem(item, item.gridX, item.gridY, !item.isRotated);
    }

    public InventoryItemDimensions GetItemAt(int x, int y)
    {
        if (fullCells == null || x < 0 || y < 0 || x >= columns || y >= rows)
        {
            return null;
        }

        return fullCells[x, y];
    }

    public void RemoveItemFromGrid(InventoryItemDimensions item)
    {
        if (item == null || !item.isPlaced) return;

        ClearItemCells(item);
        item.isPlaced = false;
    }

    public bool IsInsideGrid(
        Vector2 screenPosition, Camera uiCamera)
    {
        if (itemLayer == null)
        {
            return false;
        }

        return RectTransformUtility.RectangleContainsScreenPoint(itemLayer,screenPosition,uiCamera);
    }

    public bool TryReturnItemToGrid(InventoryItemDimensions item)
    {
        if (item == null)
        {
            return false;
        }

        if (CanPlace(item, item.gridX, item.gridY, item.isRotated))
        {
            if (PlaceItem(item, item.gridX, item.gridY, item.isRotated))
            {
                item.gameObject.SetActive(true);
                return true;
            }
        }

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                if (CanPlace(item, x, y, item.isRotated))
                {
                    if (PlaceItem(item, x, y, item.isRotated))
                    {
                        item.gameObject.SetActive(true);
                        return true;
                    }
                }
            }
        }

        return false;
    }
    public void Accept(IVisitor visitor)
    {
        visitor.VisitItems(this);
    }

    public void SaveInv()
    {
        //if (itemLayer == null)
        //{
        //    Debug.LogError("ItemPlacement: Item Layer is not assigned.", this);
        //}

        var Inven = gameObject.GetComponent<IVisitable>();
        if (PlayerSave != null)
        {                                                       //Needs set condition to not be recorded every second!!
            Inven.Accept(PlayerSave);                           // This row as well.
        }

        
    }


    InventoryData SaveingInventoryintoData(InventoryItemDimensions Cells)
    {
        InventoryData temp = new InventoryData();

        temp.ItemType = Cells.gameObject.name;
        temp.Width = Cells.width;
        temp.Height = Cells.height;
        temp.x = Cells.gridX;
        temp.y = Cells.gridY;
        temp.isPlaced = Cells.isPlaced;
        temp.isRotated = Cells.isRotated;
        return temp;
    }

    void OnDestroy()
    {
        int counter = 0;
        List<Vector2> path = new List<Vector2>();
        int Coordscounter = 0;
        for (int i = 0; i < 3; i++)
        {
            for (int f = 0; f < 3; f++)
            {
                if (Coordscounter != 0)
                {
                    for (int j = 0; j < Coordscounter; j++)
                    {
                        if (path[j] == new Vector2(f, i))
                        {
                            f++;
                        }

                    }

                }
                if (f == 3)
                {
                    break;
                }
                if (fullCells[f, i] != null)
                {
                    listofitems.Add(SaveingInventoryintoData(fullCells[f, i]));
                    Debug.Log(listofitems[counter].ItemType);
                    counter++;
                    if (fullCells[f, i].Width == 2 && fullCells[f, i].Height != 2)
                    {
                        f++;
                    }
                    else if (fullCells[f, i].Height == 2 && fullCells[f, i].Width != 2)
                    {
                        path.Add(new Vector2(f, i + 1));
                        Coordscounter++;

                    }
                    else if (fullCells[f, i].Width == 2 && fullCells[f, i].Height == 2)
                    {
                        path.Add(new Vector2(f, i + 1));
                        path.Add(new Vector2(f + 1, i + 1));
                        f++;
                        Coordscounter += 2;
                        
                        Debug.Log("2x2 inventory slot");
                    }
                }
            }
        }

        SaveInv();
    }

    public void Load()
    {
        var Inven = gameObject.GetComponent<IVisitable>();

        if (GameObject.Find("LoadData") != null)
        {
            PlayerData = GameObject.Find("LoadData").GetComponent<LoadData>();
        }
        if (PlayerData != null)
        {
            //new WaitForSeconds(0.5f);
            Inven.Accept(PlayerData);
        }
    }
}