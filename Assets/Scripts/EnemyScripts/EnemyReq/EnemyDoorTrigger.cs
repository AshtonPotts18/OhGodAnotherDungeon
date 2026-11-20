using UnityEngine;
using UnityEngine.Events;
public class EnemyDoorTrigger : MonoBehaviour
{
    [SerializeField] private UnityEvent onEnemyApproach;
    private float nextCheck;

    private void OnTriggerEnter(Collider other) { TryOpenDoor(other); }

    private void OnTriggerStay(Collider other)
    {
        if (Time.time < nextCheck) { return; }
        TryOpenDoor(other);
    }

    private void TryOpenDoor(Collider other)
    {
        Enemy_Movement enemy = other.GetComponentInParent<Enemy_Movement>();
        if (enemy == null) { enemy = other.transform.root.GetComponentInChildren<Enemy_Movement>(); }

        if(enemy == null || !enemy.isActiveAndEnabled) { return; }

        nextCheck = Time.time + 0.5f;
        onEnemyApproach?.Invoke();
    }
}
