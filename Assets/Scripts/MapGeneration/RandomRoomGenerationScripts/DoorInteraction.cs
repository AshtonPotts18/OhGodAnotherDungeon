using UnityEngine;
using UnityEngine.AI;

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
        gameObject.GetComponent<NavMeshObstacle>().carving = isOpen;
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

    //Ashton made this change to add the ability for doors to be detected by the enemies 
    public void OpenForEnemy()
    {
        if (isOpen)
        {
            return;
        }
        transform.Rotate(0f, -90f, 0f);
        isOpen = true;
    }

}
