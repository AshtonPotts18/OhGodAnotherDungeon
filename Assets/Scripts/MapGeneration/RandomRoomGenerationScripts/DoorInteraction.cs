using UnityEngine;
using UnityEngine.AI;

public class DoorInteraction : MonoBehaviour, IInteract
{
    private static readonly int InteractHash = Animator.StringToHash("Interact");
    private bool isOpen = false;
    [SerializeField] private Animator animation;
 

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

}
