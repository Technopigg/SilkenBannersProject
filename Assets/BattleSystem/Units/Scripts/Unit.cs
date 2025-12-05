using UnityEngine;

/// <summary>
/// Small component that identifies a soldier/unit at runtime (team, battle lock, etc).
/// Provides quick references to combat/movement/health components.
/// </summary>
public class Unit : MonoBehaviour
{
    public int teamID = -1;
    public bool isLockedInBattle = false;

    public UnitMovement movement { get; private set; }
    public UnitCombat combat { get; private set; }
    public UnitHealth health { get; private set; }
    public UnitStats stats { get; private set; }

    void Awake()
    {
        movement = GetComponent<UnitMovement>();
        combat = GetComponent<UnitCombat>();
        health = GetComponent<UnitHealth>();
        stats = GetComponent<UnitStats>();

        if (movement == null) Debug.LogWarning($"{name}: No UnitMovement found!");
        if (combat == null) Debug.LogWarning($"{name}: No UnitCombat found!");
        if (health == null) Debug.LogWarning($"{name}: No UnitHealth found!");
        if (stats == null) Debug.LogWarning($"{name}: No UnitStats found!");
    }
}