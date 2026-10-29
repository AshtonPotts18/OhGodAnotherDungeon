using UnityEngine;
using static DamagingEnemy;

public abstract class EnemyBehavior : MonoBehaviour
{
    public abstract void Initialize(
        Transform target, Detection detector, Enemy_Movement mover);

    protected IDamageable FindDamageable(Transform target)
    {
        if(target == null) { return null; }

        foreach(MonoBehaviour component in target.GetComponentsInParent<MonoBehaviour>(true))
        { if (component is IDamageable damageable) { return damageable; } }

        foreach (MonoBehaviour component in target.GetComponentsInChildren<MonoBehaviour>(true))
        { if (component is IDamageable damageable) { return damageable; } }

        return null;
    }

    public virtual void DealDamage(IDamageable target, float amount)
    {
        if (target != null && amount > 0f) { target.TakeDamage(amount); }
    }
}
