using UnityEngine;

public class DamagingEnemy : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public interface IDamageable
    {
        void TakeDamage(float amount);
    }

}
