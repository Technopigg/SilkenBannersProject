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

    public bool combatDisabled = false;    // ✔ Used by morale system

    private float nextAttackTime = 0f;

    void Awake()
    {
        squadRoot = GetComponentInParent<Squad>();
    }

    public void SetTarget(Transform t)
    {
        if (combatDisabled) return;
        currentTarget = t;
    }

    public void MoveTowardsTarget()
    {
        if (combatDisabled || currentTarget == null) return;

        UnitMovement mover = GetComponent<UnitMovement>();
        if (mover != null)
        {
            mover.SetMovementTarget(currentTarget.position, mover.moveSpeed);
        }
    }

    public void TryAttack()
    {
        if (combatDisabled || currentTarget == null) return;

        float dist = Vector3.Distance(transform.position, currentTarget.position);
        if (dist > attackRange) return;

        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;

            if (currentTarget.TryGetComponent<UnitHealth>(out var hp))
                hp.TakeDamage(attackDamage);
        }
    }

    // ─────────────────────────────────────────────
    // ✔ Added for SquadMorale system
    // ─────────────────────────────────────────────
    public void DisableCombatTemporarily()
    {
        combatDisabled = true;
        StopAllCoroutines();

        var move = GetComponent<UnitMovement>();
        if (move != null)
            move.StopImmediate();
    }

    public void EnableCombat()
    {
        combatDisabled = false;
    }
}