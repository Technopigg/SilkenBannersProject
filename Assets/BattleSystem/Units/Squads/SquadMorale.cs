using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoraleState
{
    Steady,
    Shaken,
    Wavering,
    Routing
}

public class SquadMorale : MonoBehaviour
{
    public Squad squad;

    [Header("Morale Settings")]
    public float maxMorale = 100f;
    public float morale = 100f;
    public MoraleState state = MoraleState.Steady;

    [Header("Loss Impact")]
    public float lossPerCasualty = 5f;
    public float lossWhenFlanked = 10f;
    public float lossWhenLosingCombat = 0.8f; // per second

    [Header("Regeneration")]
    public float regenRate = 2f; // per second when not fighting

    [Header("Routing Settings")]
    public float shakenThreshold = 60f;
    public float waveringThreshold = 30f;
    public float routingThreshold = 15f;

    private SquadCombatController combat;

    void Awake()
    {
        if (squad == null) squad = GetComponent<Squad>();
        combat = GetComponent<SquadCombatController>();
    }

    void Update()
    {
        UpdateMoraleState();

        if (!combat.isEngaged)
        {
            morale = Mathf.Min(maxMorale, morale + regenRate * Time.deltaTime);
        }
        else
        {
            // losing? apply pressure
            if (combat.enemyCount > squad.soldiers.Count)
            {
                morale -= lossWhenLosingCombat * Time.deltaTime;
            }
        }

        morale = Mathf.Clamp(morale, 0f, maxMorale);
    }

    public void ApplyCasualty()
    {
        morale -= lossPerCasualty;
    }

    public void ApplyFlankShock()
    {
        morale -= lossWhenFlanked;
    }

    private void UpdateMoraleState()
    {
        if (morale <= routingThreshold)
        {
            if (state != MoraleState.Routing)
                TriggerRouting();
        }
        else if (morale <= waveringThreshold)
        {
            state = MoraleState.Wavering;
        }
        else if (morale <= shakenThreshold)
        {
            state = MoraleState.Shaken;
        }
        else
        {
            state = MoraleState.Steady;
        }
    }

    private void TriggerRouting()
    {
        state = MoraleState.Routing;

        Vector3 fleeDir = -transform.forward;
        fleeDir.y = 0f;

        Vector3 fleePoint = transform.position + fleeDir * 30f;

        squad.MoveSquad(fleePoint);

        foreach (Transform soldier in squad.soldiers)
        {
            UnitCombat uc = soldier.GetComponent<UnitCombat>();
            if (uc != null) uc.DisableCombatTemporarily();
        }
    }
}
