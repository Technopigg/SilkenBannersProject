using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class SquadCombatController : MonoBehaviour
{
    public Squad squad;                         // Assigned automatically
    public float engageDistance = 2.5f;

    public int enemyCount = 0;                  // ✔ Required by morale system

    private readonly List<UnitCombat> enemiesInRange = new();

    void Awake()
    {
        if (squad == null)
            squad = GetComponent<Squad>();

        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 7f; // You can tweak
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<UnitCombat>(out var unit))
        {
            if (unit.squadRoot != squad)
            {
                if (!enemiesInRange.Contains(unit))
                    enemiesInRange.Add(unit);

                enemyCount++;   // ✔ Morale uses this
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<UnitCombat>(out var unit))
        {
            if (enemiesInRange.Contains(unit))
                enemiesInRange.Remove(unit);

            if (unit.squadRoot != squad && enemyCount > 0)
                enemyCount--;  // ✔ Morale uses this
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

            // find nearest enemy
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
                combat.SetTarget(closest.transform);

                if (shortest > engageDistance)
                {
                    combat.MoveTowardsTarget();
                }
                else
                {
                    combat.TryAttack();
                }
            }
        }
    }
}
