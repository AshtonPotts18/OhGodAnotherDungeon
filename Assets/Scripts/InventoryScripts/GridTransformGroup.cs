using UnityEngine;

[ExecuteAlways]
public class GridTransformGroup : MonoBehaviour
{
    [Header("Grid Settings")]
    [Min(1)] public int columns = 3;
    public Vector2 cellSize = new Vector2(50, 50);
    public Vector2 spacing = new Vector2(2, 2);

    [Header("Options")]
    public bool useRectTransform = true;
    public bool includeInactive = false;

    public void ArrangeChildren()
    {
        if (columns < 1)
            return;

        int gridIndex = 0;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            if (!includeInactive && !child.gameObject.activeSelf)
                continue;

            int row = gridIndex / columns;
            int col = gridIndex % columns;

            Vector3 newPosition = new Vector3(
                col * (cellSize.x + spacing.x),
                -row * (cellSize.y + spacing.y),
                0f
            );

            if (useRectTransform &&
                child is RectTransform rect)
            {
                rect.anchoredPosition = new Vector2(
                    newPosition.x,
                    newPosition.y
                );
            }
            else
            {
                child.localPosition = newPosition;
            }

            gridIndex++;
        }
    }

    private void OnEnable()
    {
        ArrangeChildren();
    }

    private void OnTransformChildrenChanged()
    {
        ArrangeChildren();
    }

    private void OnValidate()
    {
        columns = Mathf.Max(1, columns);
        ArrangeChildren();
    }

    [ContextMenu("Arrange Children")]
    private void ArrangeChildrenFromMenu()
    {
        ArrangeChildren();
    }
}