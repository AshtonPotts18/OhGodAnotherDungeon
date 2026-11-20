using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInterator : MonoBehaviour
{
    private Camera playerCamera;
    [SerializeField] InputActionAsset inputActions;
    [SerializeField] private TextMeshProUGUI interactPrompt;
    private InputAction interactAction;
    [SerializeField] float interactRange = 2.5f;
    [SerializeField] string sellStationLayerName = "SellStation";
    int shopLayerMask = 0;
    int sellStationLayerMask = 8;

    private void Awake()
    {
        interactAction = inputActions.FindAction("Interact");
        
    }
    private void Start()
    {
        shopLayerMask = LayerMask.GetMask("Shop");
        sellStationLayerMask = LayerMask.GetMask(sellStationLayerName);

        TextMeshProUGUI[] texts = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include);
        playerCamera = GameManager.PlayerCamera;

        foreach (TextMeshProUGUI text in texts)
        {
            if (text.gameObject.name == "InteractPrompt")
            {
                interactPrompt = text;
                break;
            }
        }
    }

    private void Update()
    {

        if (playerCamera == null) return;

        


        Ray playerCrossHair = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(playerCrossHair, out RaycastHit hitInfo, interactRange, ~shopLayerMask) && hitInfo.collider.TryGetComponent<IInteract>(out _))
        {
            interactPrompt.text = "Left Click to Interact";
            interactPrompt.gameObject.SetActive(true);
        }
        else
        {
            interactPrompt.gameObject.SetActive(false);
        }
    }

    public void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            Ray playerLookDirection = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            Debug.DrawRay(playerLookDirection.origin, playerLookDirection.direction);

            if (Physics.Raycast(playerLookDirection, out RaycastHit _, interactRange, sellStationLayerMask))
            {
                return;
            }

            //first check if something is in player's hand
            else if (GameManager.Player.TryGetComponent<PickupController>(out PickupController pickup) && pickup.HeldItem != null) // first check if there is an item in the player hands, if so then use it
            {
                Debug.Log(pickup.HeldItem.GetType());
                pickup.HeldItem.UseItem();
                // use item
            }
            else
            {
                

                if (Physics.Raycast(playerLookDirection, out RaycastHit hitInfo, interactRange) && hitInfo.collider.TryGetComponent<IInteract>(out IInteract interatableObject))
                { 
                    //IInteract interatableObject = hitInfo.collider.GetComponent<IInteract>();
                    interatableObject?.Interact(); // checks if he interactable object is null
                }

            }

        }

    }
}
