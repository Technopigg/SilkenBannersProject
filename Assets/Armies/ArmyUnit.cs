using UnityEngine;

[System.Serializable]
public class ArmyUnit
{
    public string type;          
    public int count;           
    public bool isLockedInBattle = false;

    public ArmyUnit(string type, int count)
    {
        this.type = type;
        this.count = count;
    }
}