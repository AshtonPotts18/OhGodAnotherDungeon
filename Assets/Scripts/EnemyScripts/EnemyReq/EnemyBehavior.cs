using UnityEngine;

public abstract class EnemyBehavior : MonoBehaviour
{
    public abstract void Initialize(
        Transform target, Detection detector, Enemy_Movement mover);
}
