using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public GameObject playerCamera;

    Animator animation;
    public float moveSpeed = 5.0f;
    public float lookSpeed = 30.0f;
    public Slider slider;

    public float sprintspeed = 1.25f;

    [SerializeField]
    CharacterController playerController;
    InputAction move;
    InputAction look;

    float cameraPitch = 0f;

    void Start()
    {
        move = InputSystem.actions.FindAction("Move");
        look = InputSystem.actions.FindAction("Look");
        playerController = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Movement()
    {
        Vector2 dir = move.ReadValue<Vector2>();
        Vector3 movement = (transform.right * dir.x + transform.forward * dir.y) * moveSpeed * Time.deltaTime;
        
        if(Keyboard.current.shiftKey.isPressed && slider.value > 0)
        {
            movement.x *= sprintspeed;
            movement.z *= sprintspeed;
        }
        playerController.Move(movement);

        Vector2 dir2 = look.ReadValue<Vector2>();
        transform.Rotate(new Vector3(0, dir2.x, 0) * lookSpeed * Time.deltaTime);

        cameraPitch -= dir2.y * lookSpeed * Time.deltaTime;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
    }
    void Update()
    {
        
        Movement();

    }

}