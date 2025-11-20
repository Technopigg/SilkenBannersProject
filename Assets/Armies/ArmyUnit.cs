using UnityEngine;

[System.Serializable]
public class ArmyUnit
{
    public string type;           // e.g., "Infantry", "Archer", "Cavalry"
    public int count;             // number of soldiers in this unit type
    public bool isLockedInBattle = false;

    public ArmyUnit(string type, int count)
    {
        this.type = type;
        this.count = count;
    }
}