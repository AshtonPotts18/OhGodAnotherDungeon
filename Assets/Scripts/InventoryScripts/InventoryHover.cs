using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryHover : MonoBehaviour
{
    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private Text itemName;
    [SerializeField] private Text itemDescription;

    [SerializeField] private HotkeyController hotkeyController;

    private InventoryItemDimensions currentItem;

    private PointerEventData pointerData;
    private readonly List<RaycastResult> raycastResults = new();

    private void Start()
    {
        if (descriptionPanel != null)
        {
            descriptionPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (hotkeyController == null)
        {
            hotkeyController = FindAnyObjectByType<HotkeyController>();

        }

        DetectItem();
    }

    private void DetectItem()
    {
        if (EventSystem.current == null || Mouse.current == null)
            return;

        pointerData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };

        raycastResults.Clear();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        InventoryItemDimensions hoveredItem = null;

        foreach (RaycastResult result in raycastResults)
        {
            hoveredItem = result.gameObject.GetComponentInParent<InventoryItemDimensions>();

            if (hoveredItem != null) break;
        }

        if (hoveredItem == currentItem) return;

        if (currentItem != null && hotkeyController != null)
        {
            hotkeyController.ClearHoveredItem(currentItem);
        }

        currentItem = hoveredItem;

        if (currentItem != null)
        {
            ShowDescription(currentItem);
        }
        else
        { 
            HideDescription();
        }
    }

    private void ShowDescription(InventoryItemDimensions item)
    { 
        if (hotkeyController != null)
        {
            hotkeyController.SetHoveredItem(item);
        }

        if (item.originalItem == null) return;

        if (descriptionPanel != null)
        {
            descriptionPanel.SetActive(true);
        }

        if (itemName != null)
        {
            itemName.text = "Name: " + item.originalItem.ItemName + "\nValue: " + item.originalItem.Value;
        }

        if (itemDescription != null)
        {
            itemDescription.text = item.originalItem.ItemDescription;
        }

    }

    private void HideDescription()
    {
        if (descriptionPanel != null)
        {
            descriptionPanel.SetActive(false);
        }
    }

}