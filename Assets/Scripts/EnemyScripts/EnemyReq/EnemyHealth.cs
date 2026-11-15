using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour, DamagingEnemy.IDamageable
{
    [SerializeField, Min(1f)] private float maxHealth = 30f;

    [SerializeField, Min(0.01f)] private float damageFlashDuration = 0.15f;

    private Renderer[] damageRenderers;
    private MaterialPropertyBlock[] originalPropertyBlocks;
    private Coroutine damageFlashRoutine;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    private float currentHealth;
    public float CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
        damageRenderers = GetComponentsInChildren<Renderer>(true);
        originalPropertyBlocks = new MaterialPropertyBlock[damageRenderers.Length];

        for (int i = 0; i < damageRenderers.Length; i++)
        {
            originalPropertyBlocks[i] = new MaterialPropertyBlock();
            damageRenderers[i].GetPropertyBlock(originalPropertyBlocks[i]);
        }
        Debug.Log($"{name}: health initialized to {currentHealth}.", this);
    }

    private void TriggerDamageFlash()
    {
        if (damageFlashRoutine != null)
            StopCoroutine(damageFlashRoutine);

        damageFlashRoutine = StartCoroutine(DamageFlashRoutine());
    }

    private IEnumerator DamageFlashRoutine()
    {
        foreach (Renderer enemyRenderer in damageRenderers)
        {
            if (enemyRenderer == null)
                continue;

            MaterialPropertyBlock flashBlock = new MaterialPropertyBlock();
            enemyRenderer.GetPropertyBlock(flashBlock);

            flashBlock.SetColor(BaseColorId, Color.red);
            flashBlock.SetColor(ColorId, Color.red);
            flashBlock.SetColor(EmissionColorId, new Color(2f, 0f, 0f, 1f));

            enemyRenderer.SetPropertyBlock(flashBlock);
        }

        yield return new WaitForSeconds(damageFlashDuration);

        for (int i = 0; i < damageRenderers.Length; i++)
        {
            if (damageRenderers[i] != null)
                damageRenderers[i].SetPropertyBlock(originalPropertyBlocks[i]);
        }

        damageFlashRoutine = null;
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f || currentHealth <= 0f) { return; }

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        TriggerDamageFlash();

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
