using UnityEngine;
using UnityEngine.EventSystems;

public class SelectButtons : MonoBehaviour
{
    public GameObject button;

    void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(button);
    }
}
