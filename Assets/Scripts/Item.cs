using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] public GameObject worldPrefab;

    //Item size
    [SerializeField, Min(1)] public int width = 2;
    [SerializeField, Min(1)] public int height = 2;

    [HideInInspector] public int gridX;
    [HideInInspector] public int gridY;
    [HideInInspector] public bool isRotated;
    [HideInInspector] public bool isPlaced;

    public int Width => isRotated ? height : width;
    public int Height => isRotated ? width : height;

    public RectTransform RectTransform { get; private set; }

    void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
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
}
