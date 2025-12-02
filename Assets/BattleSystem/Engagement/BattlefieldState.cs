using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BattlefieldState
{
    public List<SquadState> squads = new List<SquadState>();
    
    public Vector3? playerGeneralPosition;
    public Vector3? enemyGeneralPosition;
    public Quaternion? playerGeneralRotation;
    public Quaternion? enemyGeneralRotation;
}

[System.Serializable]
public class SquadState
{
    public string owner;       
    public string unitType;    
    public int squadID;
    public List<Vector3> soldierPositions = new List<Vector3>();
}