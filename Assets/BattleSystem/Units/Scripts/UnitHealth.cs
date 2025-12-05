using UnityEngine;

public class UnitHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    public bool IsDead { get; private set; } = false;

    private UnitStats stats;
    private Animator animator;

    void Awake()
    {
        stats = GetComponent<UnitStats>();
        animator = GetComponent<Animator>();

        if (stats != null)
            maxHealth = stats.maxHealth;

        currentHealth = maxHealth;

        Debug.Log($"{name}: UnitHealth Awake → MaxHealth {maxHealth}");
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        currentHealth -= amount;

        Debug.Log($"{name}: Took DAMAGE {amount}, HP now {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Debug.Log($"{name}: DIED");
            Die();
        }
    }

    private void Die()
    {
        IsDead = true;

        var combat = GetComponent<UnitCombat>();
        if (combat != null) combat.combatDisabled = true;

        var movement = GetComponent<UnitMovement>();
        if (movement != null) movement.StopImmediate();

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 4f);
    }
}