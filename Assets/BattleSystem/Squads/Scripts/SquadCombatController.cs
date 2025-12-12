using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class SquadCombatController : MonoBehaviour
{
    public Squad squad;
    [Tooltip("Small buffer added to automatically computed detection radius")]
    public float detectionBuffer = 1f;
    
    public float engageDistance = 0.5f;

    public bool isEngaged = false;
    public readonly List<SquadCombatController> enemySquadsInRange = new();

    private SphereCollider col;

    void Awake()
    {
        if (squad == null)
            squad = GetComponent<Squad>();

        col = GetComponent<SphereCollider>();
        if (col == null)
            col = gameObject.AddComponent<SphereCollider>();

        col.isTrigger = true;

        if (col.radius <= 0f)
            col.radius = 1f;
    }

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        else
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        UpdateColliderCenterAndRadius();
        
        PerformInitialOverlapScan();
    }

    void LateUpdate()
    {
        UpdateColliderCenterOnly();
    }
    
    private void UpdateColliderCenterAndRadius()
    {
        if (squad == null || col == null) return;

        Vector3 worldCenter = squad.GetSquadCenter();
        col.center = transform.InverseTransformPoint(worldCenter);
        
        float maxDist = 0f;
        foreach (var s in squad.soldiers)
        {
            if (s == null) continue;
            float d = Vector3.Distance(worldCenter, s.position);
            if (d > maxDist) maxDist = d;
        }
        
        float desiredRadius = Mathf.Max(col.radius, maxDist + detectionBuffer);
        col.radius = desiredRadius;
    }
    private void UpdateColliderCenterOnly()
    {
        if (squad == null || col == null) return;
        Vector3 worldCenter = squad.GetSquadCenter();
        col.center = transform.InverseTransformPoint(worldCenter);
    }
    
    private void PerformInitialOverlapScan()
    {
        if (squad == null || col == null) return;

        Vector3 worldCenter = squad.GetSquadCenter();
        float worldRadius = GetWorldRadius(col);

        Collider[] hits = Physics.OverlapSphere(worldCenter, worldRadius);
        foreach (var hit in hits)
        {
            if (hit == null) continue;
            if (hit == col) continue;
            if (hit.TryGetComponent<SquadCombatController>(out var other))
            {
                if (other == this) continue;
                if (other.squad == null) continue;
                if (other.squad.teamID == squad.teamID) continue;

                if (!enemySquadsInRange.Contains(other))
                {
                    enemySquadsInRange.Add(other);
                    Debug.Log($"{squad.name} (initial scan) → Enemy squad detected: {other.squad.name}");
                }
            }
        }

        UpdateEngagementState();
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

        if (enemySquadCtrl.squad == null) return;
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
        if (!isEngaged || squad == null || squad.soldiers.Count == 0) return;

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
        Vector3 worldCenter = (col != null) ? col.transform.TransformPoint(col.center) : transform.position;
        Gizmos.DrawWireSphere(worldCenter, (col != null) ? GetWorldRadius(col) : 1f);
        if (squad != null && squad.soldiers != null && squad.soldiers.Count > 0)
        {
            Gizmos.color = Color.yellow;
            Vector3 dynCenter = squad.GetSquadCenter();
            Gizmos.DrawWireSphere(dynCenter, (col != null) ? GetWorldRadius(col) : 1f);
        }
    }
}
