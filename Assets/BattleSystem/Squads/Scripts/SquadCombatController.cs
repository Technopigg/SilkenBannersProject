using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class SquadCombatController : MonoBehaviour
{
    public Squad squad;
    public float engageDistance = 0.5f;

    public bool isEngaged = false;
    public readonly List<SquadCombatController> enemySquadsInRange = new();

    private SphereCollider col;

    void Awake()
    {
        if (squad == null)
            squad = GetComponent<Squad>();

        col = GetComponent<SphereCollider>();
        col.isTrigger = true;

        if (col.radius <= 0f)
            col.radius = 1f;
    }

    private float GetWorldRadius(SphereCollider c)
    {
        if (c == null) return 0f;
        Vector3 lossy = c.transform.lossyScale;
        float maxScale = Mathf.Max(Mathf.Abs(lossy.x), Mathf.Abs(lossy.y), Mathf.Abs(lossy.z));
        return c.radius * maxScale;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<SquadCombatController>(out var enemySquadCtrl))
            return;

        if (enemySquadCtrl.squad.teamID == squad.teamID) return;
        
        Vector3 myCenter = squad.GetSquadCenter();
        Vector3 enemyCenter = enemySquadCtrl.squad.GetSquadCenter();

        float myWorldRadius = GetWorldRadius(col);
        float enemyWorldRadius = GetWorldRadius(enemySquadCtrl.GetComponent<SphereCollider>());

        float centerDistance = Vector3.Distance(myCenter, enemyCenter);

        Debug.Log(
            $"{squad.name} OnTriggerEnter → {enemySquadCtrl.squad.name}\n" +
            $"  MyCenter: {myCenter:F2}, EnemyCenter: {enemyCenter:F2}\n" +
            $"  CenterDist: {centerDistance:F2}\n" +
            $"  MyWorldRadius: {myWorldRadius:F2}, EnemyWorldRadius: {enemyWorldRadius:F2}"
        );
        
        if (centerDistance > myWorldRadius + enemyWorldRadius + 0.05f)
        {
            Debug.LogWarning($"{squad.name} → IGNORING trigger with {enemySquadCtrl.squad.name} (centers farther than combined radii)");
            return;
        }

        if (!enemySquadsInRange.Contains(enemySquadCtrl))
        {
            enemySquadsInRange.Add(enemySquadCtrl);
            Debug.Log($"{squad.name} → Enemy squad detected: {enemySquadCtrl.squad.name}");
            UpdateEngagementState();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<SquadCombatController>(out var enemySquadCtrl))
            return;

        if (enemySquadsInRange.Contains(enemySquadCtrl))
        {
            enemySquadsInRange.Remove(enemySquadCtrl);
            Debug.Log($"{squad.name} → Enemy squad left: {enemySquadCtrl.squad.name}");
            UpdateEngagementState();
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

                if (shortestDistance > combat.attackRange)
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

        if (squad != null && squad.soldiers != null && squad.soldiers.Count > 0)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(squad.GetSquadCenter(), col.radius);
        }
    }
}
