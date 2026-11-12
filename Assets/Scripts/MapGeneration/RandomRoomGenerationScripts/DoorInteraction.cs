using UnityEngine;
using UnityEngine.AI;

public class DoorInteraction : MonoBehaviour, IInteract
{
    private static readonly int InteractHash = Animator.StringToHash("Interact");
    private bool isOpen = false;
    [SerializeField] private Animator animation;
    [SerializeField] AnimationClip openDoorClip;
    [SerializeField] AnimationClip closeDoorClip;
 

    public void Interact()
    {
        if (isOpen) 
        {
            animation.SetTrigger(InteractHash);
        }
        else
        {
            animation.SetTrigger(InteractHash);
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

}
