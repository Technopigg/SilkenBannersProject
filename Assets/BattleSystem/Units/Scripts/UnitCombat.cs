using UnityEngine;

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
        Debug.Log($"{name}: UnitCombat Awake - squadRoot = {squadRoot}");
    }

    public void SetTarget(Transform t)
    {
        if (combatDisabled) return;

        currentTarget = t;
        Debug.Log($"{name}: Target SET → {t.name}");
    }

    public void MoveTowardsTarget()
    {
        if (combatDisabled || currentTarget == null) return;

        UnitMovement mover = GetComponent<UnitMovement>();
        if (mover != null)
        {
            Debug.Log($"{name}: Moving toward target {currentTarget.name}");
            mover.SetMovementTarget(currentTarget.position, mover.MoveSpeed);
        }
    }

    public void TryAttack()
    {
        if (combatDisabled || currentTarget == null) return;

        float dist = Vector3.Distance(transform.position, currentTarget.position);

        Debug.Log($"{name}: TryAttack() → target={currentTarget.name}, dist={dist}");

        if (dist > attackRange)
        {
            Debug.Log($"{name}: Target OUT of attack range");
            return;
        }

        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;

            if (currentTarget.TryGetComponent<UnitHealth>(out var hp))
            {
                Debug.Log($"{name}: ATTACKING {currentTarget.name} for {attackDamage} dmg!");
                hp.TakeDamage(attackDamage);
            }
            else
            {
                Debug.Log($"{name}: ERROR → Target has NO UnitHealth");
            }
        }
        else
        {
            Debug.Log($"{name}: Attack ON COOLDOWN");
        }
    }

    public void DisableCombatTemporarily()
    {
        Debug.Log($"{name}: Combat disabled temporarily");
        combatDisabled = true;
        StopAllCoroutines();

        var move = GetComponent<UnitMovement>();
        if (move != null)
            move.StopImmediate();
    }

    public void EnableCombat()
    {
        Debug.Log($"{name}: Combat re-enabled");
        combatDisabled = false;
    }

    void OnDrawGizmosSelected()
    {
        // Attack Range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Current Target
        if (currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
}
