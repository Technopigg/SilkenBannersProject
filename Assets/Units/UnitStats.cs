using UnityEngine;

[System.Serializable]
public class UnitStats : MonoBehaviour
{
    [Header("Base Stats")]
    public float maxHealth = 100f;
    public float attackDamage = 12f;
    public float attackRate = 1.2f;
    public float attackRange = 2f;
    public float defense = 0f;
    public float moveSpeed = 3.5f;

    [Header("Dynamic Stats (Runtime)")]
    public float currentHealth;
    public float morale = 1f; 

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float dmg)
    {
        float finalDamage = Mathf.Max(1f, dmg - defense);
        currentHealth -= finalDamage;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}