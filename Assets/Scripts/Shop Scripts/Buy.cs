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

    private readonly Dictionary<Item, StockEntry> availableItems = new Dictionary<Item, StockEntry>();

    private readonly HashSet<Collider> playerColliders = new HashSet<Collider>();

    private Item currentItem;
    private bool playerNearby;

    private void Awake()
    {
        if (promptObject != null)
        {
            promptObject.SetActive(false);
        }
    }

    private void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (playerCamera == null)
        {
            Debug.LogError("BuyStation: No camera found. Tag the player camera MainCamera.");
        }

        if (purchaseSpawnPoint == null)
        {
            Debug.LogError("BuyStation: Assign a Purchase Spawn Point in the Inspector.");
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
    }

    private void Update()
    {
        UpdateShopMoneyUI();

        currentItem = null;

        if (playerCamera == null)
            return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            Item hitItem = hit.collider.GetComponentInParent<Item>();

            if (hitItem != null && availableItems.ContainsKey(hitItem))
            {
                currentItem = hitItem;
                Debug.Log("Looking at shop item: " + hitItem.ItemName);
            }
        }

        bool showPrompt = playerNearby && currentItem != null;

        if (promptObject == null)
        {
            Debug.LogError("BuyStation: Prompt Object is not assigned!");
        }
        else
        {
            promptObject.SetActive(showPrompt);

            if (showPrompt)
            {
                Debug.Log("BuyStation: Showing purchase prompt.");

                if (promptText != null)
                {
                    promptText.text = $"{currentItem.ItemName}\n" + $"Price: ${currentItem.Value}\n" + "Press F to Buy";
                }
                else
                {
                    Debug.LogError("BuyStation: Prompt Text is not assigned!");
                }
            }
        }

    }

    private void SpawnStock()
    {
        if (stock == null) return;

        Debug.Log($"BuyStation: Starting stock spawn. Entries: {stock.Length}");

        foreach (StockEntry entry in stock)
        {
            if (entry.itemPrefab == null)
            {
                Debug.LogError("BuyStation: Item Prefab is not assigned!");
                continue;
            }

            if (entry.displayPoint == null)
            {
                Debug.LogError($"BuyStation: Display Point missing for {entry.itemPrefab.name}!");
                continue;
            }

            GameObject display = Instantiate(entry.itemPrefab, entry.displayPoint.position, entry.displayPoint.rotation);

            Debug.Log($"BuyStation: Spawned {display.name} at {display.transform.position}");

            Rigidbody rb = display.GetComponentInChildren<Rigidbody>();

            if (rb != null)
            {
                rb.isKinematic = true;
            }

            Item item = display.GetComponentInChildren<Item>();

            if (item == null)
            {
                Debug.LogError($"{display.name} has no Item component!");
                continue;
            }

            availableItems.Add(item, entry);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) || other.transform.root.CompareTag(playerTag))
        {
            playerColliders.Add(other);
            playerNearby = true;

            Debug.Log("Player entered buy station area.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        playerColliders.Remove(other);
        playerNearby = playerColliders.Count > 0;

        if (!playerNearby)
        {
            currentItem = null;

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

        if (Shop.Instance == null)
        {
            Debug.LogError("BuyStation: No Shop instance found.");
            return;
        }

        if (purchaseSpawnPoint == null || entry.itemPrefab == null) return;

        int price = Mathf.Max(0, currentItem.Value);

        if (!Shop.Instance.BuyItem(price))
        {
            Debug.Log("Not enough gold!");
            return;
        }

        string purchasedName = currentItem.ItemName;
        Item purchasedDisplay = currentItem;

        availableItems.Remove(purchasedDisplay);
        currentItem = null;

        Destroy(purchasedDisplay.gameObject);

        GameObject droppedItem = Instantiate(entry.itemPrefab, purchaseSpawnPoint.position, purchaseSpawnPoint.rotation);

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

        Debug.Log("Purchased " + purchasedName);
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
}