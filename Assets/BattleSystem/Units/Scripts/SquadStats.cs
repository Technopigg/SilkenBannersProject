using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tracks all members of a squad and provides aggregated stats (like morale)
/// </summary>
public class SquadStats : MonoBehaviour
{
    public List<UnitStats> members = new List<UnitStats>();

    void Awake()
    {
        members.Clear();

        foreach (Transform t in transform)
        {
            UnitStats stats = t.GetComponent<UnitStats>();
            if (stats != null)
                members.Add(stats);
        }
    }

    public float GetAverageMorale()
    {
        if (members.Count == 0) return 1f;

        float total = 0f;
        foreach (var m in members)
            total += m.morale;

        return total / members.Count;
    }

    public int GetAliveCount()
    {
        int count = 0;
        foreach (var m in members)
            if (m.IsAlive()) count++;
        return count;
    }
}