using UnityEngine;

public class EnemyDoorTrigger : MonoBehaviour
{
    [SerializeField] private DoorInteraction door;

    private void Awake()
    {
        if (door == null) { door = GetComponentInParent<DoorInteraction>(); }    
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[EnemyDoorTrigger] Entered by {other.name}", this);

        if (door == null)
        {
            Debug.LogError("[EnemyDoorTrigger] Door slot is empty.", this);
            return;
        }

        Enemy_Movement enemy = other.GetComponentInParent<Enemy_Movement>();

        if (enemy == null)
        {
            Debug.Log($"[EnemyDoorTrigger] {other.name} is not on an Enemy_Movement object.", this);
            return;
        }

        Debug.Log("[EnemyDoorTrigger] Enemy found; opening door.", this);
        door.OpenForEnemy();
    }

}
