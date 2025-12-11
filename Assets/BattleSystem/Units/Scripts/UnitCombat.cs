using UnityEngine;

[RequireComponent(typeof(UnitMovement))]
public class UnitCombat : MonoBehaviour
{
    [Header("Combat Stats")]
    public float attackRange = 1.8f;
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;

    [Header("Unit Type")]
    public bool isRanged = false;

    [Header("Runtime")]
    public Transform currentTarget;
    public Squad squadRoot;
    public bool combatDisabled = false;

    [Header("Animation")]
    public Animator animator;

    private float nextAttackTime = 0f;
    private UnitMovement mover;

    void Awake()
    {
        mover = GetComponent<UnitMovement>();
        if (animator == null)
            animator = GetComponent<Animator>();

        squadRoot = GetComponentInParent<Squad>();
        if (squadRoot == null)
            Debug.LogWarning($"{name}: No Squad found in parent hierarchy!");
    }

    public void SetTarget(Transform t)
    {
        if (combatDisabled) return;
        if (t == null) return;

        currentTarget = t;
    }

    public void MoveTowardsTarget()
    {
        if (combatDisabled || currentTarget == null || mover == null) return;
        mover.SetMovementTarget(currentTarget.position, mover.MoveSpeed);

        if (animator != null)
            animator.SetBool("Walking", true);
    }

    public void TryAttack()
    {
        if (combatDisabled || currentTarget == null) return;

        float dist = Vector3.Distance(transform.position, currentTarget.position);
        if (dist > attackRange)
        {
            MoveTowardsTarget();
            return;
        }

        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;

            if (currentTarget.TryGetComponent<UnitHealth>(out var hp))
                hp.TakeDamage(attackDamage);

            if (animator != null)
            {
                animator.SetTrigger("InwardSlash");
                animator.SetBool("Walking", false);
            }
        }
    }

    public void DisableCombatTemporarily()
    {
        combatDisabled = true;
        if (mover != null)
            mover.StopImmediate();
    }

    public void EnableCombat()
    {
        combatDisabled = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
}
