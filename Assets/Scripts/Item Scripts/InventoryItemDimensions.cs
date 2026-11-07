using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryItemDimensions : MonoBehaviour
{
    [SerializeField] public GameObject worldPrefab;
    [SerializeField] private InputActionReference hotkeyAction;

    //Item size
    [SerializeField, Min(1)] public int width = 2;
    [SerializeField, Min(1)] public int height = 2;

    [HideInInspector] public Item originalItem;
    //Fields
    [HideInInspector] public int gridX;
    [HideInInspector] public int gridY;
    [HideInInspector] public bool isRotated;
    [HideInInspector] public bool isPlaced;

    //Getters
    public int Width => isRotated ? height : width;
    public int Height => isRotated ? width : height;
    public InputActionReference HotkeyAction => hotkeyAction;

    public RectTransform RectTransform { get; private set; }
    public InventoryItemDimensions instance { get; private set; }

    void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
        instance = this;
    }

    void OnValidate()
    {
        width = Mathf.Max(1, width);
        height = Mathf.Max(1, height);
    }

    public void SetGridPosition(int x, int y)
    {
        gridX = x;
        gridY = y;
    }
    public void SetRotation(bool rotated)
    {
        isRotated = rotated;
    }

    public void SetHotkeyAction(InputActionReference action)
    {
        hotkeyAction = action;
    }
}
