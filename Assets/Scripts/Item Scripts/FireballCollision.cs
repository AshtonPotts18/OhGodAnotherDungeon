using UnityEngine;

public class FireballCollision : MonoBehaviour
{
    [SerializeField] private float damage = 30f;

    public GameObject Smoke;
    private bool hasHit;

    private void OnCollisionEnter(Collision collision)
    {
        if(hasHit) { return; }
        hasHit = true;

        MonoBehaviour[] components = collision.collider.GetComponentsInParent<MonoBehaviour>();

        foreach(MonoBehaviour component in components)
        {
            if(component is DamagingEnemy.IDamageable damageable)
            {
                damageable.TakeDamage(damage);
                break;
            }
        }
        GameObject Smoke_ = Instantiate(Smoke, transform.position, transform.rotation);
        
        Destroy(gameObject);
    }

}
