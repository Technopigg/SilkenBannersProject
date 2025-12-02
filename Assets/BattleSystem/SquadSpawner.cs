using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns squads and generals OR restores them if a saved BattlefieldState exists.
/// Fully updated to avoid:
/// - Double spawns
/// - Missing reference crashes
/// - Camera/spawner timing issues
/// Works with BattleRootController.
/// </summary>
public class SquadSpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform playerSpawnPoint;
    public Transform enemySpawnPoint;

    [Header("Prefabs")]
    public GameObject squadPrefab;       // (May be unused)
    public GameObject generalPrefab;

    [Header("Unit Prefabs")]
    [SerializeField] private GameObject spearmanPrefab;
    [SerializeField] private GameObject archerPrefab;

    public GameObject PlayerGeneral { get; private set; }
    public GameObject EnemyGeneral { get; private set; }

    private bool hasSpawned = false;      // ❗ Prevent double spawns

    // ────────────────────────────────────────────────────────────────
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

        // 1) RESTORE STATE
        BattlefieldState saved = BattleManager.Instance.GetLastBattlefieldState();
        if (saved != null && saved.squads != null && saved.squads.Count > 0)
        {
            Debug.Log("SquadSpawner: Restoring battlefield from saved state...");
            RestoreBattlefield(saved);
            return;
        }

        // 2) SPAWN FRESH
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

        Debug.Log("SquadSpawner: Spawning armies...");

        SpawnArmy(playerToken, playerSpawnPoint, "Player");
        SpawnArmy(enemyToken, enemySpawnPoint, "Enemy");
    }

    // ────────────────────────────────────────────────────────────────
    // PREFAB GETTER
    // ────────────────────────────────────────────────────────────────
    private GameObject GetPrefabForType(string unitType)
    {
        if (string.IsNullOrWhiteSpace(unitType))
            return null;

        switch (unitType.Trim().ToLower())
        {
            case "spearman": return spearmanPrefab;
            case "archer": return archerPrefab;
            default:
                Debug.LogError($"Unit type '{unitType}' not mapped to a prefab!");
                return null;
        }
    }

    // ────────────────────────────────────────────────────────────────
    // SPAWN ARMY
    // ────────────────────────────────────────────────────────────────
    private void SpawnArmy(ArmyToken token, Transform spawnPoint, string owner)
    {
        foreach (var unit in token.composition)
        {
            SpawnSquad(spawnPoint.position, unit, owner, unit.count);
        }

        // Spawn General
        Vector3 genPos = spawnPoint.position + Vector3.forward * 2f;

        GameObject generalObj = Instantiate(generalPrefab, genPos, Quaternion.identity);
        var movement = generalObj.GetComponent<Player3PMovement>();

        if (owner == "Player")
        {
            PlayerGeneral = generalObj;
            generalObj.tag = "PlayerUnit";

            if (movement != null) movement.isPlayerControlled = true;
            Debug.Log("Player General spawned.");
        }
        else
        {
            EnemyGeneral = generalObj;
            generalObj.tag = "EnemyUnit";

            if (movement != null) movement.isPlayerControlled = false;
            Debug.Log("Enemy General spawned.");
        }
    }

    // ────────────────────────────────────────────────────────────────
    // SPAWN SQUAD
    // ────────────────────────────────────────────────────────────────
    private void SpawnSquad(Vector3 position, ArmyUnit unit, string owner, int count)
    {
        GameObject prefab = GetPrefabForType(unit.type);
        if (prefab == null)
        {
            Debug.LogError($"Cannot spawn squad: prefab missing for type {unit.type}");
            return;
        }

        GameObject squadObj = new GameObject($"{owner}_Squad_{unit.type}");
        Squad squad = squadObj.AddComponent<Squad>();

        squad.squadID = Random.Range(1000, 9999);
        squad.owner = owner;
        squad.unitType = unit.type;

        squad.soldiers = new List<Transform>();

        for (int i = 0; i < Mathf.Max(1, count); i++)
        {
            Vector3 offset = position + new Vector3(i * 1.5f, 0, 0);
            GameObject soldier = Instantiate(prefab, offset, Quaternion.identity);
            soldier.transform.SetParent(squadObj.transform);

            squad.soldiers.Add(soldier.transform);
        }

        Debug.Log($"{owner} Squad {squad.squadID} spawned with {count} {unit.type}(s)");
    }

    // ────────────────────────────────────────────────────────────────
    // RESTORE BATTLEFIELD
    // ────────────────────────────────────────────────────────────────
    private void RestoreBattlefield(BattlefieldState state)
    {
        if (state.squads == null) return;

        foreach (var ss in state.squads)
        {
            GameObject prefab = GetPrefabForType(ss.unitType);
            if (prefab == null) continue;

            GameObject squadObj = new GameObject($"{ss.owner}_Squad_{ss.squadID}_{ss.unitType}");
            Squad squad = squadObj.AddComponent<Squad>();

            squad.squadID = ss.squadID;
            squad.owner = ss.owner;
            squad.unitType = ss.unitType;
            squad.soldiers = new List<Transform>();

            foreach (var pos in ss.soldierPositions)
            {
                GameObject soldier = Instantiate(prefab, pos, Quaternion.identity);
                soldier.transform.SetParent(squadObj.transform);
                squad.soldiers.Add(soldier.transform);
            }

            Debug.Log($"Restored Squad {ss.squadID} ({ss.unitType}) with {ss.soldierPositions.Count} soldiers.");

        }

        // Restore Player General
        if (state.playerGeneralPosition.HasValue)
        {
            PlayerGeneral = Instantiate(
                generalPrefab,
                state.playerGeneralPosition.Value,
                state.playerGeneralRotation ?? Quaternion.identity
            );
            PlayerGeneral.tag = "PlayerUnit";
            var mv = PlayerGeneral.GetComponent<Player3PMovement>();
            if (mv != null) mv.isPlayerControlled = true;
            Debug.Log("Restored Player General.");
        }

        // Restore Enemy General
        if (state.enemyGeneralPosition.HasValue)
        {
            EnemyGeneral = Instantiate(
                generalPrefab,
                state.enemyGeneralPosition.Value,
                state.enemyGeneralRotation ?? Quaternion.identity
            );
            EnemyGeneral.tag = "EnemyUnit";
            var mv = EnemyGeneral.GetComponent<Player3PMovement>();
            if (mv != null) mv.isPlayerControlled = false;
            Debug.Log("Restored Enemy General.");
        }
    }

    // ────────────────────────────────────────────────────────────────
    // SAVE STATE (returns BattlefieldState!)
    // ────────────────────────────────────────────────────────────────
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
                if (t != null)
                    ss.soldierPositions.Add(t.position);

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

        Debug.Log("SquadSpawner: Saved battlefield state.");

        return state;
    }

    void OnDestroy()
    {
        // Avoid null errors during shutdown
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.SaveBattlefieldState(SaveBattlefieldStateNow());
        }
    }
}
