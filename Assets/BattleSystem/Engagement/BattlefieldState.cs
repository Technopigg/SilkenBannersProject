using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BattlefieldState
{
    public List<SquadState> squads = new List<SquadState>();
    
    // --- Champion positions & rotations ---
    public Vector3? playerChampionPosition;
    public Vector3? enemyChampionPosition;
    public Quaternion? playerChampionRotation;
    public Quaternion? enemyChampionRotation;
}

[System.Serializable]
public class SquadState
{
    public string owner;       
    public string unitType;    
    public int squadID;
    public List<Vector3> soldierPositions = new List<Vector3>();
}