using UnityEngine;

public class EncounterDetection : MonoBehaviour
{
    private ArmyToken armyToken;

    void Awake()
    {
        armyToken = GetComponent<ArmyToken>();
    }

    void OnTriggerEnter(Collider other)
    {
        ArmyToken otherToken = other.GetComponent<ArmyToken>();
        if (otherToken == null || otherToken == armyToken)
            return;

        Debug.Log($"Encounter: {armyToken.name} vs {otherToken.name}");

        // stop both armies in place
        armyToken.SetTarget(armyToken.transform.position);
        otherToken.SetTarget(otherToken.transform.position);

        // Tell the NEW battle manager to create & store engagement
        BattleManager.Instance.CreateEngagementAndStart(armyToken, otherToken);
    }
}