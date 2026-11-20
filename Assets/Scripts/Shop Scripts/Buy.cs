using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class BuyStation : MonoBehaviour
{
    [Serializable]
    private class StockEntry
    {
        [Tooltip("The existing physical item prefab to display and sell.")]
        public GameObject itemPrefab;

        [Tooltip("Where this item is displayed.")]
        public Transform displayPoint;
    }

    //player
    [SerializeField] private Camera playerCamera;
    [SerializeField] private string playerTag = "Player";
    [SerializeField, Min(0.1f)] private float interactionDistance = 4f;

    //input
    [SerializeField] private InputActionReference interactAction;

    [SerializeField] private StockEntry[] stock;
    [SerializeField] private Transform purchaseSpawnPoint;

    [SerializeField] private GameObject promptObject;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private GameObject shopMoneyObject;
    [SerializeField] private TMP_Text shopMoneyText;

    //Shop Feedback
    [SerializeField] private AudioSource shopAudio;
    [SerializeField] private AudioClip LookAtSound;
    [SerializeField] private AudioClip buySound;
    [SerializeField] private string noInteractPromptLayer = "NoInteractPrompt";


    private readonly Dictionary<Item, StockEntry> availableItems = new Dictionary<Item, StockEntry>();

    private readonly HashSet<Collider> playerColliders = new HashSet<Collider>();

    private Item highlightedItem;
    private Item currentItem;
    private bool playerNearby;

    private void Awake()
    {
        if (promptObject != null)
        {
            promptObject.SetActive(false);
        }
        if (shopMoneyObject != null)
        {
            shopMoneyObject.SetActive(false);
        }
    }

    public void StartBuyStation()
    {
        playerNearby = false;

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        SpawnStock();
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

        UpdateItemHighlight(null);
    }

    private void Update()
    {
        UpdateShopMoneyUI();

        currentItem = null;

        if (playerCamera != null && playerNearby)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
            {
                Item hitItem = hit.collider.GetComponentInParent<Item>();

                if (hitItem != null && availableItems.ContainsKey(hitItem))
                {
                    currentItem = hitItem;
                }
            }
        }
        
        UpdateItemHighlight(currentItem);

        bool showPrompt = playerNearby && currentItem != null;

        if (promptObject == null)
        {
        }
        else
        {
            promptObject.SetActive(showPrompt);

            if (showPrompt)
            {

                if (promptText != null)
                {
                    promptText.text = $"{currentItem.ItemName}\n" + $"Price: ${currentItem.Value}\n" + "Left Click to Buy";
                }
               
            }
        }

    }

    private void SpawnStock()
    {
        if (stock == null) return;

        foreach (StockEntry entry in stock)
        {
            if (entry.itemPrefab == null) continue;

            if (entry.displayPoint == null) continue;

            GameObject display = Instantiate(entry.itemPrefab, entry.displayPoint.position, entry.displayPoint.rotation);
            display.layer = LayerMask.NameToLayer(noInteractPromptLayer);
            
            Item displayItem = display.GetComponent<Item>();

            if (displayItem != null)
            {
                displayItem.isShopItem = true;
            }

            Rigidbody rb = display.GetComponentInChildren<Rigidbody>();

            if (rb != null)
            {
                rb.isKinematic = true;
            }

            Item item = display.GetComponentInChildren<Item>();

            if (item == null) continue;

            availableItems.Add(item, entry);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) || other.transform.root.CompareTag(playerTag))
        {
            playerColliders.Add(other);
            playerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {

        playerColliders.Remove(other);
        playerNearby = playerColliders.Count > 0;

        if (!playerNearby)
        {
            currentItem = null;

            UpdateItemHighlight(null);

            if (promptObject != null)
            {
                promptObject.SetActive(false);
            }
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (!playerNearby || currentItem == null) return;

        if (!availableItems.TryGetValue(currentItem, out StockEntry entry)) return;

        if (Shop.Instance == null) return;

        if (purchaseSpawnPoint == null || entry.itemPrefab == null) return;

        int price = Mathf.Max(0, currentItem.Value);

        if (Shop.Instance.BuyItem(price) == false) return;

        string purchasedName = currentItem.ItemName;
        Item purchasedDisplay = currentItem;

        if (shopAudio != null && buySound != null)
        {
            shopAudio.PlayOneShot(buySound);
        }

        UpdateItemHighlight(null);

        availableItems.Remove(purchasedDisplay);
        currentItem = null;

        Destroy(purchasedDisplay.gameObject);

        GameObject droppedItem = Instantiate(entry.itemPrefab, purchaseSpawnPoint.position, purchaseSpawnPoint.rotation);
        
        GameObject itemManager = GameObject.Find("ItemManager");
        droppedItem.transform.SetParent(itemManager.transform);
        
            Rigidbody rb = droppedItem.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        if (promptObject != null)
        {
            promptObject.SetActive(false);
        }

     }

    private void UpdateShopMoneyUI()
    {
        if (shopMoneyObject == null || shopMoneyText == null) return;

        shopMoneyObject.SetActive(playerNearby);

        if (playerNearby && Shop.Instance != null)
        {
            shopMoneyText.text = $"Money: ${Shop.Instance.PartyMoney}";
        }
    }

    private void UpdateItemHighlight(Item item)
    {
        if(highlightedItem == item) return;

        if(highlightedItem != null)
        {
            ItemHighLight oldHighlight = highlightedItem.GetComponent<ItemHighLight>();

            if (oldHighlight != null)
            {
                oldHighlight.SetHighLighted(false);
            }
        }

        highlightedItem = item;

        if(highlightedItem != null)
        {
            ItemHighLight newHighlight = highlightedItem.GetComponent<ItemHighLight>();

            if(newHighlight != null)
            {
                newHighlight.SetHighLighted(true);
            }

            if (shopAudio != null && LookAtSound != null)
            {
                shopAudio.PlayOneShot(LookAtSound);
            }
        }
    }
}