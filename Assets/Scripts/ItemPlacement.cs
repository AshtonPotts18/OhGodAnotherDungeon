using UnityEngine;

public class ItemPlacement : MonoBehaviour
{
    //Grid settings
    [SerializeField] private int columns = 3;
    [SerializeField] private int rows = 3;
    public Vector2 cellSize = new Vector2(50, 50);
    public Vector2 spacing = new Vector2(2, 2);

    //references
    [SerializeField] private RectTransform itemLayer;

    private DisplayedInventoryItem[,] fullCells;

    private void Awake()
    {
        columns = Mathf.Max(1, columns);
        rows = Mathf.Max(1, rows);

        fullCells = new DisplayedInventoryItem[columns, rows];

        if (itemLayer == null)
        {
            Debug.LogError("ItemPlacement: Item Layer is not assigned.", this);
        }
    }

    public bool CanPlace(DisplayedInventoryItem item, int x, int y, bool rotated)
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
                DisplayedInventoryItem occupant = fullCells[x + dx, y + dy];

                if (occupant != null && occupant != item)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public bool PlaceItem(DisplayedInventoryItem item, int x, int y, bool rotated = false)
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

    private void ClearItemCells(DisplayedInventoryItem item)
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

    public bool RotatedItem(DisplayedInventoryItem item)
    {
        if (item == null || !item.isPlaced)
        {
            return false;
        }

        return PlaceItem(item, item.gridX, item.gridY, !item.isRotated);
    }

    public DisplayedInventoryItem GetItemAt(int x, int y)
    {
        if (fullCells == null || x < 0 || y < 0 || x >= columns || y >= rows)
        {
            return null;
        }

        return fullCells[x, y];
    }

    public void RemoveItemFromGrid(DisplayedInventoryItem item)
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
}