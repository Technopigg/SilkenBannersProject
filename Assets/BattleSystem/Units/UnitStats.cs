using UnityEngine;

[DisallowMultipleComponent]
public class UnitStats : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("Primary Combat")]
    public float attackDamage = 12f;
    public float attackRate = 1.0f;
    public float weaponReach = 1.6f;

    [Header("Defence")]
    public float defense = 0f;
    [Header("Movement")]
    public float moveSpeed = 3.5f;
    [Header("Psychology")]
    [Range(0f, 2f)] public float bravery = 1.0f;
    [Range(0f, 1f)] public float morale = 1.0f;
    [Range(0f, 1f)] public float fatigue = 0f;

    [Header("Formation / Cohesion")]
    [Range(0f, 1f)] public float cohesion = 1.0f;

    [Header("Facing Multipliers")]
    public float frontMultiplier = 1.0f;
    public float flankMultiplier = 1.2f;
    public float rearMultiplier = 1.4f;

    [Header("Facing Angles")]
    public float frontAngle = 60f;
    public float rearAngle = 120f;

    void Awake()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    public bool IsAlive() => currentHealth > 0f;

    public float TakeDamage(float rawDamage, Vector3 attackerPos, Transform attacker = null)
    {
        if (!IsAlive()) return 0f;

        float facingMult = 1f;

        Vector3 toAttacker = attackerPos - transform.position;
        toAttacker.y = 0f;

        if (toAttacker.sqrMagnitude > 0.0001f)
        {
            float ang = Vector3.Angle(transform.forward, toAttacker);
            if (ang <= frontAngle) facingMult = frontMultiplier;
            else if (ang >= rearAngle) facingMult = rearMultiplier;
            else facingMult = flankMultiplier;
        }

        float damage = rawDamage * facingMult;
        float finalDamage = Mathf.Max(1f, damage - defense);

        currentHealth -= finalDamage;

        float moraleHit = Mathf.Clamp01((finalDamage / maxHealth) * 1.5f);
        moraleHit *= facingMult;
        morale = Mathf.Clamp01(morale - moraleHit);

        if (currentHealth <= 0f)
            Die();

        return finalDamage;
    }

    public void AddFatigue(float amount)
    {
        fatigue = Mathf.Clamp01(fatigue + amount);
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
    }

    void Die()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }
}
