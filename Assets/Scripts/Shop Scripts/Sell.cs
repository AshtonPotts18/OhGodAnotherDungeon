using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using TMPro;

public class Sell : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private InputActionReference interactAction;

    [SerializeField] private GameObject promptObject;
    [SerializeField] private TMP_Text promptText;
    private readonly HashSet<Item> itemsInArea = new HashSet<Item>();
    private bool playerNearby;
    
    public int TotalSaleValue
    {
        get
        {
            int total = 0;

            foreach (Item item in itemsInArea)
            {
                if(item != null)
                {
                    total += Mathf.Max(0, item.Value);
                }
            }

            return total;
        }
    }

    private void Start()
    {
        
        if(promptObject != null)
        {
            promptObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.action.started += OnInteract;
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.action.started -= OnInteract;
        }
    }

    private void Update()
    {
        itemsInArea.RemoveWhere(item => item == null);

        bool canSell = playerNearby && itemsInArea.Count > 0;

        if(promptObject != null && promptObject.activeSelf != canSell)
        {
            promptObject.SetActive(canSell);
        }
    }
    
    private void OnInteract(InputAction.CallbackContext context)
    {
        Debug.Log("Sell: Interact pressed!");

        if (!playerNearby)
        {
            Debug.Log("Player is not near the sell area.");
            return;
        }

        itemsInArea.RemoveWhere(item => item == null);

        if (itemsInArea.Count == 0)
        {
            Debug.Log("No items to sell.");
            return;
        }

        ConfirmSale();
    }

    private void OnTriggerEnter(Collider other)
    {
        Item item = other.GetComponentInParent<Item>();

        if (item != null)
        {
            itemsInArea.Add(item);
            Debug.Log($"Item added to sell area: {item.ItemName}");
        }

        if (other.CompareTag(playerTag))
        {
            playerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Item item = other.GetComponentInParent<Item>();

        if(item != null)
        {
            itemsInArea.Remove(item);
            Debug.Log($"Item removed from sell area: {item.ItemName}");
        }

        if (other.CompareTag(playerTag))
        {
            playerNearby = false;
        }
    }

    public void ConfirmSale()
    {
        itemsInArea.RemoveWhere(item => item == null);

        if (!playerNearby || itemsInArea.Count == 0) return;

        if (Shop.Instance == null)
        {
            Debug.LogError("No Shop instance found!");
            return;
        }

        Item[] itemsToSell = new Item[itemsInArea.Count];
        itemsInArea.CopyTo(itemsToSell);

        if (Shop.Instance.SellItems(itemsToSell))
        {
            itemsInArea.Clear();
        }
    }

}
