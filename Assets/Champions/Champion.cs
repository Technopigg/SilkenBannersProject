using UnityEngine;

[RequireComponent(typeof(UnitStats), typeof(UnitCombat), typeof(UnitMovement))]
public class Champion : MonoBehaviour
{
    [Header("Champion Stats")]
    public string championName = "Hero";
    public int level = 1;
    public int experience = 0;

    [Header("Runtime")]
    public bool isPlayerControlled = false;

    private UnitStats stats;
    private UnitCombat combat;
    private UnitMovement movement;

    void Awake()
    {
        stats = GetComponent<UnitStats>();
        combat = GetComponent<UnitCombat>();
        movement = GetComponent<UnitMovement>();
    }

    public void InitializeChampion(string name, int level, bool playerControlled = false)
    {
        championName = name;
        this.level = level;
        isPlayerControlled = playerControlled;

        if (movement != null)
            movement.enabled = true;

        if (combat != null)
            combat.EnableCombat();
    }

    public void GainExperience(int xp)
    {
        experience += xp;
    }
}