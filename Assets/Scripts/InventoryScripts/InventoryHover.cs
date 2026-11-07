using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private InventoryItemDimensions item;
    private HotkeyController hotkeyController;

    private void Awake()
    {
        item = GetComponent<InventoryItemDimensions>();
    }
    void Start()
    {
        hotkeyController = FindAnyObjectByType<HotkeyController>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item == null || hotkeyController == null) return;

        hotkeyController.SetHoveredItem(item);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hotkeyController == null) return;
        
        hotkeyController.ClearHoveredItem(item);
    }
}
