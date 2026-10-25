using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInterator : MonoBehaviour
{
    private Camera playerCamera;
    [SerializeField] InputActionAsset inputActions;
    private InputAction interactAction;
    [SerializeField] float interactRange = 2.5f;

    private void OnEnable()
    {
        inputActions.FindActionMap("Player").Enable();
    }
    //private void OnDisable()
    //{
    //    inputActions.FindActionMap("Player").Disable();
    //}
    private void Awake()
    {
        interactAction = inputActions.FindAction("Interact");
        
    }
    private void Start()
    {
        
        playerCamera = GameManager.PlayerCamera;
    }

    public void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {

            //first check if something is in player's hand
            if (GameManager.Player.TryGetComponent<PickupController>(out PickupController pickup)) // first check if there is an item in the player hands, if so then use it
            {
                if (pickup.HeldItem != null)
                {
                    Debug.Log(pickup.HeldItem.GetType());
                    pickup.HeldItem.UseItem();
                }
                // use item
            }
            else
            {
                Ray playerLookDirection = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

                if (Physics.Raycast(playerLookDirection, out RaycastHit hitInfo, interactRange))
                {
                    IInteract interatableObject = hitInfo.collider.GetComponent<IInteract>();
                    interatableObject.Interact();
                }
            }

        }

    }
}
