using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class SquadCombatController : MonoBehaviour
{
    public Squad squad;
    public float engageDistance = 2.5f;

    public int enemyCount = 0;
    public bool isEngaged = false;

    private readonly List<UnitCombat> enemiesInRange = new();

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
        Debug.Log($"{squad.name} TRIGGER ENTER: {other.name}");

        if (other.TryGetComponent<UnitCombat>(out var unit))
        {
            Debug.Log($" - {other.name} has UnitCombat");

            if (unit.squadRoot != squad)
            {
                Debug.Log($" - {other.name} is ENEMY");

                if (!enemiesInRange.Contains(unit))
                {
                    enemiesInRange.Add(unit);
                    enemyCount++;

                    Debug.Log($" → Enemy ADDED: {other.name}. Total enemies = {enemyCount}");
                }
            }
            else
            {
                Debug.Log($" - {other.name} is SAME SQUAD (ignored)");
            }
        }

        UpdateEngagementState();
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"{squad.name} TRIGGER EXIT: {other.name}");

        if (other.TryGetComponent<UnitCombat>(out var unit))
        {
            if (enemiesInRange.Contains(unit))
            {
                enemiesInRange.Remove(unit);
                enemyCount--;

                Debug.Log($" → Enemy REMOVED: {other.name}. Total enemies = {enemyCount}");
            }
        }

        UpdateEngagementState();
    }

    private void UpdateEngagementState()
    {
        bool newState = enemiesInRange.Count > 0;

        if (newState != isEngaged)
        {
            isEngaged = newState;
            Debug.Log($"{squad.name} → Engagement State: {(isEngaged ? "ENGAGED" : "CLEAR")}");
        }
    }

    void Update()
    {
        if (enemiesInRange.Count == 0)
            return;

        foreach (var soldier in squad.soldiers)
        {
            if (soldier == null) continue;

            UnitCombat combat = soldier.GetComponent<UnitCombat>();
            if (combat == null || combat.combatDisabled) continue;

            UnitCombat closest = null;
            float shortest = Mathf.Infinity;

            foreach (var enemy in enemiesInRange)
            {
                if (enemy == null) continue;

                float dist = Vector3.Distance(soldier.position, enemy.transform.position);
                if (dist < shortest)
                {
                    shortest = dist;
                    closest = enemy;
                }
            }

            if (closest != null)
            {
                Debug.Log($"{soldier.name} → Targeting {closest.name}");

                combat.SetTarget(closest.transform);

                if (shortest > engageDistance)
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
