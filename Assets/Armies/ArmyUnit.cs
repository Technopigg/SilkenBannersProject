using System;

/// <summary>
/// Data-only class used by ArmyToken 
/// </summary>
[Serializable]
public class ArmyUnit
{
    public string type;
    public int count;
    public bool isLockedInBattle = false;
    public int teamID = -1;
    
    public ArmyUnit() { }

    public ArmyUnit(string type, int count)
    {
        this.type = type;
        this.count = count;
    }
}