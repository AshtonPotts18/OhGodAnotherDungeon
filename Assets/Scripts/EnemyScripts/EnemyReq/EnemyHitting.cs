using UnityEngine;

public class EnemyHitting : MonoBehaviour
{
    [SerializeField] private float maxHealth = 30f;

    private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f || currentHealth <= 0f) { return; }

        currentHealth -= amount;

        Debug.Log($"{name} took {amount} damage. Health: {currentHealth}/{maxHealth}", this);

        if (currentHealth <= 0f)
        {
            Debug.Log($"{name} was defeated.", this);
            Destroy(gameObject);
        }
    }
}