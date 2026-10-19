
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float lookSpeed = 30.0f;

    [SerializeField] private CharacterController playerController;
    public Slider slider;

    public float sprintspeed = 1.25f;

    private InputAction move;
    private InputAction look;
    private float cameraPitch = 0f;
    public bool cameraLookLocked = false;
    public bool rotationLocked = false;

    void Start()
    {
        move = InputSystem.actions.FindAction("Move");
        look = InputSystem.actions.FindAction("Look");

        if (playerController == null) playerController = GetComponent<CharacterController>();
    }

    void Movement()
    {
        if (move == null || look == null || playerController == null) return;

        Vector2 dir = move.ReadValue<Vector2>();

        Vector3 movement =
            (transform.right * dir.x + transform.forward * dir.y) * moveSpeed * Time.deltaTime;

        if (Keyboard.current.shiftKey.isPressed && slider.value > 0)
        {
            movement.x *= sprintspeed;
            movement.z *= sprintspeed;
        }


        playerController.Move(movement);

        if (cameraLookLocked) return;

        Vector2 dir2 = look.ReadValue<Vector2>();

        transform.Rotate(new Vector3(0, dir2.x, 0) * lookSpeed * Time.deltaTime);

        if (!rotationLocked && playerCamera != null)
        {
            cameraPitch -= dir2.y * lookSpeed * Time.deltaTime;
            cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);

            playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
        }
    }

    void Update()
    {
        Movement();

        //if (cameraLookLocked == true)
        //{
        //    Cursor.lockState = CursorLockMode.None;
        //    Cursor.visible = true;
        //}
        //else
        //{
        //    Cursor.lockState = CursorLockMode.Locked;
        //    Cursor.visible = false;
        //}
    }

}
