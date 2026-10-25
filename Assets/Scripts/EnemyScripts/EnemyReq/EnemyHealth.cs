using UnityEngine;

public class EnemyHealth : MonoBehaviour, DamagingEnemy.IDamageable
{
    [SerializeField, Min(1f)] private float maxHealth = 30f;

    private float currentHealth;
    public float CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
        Debug.Log($"{name}: health initialized to {currentHealth}.", this);
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f || currentHealth <= 0f) { return; }

        currentHealth = Mathf.Max(0f, currentHealth - amount);

        Debug.Log(
            $"{name} took {amount} damage. Health: {currentHealth}/{maxHealth}",
            this);

        if (currentHealth <= 0f) { Die(); }

    }

    private void Die()
    {
        Debug.Log($"{name} was defeated.", this);
        Destroy(gameObject);
    }
}
