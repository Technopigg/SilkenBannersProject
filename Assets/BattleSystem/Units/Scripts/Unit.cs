using UnityEngine;

/// <summary>
/// Small component that identifies a soldier/unit at runtime (team, battle lock, etc).
/// </summary>
public class Unit : MonoBehaviour
{
    public int teamID = -1;
    public bool isLockedInBattle = false;
    public UnitMovement movement;
    public UnitCombat combat;
    public UnitHealth health;

    void Awake()
    {
        movement = GetComponent<UnitMovement>();
        combat = GetComponent<UnitCombat>();
        health = GetComponent<UnitHealth>();
    }
}