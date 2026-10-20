using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InventoryDragController : MonoBehaviour
{
    //References
    [SerializeField] private PlayerMovement cameraLook;
    [SerializeField] private ItemPlacement inventory;
    [SerializeField] private RectTransform itemLayer;
    [SerializeField] private Canvas canvas;

    //Drop settings
    [SerializeField] private LayerMask dropLayer = ~0;
    [SerializeField] private float dropRange = 3f;

    private Item heldItem;

    public bool isHolding => heldItem != null;

    private Camera UiCamera
    {
        get
        {
            if(canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return null;
            }
            return canvas.worldCamera;
        }
    }

    public bool PickupItem(Item item)
    {
        if(item == null)
        {
            Debug.LogWarning("Cannot pickup null item");
            return false;
        }
        
        if(isHolding)
        {
            Debug.LogWarning("Already holding an inventory item.");
            return false;
        }
        
        if(inventory == null || itemLayer == null)
        {
            Debug.LogError("no inventory or itemlayer assigned");
            return false;
        }
        
        BeginHolding(item);
        
        return isHolding;
    }
    
    public void BeginMoveItem(Item item)
    {
        if (item == null || isHolding || !item.isPlaced) return;

        if (inventory == null || itemLayer == null)
        {
            Debug.LogError("Inventory or Item Layer is not assigned.");
            return;
        }

        inventory.RemoveItemFromGrid(item);

        BeginHolding(item);
    }

    private void BeginHolding(Item item)
    {
        if (item == null || inventory == null || itemLayer == null) return;

        heldItem = item;
        heldItem.isPlaced = false;

        RectTransform rect = heldItem.RectTransform;

        if (rect == null)
        {
            Debug.LogError("Inventory item has no RectTransform.");
            heldItem = null;
            return;
        }

        rect.SetParent(itemLayer, false);

        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.localScale = Vector3.one;

        UpdateItemSize();
        UpdateHeldItemPosition();
    }

    private void Update()
    {
        if (heldItem == null)
        {
            TryPickUpInventoryItem();
        }

        if (heldItem == null || Mouse.current == null) return;

        UpdateHeldItemPosition();

        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            heldItem.SetRotation(!heldItem.isRotated);
            UpdateItemSize();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            FinishDragging();
        }
    }

    private void UpdateItemSize()
    {
        if (heldItem == null || inventory == null) return;

        RectTransform rect = heldItem.RectTransform;

        if (rect == null) return;

        rect.sizeDelta = new Vector2(heldItem.Width * inventory.cellSize.x + (heldItem.Width - 1) * inventory.spacing.x, heldItem.Height * inventory.cellSize.y + (heldItem.Height - 1) * inventory.spacing.y);
    }

    private void UpdateHeldItemPosition()
    {
        if (itemLayer == null || heldItem == null || Mouse.current == null) return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(itemLayer, Mouse.current.position.ReadValue(), UiCamera, out Vector2 localPoint))
        {
            return;
        }

        RectTransform rect = heldItem.RectTransform;

        if (rect == null)return;

        rect.anchoredPosition = localPoint;
    }

    private void FinishDragging()
    {
        if (heldItem == null || inventory == null || Mouse.current == null) return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        if (inventory.IsInsideGrid(mousePosition, UiCamera))
        {
            TryPlaceHeldItem();
        }
        else
        {
            DropIntoWorld(mousePosition);
        }
    }

    private void TryPlaceHeldItem()
    {
        if (heldItem == null || inventory == null) return;

        RectTransform rect = heldItem.RectTransform;

        if (rect == null) return;

        float stepX = inventory.cellSize.x + inventory.spacing.x;
        float stepY = inventory.cellSize.y + inventory.spacing.y;

        if (stepX <= 0 || stepY <= 0) return;

        int x = Mathf.RoundToInt(rect.anchoredPosition.x / stepX);
        int y = Mathf.RoundToInt(-rect.anchoredPosition.y / stepY);

        if (inventory.PlaceItem(heldItem, x, y, heldItem.isRotated))
        {
            heldItem = null;
        }
    }

    private void DropIntoWorld(Vector2 mousePosition)
    {
        if (heldItem == null)return;

        GameObject prefab = heldItem.worldPrefab;

        if (prefab == null)
        {
            Debug.LogWarning("Cannot drop item: assign its World Prefab.");
            return;
        }

        Camera worldCamera = Camera.main;

        if (worldCamera == null)
        {
            Debug.LogError("Cannot drop item: no Main Camera.");
            return;
        }

        Ray ray = worldCamera.ScreenPointToRay(mousePosition);

        Vector3 dropPosition;

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, dropLayer))
        {
            dropPosition = hit.point;
        }
        else
        {
            dropPosition = ray.GetPoint(dropRange);
        }

        Instantiate(prefab, dropPosition, prefab.transform.rotation);

        Destroy(heldItem.gameObject);
        heldItem = null;
    }
    public void TryPickUpInventoryItem()
    {
        if (Mouse.current == null || EventSystem.current == null) return;

        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        PointerEventData pointerData = new PointerEventData(EventSystem.current);

        pointerData.position = Mouse.current.position.ReadValue();

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            Item item = result.gameObject.GetComponentInParent<Item>();

            if (item != null && item.isPlaced)
            {
                BeginMoveItem(item);
                return;
            }
        }
    }
    public void SetInventoryOpen(bool isOpen)
    {
        //if (cameraLook != null)
        //{
        //    cameraLook.cameraLookLocked = isOpen;
        //}

        cameraLook = FindAnyObjectByType<PlayerMovement>();

        if (cameraLook == null)
        {
            Debug.LogError("Could not find the spawned player's PlayerMovement.");
            return;
        }

        cameraLook.cameraLookLocked = isOpen;
        Debug.Log($"Inventory open: {isOpen}");
    }
}
   


