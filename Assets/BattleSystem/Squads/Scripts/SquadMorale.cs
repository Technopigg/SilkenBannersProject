using System.Collections.Generic;
using UnityEngine;

public enum MoraleState
{
    Steady,
    Shaken,
    Wavering,
    Routing
}

[DisallowMultipleComponent]
public class SquadMorale : MonoBehaviour
{
    public Squad squad;

    [Header("Morale Settings")]
    public float maxMorale = 100f;
    [SerializeField] private float morale = 100f;
    public MoraleState state = MoraleState.Steady;

    [Header("Loss Impact")]
    public float lossPerCasualty = 5f;
    public float lossWhenFlanked = 10f;
    public float lossWhenLosingCombat = 0.8f; 

    [Header("Regeneration")]
    public float regenRate = 2f; 

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
        if (squad == null) return;

        UpdateMoraleState();

        // Regenerate morale when not engaged
        if (combat == null || !combat.isEngaged)
        {
            morale = Mathf.Min(maxMorale, morale + regenRate * Time.deltaTime);
        }
        else
        {
            // Reduce morale if outnumbered by enemy squads
            int enemyUnits = 0;
            if (combat != null)
            {
                foreach (var enemySquad in combat.enemySquadsInRange)
                    if (enemySquad != null && enemySquad.squad != null)
                        enemyUnits += enemySquad.squad.soldiers.Count;

                if (enemyUnits > squad.soldiers.Count)
                {
                    morale -= lossWhenLosingCombat * Time.deltaTime;
                }
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

        if (squad == null || squad.soldiers.Count == 0) return;

        Vector3 fleeDir = -squad.transform.forward;
        fleeDir.y = 0f;

        Vector3 fleePoint = squad.GetSquadCenter() + fleeDir * 30f;
        squad.MoveSquad(fleePoint);

        foreach (Transform soldier in squad.soldiers)
        {
            if (soldier == null) continue;

            UnitCombat uc = soldier.GetComponent<UnitCombat>();
            if (uc != null) uc.DisableCombatTemporarily();
        }
    }

    public float GetMoralePercentage()
    {
        return morale / maxMorale;
    }
}
