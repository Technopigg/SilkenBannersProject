using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns squads and generals or restores them if a saved BattlefieldState exists.
/// Fully supports the new ArmyToken system and Total War–style squad combat.
/// </summary>
public class SquadSpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform playerSpawnPoint;
    public Transform enemySpawnPoint;

    [Header("Prefabs")]
    public GameObject squadPrefab; 
    public GameObject generalPrefab;

    [Header("Unit Prefabs")]
    [SerializeField] private GameObject spearmanPrefab;
    [SerializeField] private GameObject archerPrefab;

    public GameObject PlayerGeneral { get; private set; }
    public GameObject EnemyGeneral { get; private set; }

    private bool hasSpawned = false;

    void Start()
    {
        if (hasSpawned)
        {
            Debug.LogWarning("SquadSpawner.Start() called twice — ignoring.");
            return;
        }

        hasSpawned = true;

        if (BattleManager.Instance == null)
        {
            Debug.LogError("SquadSpawner: No BattleManager found!");
            return;
        }

        BattlefieldState saved = BattleManager.Instance.GetLastBattlefieldState();
        if (saved != null && saved.squads != null && saved.squads.Count > 0)
        {
            RestoreBattlefield(saved);
            return;
        }

        ArmyToken playerToken = BattleManager.Instance.GetPlayerToken();
        ArmyToken enemyToken = BattleManager.Instance.GetEnemyToken();

        if (playerToken == null || enemyToken == null)
        {
            Debug.LogWarning("SquadSpawner: Missing tokens — cannot spawn armies.");
            return;
        }

        if (playerSpawnPoint == null || enemySpawnPoint == null)
        {
            Debug.LogError("SquadSpawner: Missing spawn points!");
            return;
        }

        SpawnArmy(playerToken, playerSpawnPoint, "Player", 0);
        SpawnArmy(enemyToken, enemySpawnPoint, "Enemy", 1);
    }

    private GameObject GetPrefabForType(string unitType)
    {
        if (string.IsNullOrWhiteSpace(unitType)) return null;

        switch (unitType.Trim().ToLower())
        {
            case "spearman": return spearmanPrefab;
            case "archer": return archerPrefab;
            default:
                Debug.LogError($"Unit type '{unitType}' not mapped to a prefab!");
                return null;
        }
    }

    private void SpawnArmy(ArmyToken token, Transform spawnPoint, string owner, int teamID)
    {
        foreach (var unit in token.composition)
        {
            SpawnSquad(spawnPoint.position, unit, owner, teamID, unit.count);
        }
        Vector3 genPos = spawnPoint.position + Vector3.forward * 2f;
        GameObject genPrefabToUse = (token.generalPrefab != null) ? token.generalPrefab : generalPrefab;
        GameObject generalObj = Instantiate(genPrefabToUse, genPos, Quaternion.identity);

        generalObj.tag = (teamID == 0) ? "PlayerUnit" : "EnemyUnit";
        SetLayerRecursive(generalObj, (teamID == 0)
            ? LayerMask.NameToLayer("PlayerSoldier")
            : LayerMask.NameToLayer("EnemySoldier"));

        var movement = generalObj.GetComponent<Player3PMovement>();
        if (movement != null) movement.isPlayerControlled = (teamID == 0);

        if (teamID == 0) PlayerGeneral = generalObj;
        else EnemyGeneral = generalObj;
    }

    private void SpawnSquad(Vector3 position, ArmyUnit unit, string owner, int teamID, int count)
    {
        GameObject prefab = GetPrefabForType(unit.type);
        if (prefab == null)
        {
            Debug.LogError($"Cannot spawn squad: prefab missing for type {unit.type}");
            return;
        }

        GameObject squadObj = new GameObject($"{owner}_Squad_{unit.type}");
        Squad squad = squadObj.AddComponent<Squad>();
        Rigidbody rb = squadObj.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        SphereCollider detection = squadObj.AddComponent<SphereCollider>();
        detection.isTrigger = true;
        detection.radius = 20f; 

        SquadCombatController ctrl = squadObj.AddComponent<SquadCombatController>();
        ctrl.squad = squad;

        squad.teamID = teamID;
        squad.owner = owner;
        squad.unitType = unit.type;
        squad.squadID = Random.Range(1000, 9999);
        squad.soldiers = new List<Transform>();

        int layerToAssign = (teamID == 0) ? LayerMask.NameToLayer("PlayerSoldier") : LayerMask.NameToLayer("EnemySoldier");
        string tagToAssign = (teamID == 0) ? "PlayerUnit" : "EnemyUnit";

        for (int i = 0; i < Mathf.Max(1, count); i++)
        {
            Vector3 offset = position + new Vector3(i * 1.5f, 0, 0);
            GameObject soldier = Instantiate(prefab, offset, Quaternion.identity);
            soldier.transform.SetParent(squadObj.transform);

            var unitComp = soldier.GetComponent<Unit>();
            if (unitComp != null) unitComp.teamID = teamID;
            soldier.tag = tagToAssign;
            SetLayerRecursive(soldier, layerToAssign);
            UnitCombat uc = soldier.GetComponent<UnitCombat>();
            if (uc != null)
                uc.squadRoot = squad;

            squad.soldiers.Add(soldier.transform);
        }
    }

    private void RestoreBattlefield(BattlefieldState state)
    {
        if (state.squads == null) return;

        foreach (var ss in state.squads)
        {
            GameObject prefab = GetPrefabForType(ss.unitType);
            if (prefab == null) continue;
            GameObject squadObj = new GameObject($"{ss.owner}_Squad_{ss.squadID}_{ss.unitType}");
            Squad squad = squadObj.AddComponent<Squad>();
            Rigidbody rb = squadObj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            SphereCollider detection = squadObj.AddComponent<SphereCollider>();
            detection.isTrigger = true;
            detection.radius = 20f;

            SquadCombatController ctrl = squadObj.AddComponent<SquadCombatController>();
            ctrl.squad = squad;

            squad.teamID = (ss.owner == "Player") ? 0 : 1;
            squad.owner = ss.owner;
            squad.unitType = ss.unitType;
            squad.squadID = ss.squadID;
            squad.soldiers = new List<Transform>();

            int layerToAssign = (squad.teamID == 0) ? LayerMask.NameToLayer("PlayerSoldier") : LayerMask.NameToLayer("EnemySoldier");
            string tagToAssign = (squad.teamID == 0) ? "PlayerUnit" : "EnemyUnit";

            foreach (var pos in ss.soldierPositions)
            {
                GameObject soldier = Instantiate(prefab, pos, Quaternion.identity);
                soldier.transform.SetParent(squadObj.transform);

                var unitComp = soldier.GetComponent<Unit>();
                if (unitComp != null) unitComp.teamID = squad.teamID;

                soldier.tag = tagToAssign;
                SetLayerRecursive(soldier, layerToAssign);
                UnitCombat uc = soldier.GetComponent<UnitCombat>();
                if (uc != null)
                    uc.squadRoot = squad;

                squad.soldiers.Add(soldier.transform);
            }
        }
        
        if (state.playerGeneralPosition.HasValue)
        {
            PlayerGeneral = Instantiate(
                generalPrefab,
                state.playerGeneralPosition.Value,
                state.playerGeneralRotation ?? Quaternion.identity
            );
            PlayerGeneral.tag = "PlayerUnit";
            SetLayerRecursive(PlayerGeneral, LayerMask.NameToLayer("PlayerSoldier"));
        }

        if (state.enemyGeneralPosition.HasValue)
        {
            EnemyGeneral = Instantiate(
                generalPrefab,
                state.enemyGeneralPosition.Value,
                state.enemyGeneralRotation ?? Quaternion.identity
            );
            EnemyGeneral.tag = "EnemyUnit";
            SetLayerRecursive(EnemyGeneral, LayerMask.NameToLayer("EnemySoldier"));
        }
    }

    private void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursive(child.gameObject, layer);
    }

    public BattlefieldState SaveBattlefieldStateNow()
    {
        BattlefieldState state = new BattlefieldState();
        state.squads = new List<SquadState>();

        foreach (var squad in FindObjectsOfType<Squad>())
        {
            SquadState ss = new SquadState
            {
                squadID = squad.squadID,
                owner = squad.owner,
                unitType = squad.unitType,
                soldierPositions = new List<Vector3>()
            };

            foreach (var t in squad.soldiers)
                if (t != null) ss.soldierPositions.Add(t.position);

            state.squads.Add(ss);
        }

        if (PlayerGeneral != null)
        {
            state.playerGeneralPosition = PlayerGeneral.transform.position;
            state.playerGeneralRotation = PlayerGeneral.transform.rotation;
        }

        if (EnemyGeneral != null)
        {
            state.enemyGeneralPosition = EnemyGeneral.transform.position;
            state.enemyGeneralRotation = EnemyGeneral.transform.rotation;
        }

        return state;
    }

    void OnDestroy()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.SaveBattlefieldState(SaveBattlefieldStateNow());
        }
    }
}
