using UnityEngine;

public class DoorInteraction : MonoBehaviour, IInteract
{
    private bool isOpen = false;
 

    public void Interact()
    {
        if (isOpen) 
        {
            transform.Rotate(0, 90, 0);
        }
        else
        {
            transform.Rotate(0, -90, 0);
        }
        isOpen = !isOpen;
    }

    private void OpenDoor()
    {
        if (isOpen)
        {
            transform.Rotate(0, 90, 0);
        }
        else
        {
            transform.Rotate(0, -90, 0);
        }
        isOpen = !isOpen;
    }

}
