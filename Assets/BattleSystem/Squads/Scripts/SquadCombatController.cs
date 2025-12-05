using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class SquadCombatController : MonoBehaviour
{
    public Squad squad;
    public float engageDistance = 2.5f;

    public bool isEngaged = false;

    // Tracks enemy squads in range
    public readonly List<SquadCombatController> enemySquadsInRange = new();

    private SphereCollider col;

    void Awake()
    {
        if (squad == null)
            squad = GetComponent<Squad>();

        col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 7f;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Detect enemy squads only
        if (other.TryGetComponent<SquadCombatController>(out var enemySquadCtrl))
        {
            if (enemySquadCtrl.squad.teamID != squad.teamID && !enemySquadsInRange.Contains(enemySquadCtrl))
            {
                enemySquadsInRange.Add(enemySquadCtrl);
                Debug.Log($"{squad.name} → Enemy squad detected: {enemySquadCtrl.squad.name}");
                UpdateEngagementState();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<SquadCombatController>(out var enemySquadCtrl))
        {
            if (enemySquadsInRange.Contains(enemySquadCtrl))
            {
                enemySquadsInRange.Remove(enemySquadCtrl);
                Debug.Log($"{squad.name} → Enemy squad left: {enemySquadCtrl.squad.name}");
                UpdateEngagementState();
            }
        }
    }

    private void UpdateEngagementState()
    {
        bool newState = enemySquadsInRange.Count > 0;

        if (newState != isEngaged)
        {
            isEngaged = newState;
            Debug.Log($"{squad.name} → Engagement State: {(isEngaged ? "ENGAGED" : "CLEAR")}");
        }
    }

    void Update()
    {
        if (!isEngaged) return;
        if (squad == null || squad.soldiers.Count == 0) return;

        foreach (var soldier in squad.soldiers)
        {
            if (soldier == null) continue;

            UnitCombat combat = soldier.GetComponent<UnitCombat>();
            if (combat == null || combat.combatDisabled) continue;

            UnitCombat closestEnemyUnit = null;
            float shortestDistance = Mathf.Infinity;

            // Find closest enemy unit inside any detected enemy squad
            foreach (var enemySquadCtrl in enemySquadsInRange)
            {
                if (enemySquadCtrl == null || enemySquadCtrl.squad == null) continue;

                foreach (var enemySoldier in enemySquadCtrl.squad.soldiers)
                {
                    if (enemySoldier == null) continue;

                    float dist = Vector3.Distance(soldier.position, enemySoldier.position);
                    if (dist < shortestDistance)
                    {
                        shortestDistance = dist;
                        closestEnemyUnit = enemySoldier.GetComponent<UnitCombat>();
                    }
                }
            }

            if (closestEnemyUnit != null)
            {
                combat.SetTarget(closestEnemyUnit.transform);

                if (shortestDistance > engageDistance)
                    combat.MoveTowardsTarget();
                else
                    combat.TryAttack();
            }
        }
    }

    void OnDrawGizmos()
    {
        if (col == null) col = GetComponent<SphereCollider>();

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, col.radius);
    }
}
