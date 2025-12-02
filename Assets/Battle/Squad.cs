using System.Collections.Generic;
using UnityEngine;

public class Squad : MonoBehaviour
{
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

    void Awake()
    {
        soldiers.Clear();
        foreach (Transform child in transform)
        {
            if (child != null && child.GetComponent<UnitSelection>() != null)
            {
                soldiers.Add(child);
            }
        }
    }

    /// <summary>
    /// NEW: Simple move order (auto-facing)
    /// </summary>
    public void MoveSquad(Vector3 destination)
    {
        Vector3 facingDir = (destination - transform.position);
        facingDir.y = 0f;

        if (facingDir.sqrMagnitude < 0.001f)
            facingDir = transform.forward;

        MoveSquad(destination, facingDir.normalized);
    }

    /// <summary>
    /// Issue a move order to the entire squad, with a facing direction.
    /// facingDir should be a normalized vector (world-space).
    /// </summary>
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
            {
                speedFactor = Mathf.Clamp(centerDist / arriveSlowDistance, minSpeedFactor, 1f);
            }

            UnitMovement mover = soldier.GetComponent<UnitMovement>();
            if (mover != null)
            {
                mover.SetMovementTarget(soldierTarget, squadBaseSpeed * speedFactor);
            }
        }
    }

    public void SetSelected(bool selected)
    {
        foreach (Transform soldier in soldiers)
        {
            if (soldier == null) continue;

            UnitSelection sel = soldier.GetComponent<UnitSelection>();
            if (sel != null)
            {
                sel.SetSelected(selected);
            }
        }
    }

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
