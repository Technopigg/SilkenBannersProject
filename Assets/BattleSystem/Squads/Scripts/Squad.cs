using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Squad : MonoBehaviour
{
    [Header("Team Info")]
    public int teamID = -1;

    [Header("Squad Info")]
    public int squadID;
    public string owner;
    public string unitType;
    public List<Transform> soldiers = new List<Transform>();

    [Header("Formation Settings")]
    public int formationWidth = 5;
    public float spacing = 2f;
    public float arriveSlowDistance = 6f;
    public float minSpeedFactor = 0.3f;

    [Header("Movement")]
    public float squadBaseSpeed = 3.5f;

    // ------------------------------------------------------------
    // HEALTH SYSTEM
    // ------------------------------------------------------------
    [Header("Squad Health (Auto-Calculated)")]
    public float totalMaxHealth = 0f;    
    public float totalCurrentHealth = 0f; 
    public bool isSelected = false;

    void Awake()
    {
        if (soldiers == null) soldiers = new List<Transform>();
        if (soldiers.Count == 0)
        {
            foreach (Transform child in transform)
            {
                if (child != null && child.GetComponent<Unit>() != null)
                {
                    soldiers.Add(child);
                }
            }
        }

        RecalculateMaxHealth();
        RecalculateCurrentHealth();
    }

    // ------------------------------------------------------------
    // HEALTH CALCULATION
    // ------------------------------------------------------------
    public void RecalculateMaxHealth()
    {
        totalMaxHealth = 0f;

        foreach (Transform soldier in soldiers)
        {
            if (soldier == null) continue;

            UnitHealth u = soldier.GetComponent<UnitHealth>();
            if (u != null)
                totalMaxHealth += u.maxHealth;
        }
    }

    public void RecalculateCurrentHealth()
    {
        totalCurrentHealth = 0f;

        foreach (Transform soldier in soldiers)
        {
            if (soldier == null) continue;

            UnitHealth u = soldier.GetComponent<UnitHealth>();
            if (u != null && u.currentHealth > 0)
                totalCurrentHealth += u.currentHealth;
        }
    }

    // Call this whenever a unit dies
    public void NotifyUnitDied(UnitHealth deadUnit)
    {
        RecalculateCurrentHealth();
    }

    // ------------------------------------------------------------
    // TEAM SYSTEM
    // ------------------------------------------------------------
    public void SetTeam(int id)
    {
        teamID = id;

        foreach (Transform soldier in soldiers)
        {
            if (soldier == null) continue;
            var unitComp = soldier.GetComponent<Unit>();
            if (unitComp != null)
                unitComp.teamID = id;
        }
    }

    public void ApplyTeamColor(Color c)
    {
        foreach (Transform soldier in soldiers)
        {
            if (soldier == null) continue;
            Renderer r = soldier.GetComponentInChildren<Renderer>();
            if (r != null) r.material.color = c;
        }
    }

    // ------------------------------------------------------------
    // MOVEMENT
    // ------------------------------------------------------------
    public void MoveSquad(Vector3 destination)
    {
        Vector3 facingDir = destination - GetSquadCenter();
        facingDir.y = 0f;

        if (facingDir.sqrMagnitude < 0.001f)
            facingDir = transform.forward;

        MoveSquad(destination, facingDir.normalized);
    }

    public void MoveSquad(Vector3 destination, Vector3 facingDir)
    {
        if (soldiers == null || soldiers.Count == 0) return;

        facingDir.y = 0f;
        if (facingDir.sqrMagnitude < 0.001f)
            facingDir = transform.forward;

        facingDir.Normalize();

        Quaternion formationRot = Quaternion.LookRotation(facingDir, Vector3.up);
        Vector3 currentCenter = GetSquadCenter();
        float centerDist = Vector3.Distance(currentCenter, destination);

        int count = soldiers.Count;
        int rows = Mathf.CeilToInt((float)count / formationWidth);
        float halfWidth = ((formationWidth - 1) * spacing) * 0.5f;
        float halfDepth = ((rows - 1) * spacing) * 0.5f;

        for (int i = 0; i < count; i++)
        {
            Transform soldier = soldiers[i];
            if (soldier == null) continue;

            int row = i / formationWidth;
            int col = i % formationWidth;

            Vector3 localOffset = new Vector3(col * spacing - halfWidth, 0f, row * spacing - halfDepth);
            Vector3 worldOffset = formationRot * localOffset;
            Vector3 soldierTarget = destination + worldOffset;

            float speedFactor = 1f;
            if (centerDist <= arriveSlowDistance)
                speedFactor = Mathf.Clamp(centerDist / arriveSlowDistance, minSpeedFactor, 1f);

            UnitMovement mover = soldier.GetComponent<UnitMovement>();
            if (mover != null)
                mover.SetMovementTarget(soldierTarget, squadBaseSpeed * speedFactor);
        }
    }

    // ------------------------------------------------------------
    // SELECTION
    // ------------------------------------------------------------
    public void SetSelected(bool selected)
    {
        isSelected = selected;

        foreach (Transform soldier in soldiers)
        {
            if (soldier == null) continue;

            UnitSelection sel = soldier.GetComponent<UnitSelection>();
            if (sel != null) sel.SetSelected(selected);
        }
    }

    // ------------------------------------------------------------
    // CENTER
    // ------------------------------------------------------------
    public Vector3 GetSquadCenter()
    {
        if (soldiers == null || soldiers.Count == 0)
            return transform.position;

        Vector3 sum = Vector3.zero;
        int count = 0;
        foreach (Transform soldier in soldiers)
        {
            if (soldier != null)
            {
                sum += soldier.position;
                count++;
            }
        }

        return count > 0 ? sum / count : transform.position;
    }
}
