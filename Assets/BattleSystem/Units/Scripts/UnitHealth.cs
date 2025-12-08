using UnityEngine;
using System;

public class UnitHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    public bool IsDead { get; private set; } = false;
    public event Action<UnitHealth> OnHealthChanged;

    private UnitStats stats;
    private Animator animator;
    private SquadMorale squadMorale;

    void Awake()
    {
        stats = GetComponent<UnitStats>();
        animator = GetComponent<Animator>();

        if (stats != null)
            maxHealth = stats.maxHealth;

        currentHealth = maxHealth;
        Squad parentSquad = GetComponentInParent<Squad>();
        if (parentSquad != null)
            squadMorale = parentSquad.GetComponent<SquadMorale>();
    }

    /// <summary>
    /// Apply damage to this unit.
    /// </summary>
    /// <param name="amount">Amount of damage to take.</param>
    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(0f, currentHealth);
        OnHealthChanged?.Invoke(this);

        if (currentHealth <= 0f)
            Die();
    }

    /// <summary>
    /// Handle unit death.
    /// </summary>
    private void Die()
    {
        if (IsDead) return;
        IsDead = true;
        squadMorale?.ApplyCasualty();
        var combat = GetComponent<UnitCombat>();
        if (combat != null)
            combat.combatDisabled = true;
        var movement = GetComponent<UnitMovement>();
        if (movement != null)
            movement.StopImmediate();

    
        if (animator != null)
            animator.SetTrigger("Die");

        var col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

      
        OnHealthChanged?.Invoke(this);

        Destroy(gameObject, 4f);
    }

    /// <summary>
    /// Heal this unit by a certain amount.
    /// </summary>
    public void Heal(float amount)
    {
        if (IsDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        OnHealthChanged?.Invoke(this);
    }
}
