using UnityEngine;
using System.Collections.Generic;

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
}