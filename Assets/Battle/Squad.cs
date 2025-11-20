using System.Collections.Generic;
using UnityEngine;

public class Squad : MonoBehaviour
{
    [Header("Squad Info")]
    public int squadID;                        // unique identifier
    public string owner;                       // "Player" or "Enemy"
    public string unitType;                    // e.g. "Infantry", "Archer", "Cavalry"
    public List<Transform> soldiers = new List<Transform>();

    [Header("Formation Settings")]
    public int formationWidth = 5;             // soldiers per row
    public float spacing = 2f;                 // distance between soldiers

    void Awake()
    {
        // Auto-populate soldiers list with children that have UnitSelection
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
    /// Issue a move order to the entire squad.
    /// </summary>
    public void MoveSquad(Vector3 destination)
    {
        if (soldiers == null || soldiers.Count == 0) return;

        int i = 0;
        foreach (Transform soldier in soldiers)
        {
            if (soldier == null) continue;

            // Grid formation offset
            int row = i / formationWidth;
            int col = i % formationWidth;
            Vector3 offset = new Vector3(col * spacing, 0f, row * spacing);

            Vector3 soldierDest = destination + offset;

            // Each soldier must have a UnitMovement script
            UnitMovement mover = soldier.GetComponent<UnitMovement>();
            if (mover != null)
            {
                mover.SetDestination(soldierDest);
            }

            i++;
        }
    }

    /// <summary>
    /// Highlight squad when selected (optional visual feedback).
    /// </summary>
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

    /// <summary>
    /// Get the average world position of all soldiers in the squad.
    /// Useful for centering the RTS camera.
    /// </summary>
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