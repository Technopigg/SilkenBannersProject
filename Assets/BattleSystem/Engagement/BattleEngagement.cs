using UnityEngine;

[System.Serializable]
public class BattleEngagement
{
    public ArmyToken player;
    public ArmyToken enemy;
    public Vector3 worldPosition;
    public bool isResolved = false;

    public BattleEngagement(ArmyToken playerToken, ArmyToken enemyToken)
    {
        player = playerToken;
        enemy = enemyToken;

        worldPosition = Vector3.zero;
        isResolved = false;
    }
}