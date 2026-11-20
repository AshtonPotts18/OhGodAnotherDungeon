using UnityEngine;
using UnityEngine.EventSystems;

public class LastButtonTouched : MonoBehaviour
{
    private GameObject lastSelected;

    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            lastSelected = EventSystem.current.currentSelectedGameObject;
        }
        else if (lastSelected != null && lastSelected.activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(lastSelected);
        }
    }
}
