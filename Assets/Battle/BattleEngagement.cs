using UnityEngine;

[System.Serializable]
public class BattleEngagement
{
    public ArmyToken player;
    public ArmyToken enemy;

    // Optional: where the battle occurred on the map 
    public Vector3 worldPosition;

    // NEW RESTORED FIELD
    // WorldMapBattleManager expects this to exist
    public bool isResolved = false;

    public BattleEngagement(ArmyToken playerToken, ArmyToken enemyToken)
    {
        player = playerToken;
        enemy = enemyToken;

        worldPosition = Vector3.zero;
        isResolved = false;
    }
}