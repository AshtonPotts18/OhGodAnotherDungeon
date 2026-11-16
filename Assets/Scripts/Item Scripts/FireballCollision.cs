using UnityEngine;

public class FireballCollision : MonoBehaviour
{
    [SerializeField] private float damage = 30f;

    
    private bool hasHit;

    public GameObject smokePuffPrefab;

    [System.Obsolete]

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

        if (smokePuffPrefab != null)
        {
            GameObject smokeInstance = Instantiate(smokePuffPrefab, transform.position, Quaternion.identity);
           
            Destroy(smokeInstance, 2.0f);
        }

        Destroy(gameObject);
    }

}
