using UnityEngine;

[RequireComponent(typeof(UnitMovement))]
public class UnitCombat : MonoBehaviour
{
    [Header("Combat Stats")]
    public float attackRange = 1.8f;
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;

    [Header("Runtime")]
    public Transform currentTarget;
    public Squad squadRoot;

    public bool combatDisabled = false;

    private float nextAttackTime = 0f;

    void Awake()
    {
        squadRoot = GetComponentInParent<Squad>();
        if (squadRoot == null)
        {
            Debug.LogWarning($"{name}: No Squad found in parent hierarchy!");
        }
        else
        {
            Debug.Log($"{name}: UnitCombat Awake - squadRoot = {squadRoot.name}");
        }
    }

    public void SetTarget(Transform t)
    {
        if (combatDisabled) return;
        if (t == null) return;

        currentTarget = t;
        // Debug log can be commented out in large battles for performance
        // Debug.Log($"{name}: Target SET → {t.name}");
    }

    public void MoveTowardsTarget()
    {
        if (combatDisabled || currentTarget == null) return;

        UnitMovement mover = GetComponent<UnitMovement>();
        if (mover != null)
        {
            mover.SetMovementTarget(currentTarget.position, mover.MoveSpeed);
        }
    }

    public void TryAttack()
    {
        if (combatDisabled || currentTarget == null) return;

        float dist = Vector3.Distance(transform.position, currentTarget.position);
        if (dist > attackRange)
        {
            // Target out of range; move closer
            MoveTowardsTarget();
            return;
        }

        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;

            if (currentTarget.TryGetComponent<UnitHealth>(out var hp))
            {
                hp.TakeDamage(attackDamage);
            }
        }
    }

    public void DisableCombatTemporarily()
    {
        combatDisabled = true;
        StopAllCoroutines();

        var mover = GetComponent<UnitMovement>();
        if (mover != null)
            mover.StopImmediate();
    }

    public void EnableCombat()
    {
        combatDisabled = false;
    }

    void OnDrawGizmosSelected()
    {
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Current target line
        if (currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
}
