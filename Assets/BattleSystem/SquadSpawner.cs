using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns squads and champions or restores them if a saved BattlefieldState exists.
/// </summary>
public class SquadSpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform playerSpawnPoint;
    public Transform enemySpawnPoint;

    [Header("Prefabs")]
    public GameObject squadPrefab;
    public GameObject championPrefab;

    [Header("Unit Prefabs")]
    [SerializeField] private GameObject spearmanPrefab;
    [SerializeField] private GameObject archerPrefab;

    [Header("UI Prefabs")]
    public GameObject squadHealthBarPrefab;

    public GameObject PlayerChampion { get; private set; }
    public GameObject EnemyChampion { get; private set; }

    private bool hasSpawned = false;
    private Canvas worldSpaceCanvas;

    void Start()
    {
        if (hasSpawned)
        {
            Debug.LogWarning("SquadSpawner.Start() called twice — ignoring.");
            return;
        }

        hasSpawned = true;
        
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (var c in canvases)
        {
            if (c.renderMode == RenderMode.WorldSpace)
            {
                worldSpaceCanvas = c;
                break;
            }
        }

        if (worldSpaceCanvas == null)
            Debug.LogWarning("No World-Space Canvas found. Health bars will still be created but not parented.");

        if (BattleManager.Instance == null)
        {
            Debug.LogError("No BattleManager found!");
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
            Debug.LogWarning("Missing tokens — cannot spawn armies.");
            return;
        }

        if (playerSpawnPoint == null || enemySpawnPoint == null)
        {
            Debug.LogError("Missing spawn points!");
            return;
        }

        SpawnArmy(playerToken, playerSpawnPoint, "Player", 0);
        SpawnArmy(enemyToken, enemySpawnPoint, "Enemy", 1);
    }

    private Vector3 GetCenterOfPositions(List<Vector3> positions)
    {
        if (positions == null || positions.Count == 0) return Vector3.zero;
        Vector3 sum = Vector3.zero;
        foreach (var pos in positions) sum += pos;
        return sum / positions.Count;
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
        SpawnChampion(token, spawnPoint, owner, teamID);
    }

    private void SpawnChampion(ArmyToken token, Transform spawnPoint, string owner, int teamID)
    {
        Vector3 champPos = spawnPoint.position + Vector3.forward * 2f;
        GameObject champPrefabToUse = token.championPrefab != null ? token.championPrefab : championPrefab;
        GameObject champSquadObj = new GameObject($"{owner}_Champion_Squad");
        champSquadObj.transform.position = champPos;
        Squad champSquad = champSquadObj.AddComponent<Squad>();

        champSquad.teamID = teamID;
        champSquad.owner = owner;
        champSquad.unitType = "Champion";
        champSquad.isChampion = true;
        champSquad.squadID = Random.Range(1000, 9999);
        champSquad.soldiers = new List<Transform>();
        champSquad.formationWidth = 1;
        champSquad.spacing = 0f;

        // ======== ASSIGN ARMY TOKEN ========
        champSquad.armyToken = token;
        Debug.Log($"[SquadSpawner] Assigned armyToken to {owner}_Champion_Squad");

        Rigidbody rb = champSquadObj.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        SphereCollider detection = champSquadObj.AddComponent<SphereCollider>();
        detection.isTrigger = true;
        detection.radius = 25f;

        SquadCombatController ctrl = champSquadObj.AddComponent<SquadCombatController>();
        ctrl.squad = champSquad;

        GameObject champObj = Instantiate(champPrefabToUse, champPos, Quaternion.identity);
        champObj.transform.SetParent(champSquadObj.transform);
        champObj.tag = teamID == 0 ? "PlayerUnit" : "EnemyUnit";

        int layerToAssign = teamID == 0 ? LayerMask.NameToLayer("PlayerSoldier") : LayerMask.NameToLayer("EnemySoldier");
        SetLayerRecursive(champSquadObj, layerToAssign);

        champSquad.soldiers.Add(champObj.transform);

        Champion champComp = champObj.GetComponent<Champion>();
        if (champComp != null)
            champComp.InitializeChampion(token.championName, token.championLevel, teamID == 0);

        var movement = champObj.GetComponent<Player3PMovement>();
        if (movement != null)
        {
            movement.isPlayerControlled = (teamID == 0);
            movement.moveSpeed = 3.5f; 
        }

        if (squadHealthBarPrefab != null)
        {
            Transform parent = worldSpaceCanvas != null ? worldSpaceCanvas.transform : null;
            GameObject hb = parent != null ? Instantiate(squadHealthBarPrefab, parent) : Instantiate(squadHealthBarPrefab);
            SquadHealthBar hbComp = hb.GetComponent<SquadHealthBar>();
            if (hbComp != null)
            {
                hbComp.squad = champSquad;
                champSquad.healthBar = hbComp;
                hb.transform.position = champSquad.GetSquadCenter() + Vector3.up * 2f;
            }
        }

        if (teamID == 0) PlayerChampion = champSquadObj;
        else EnemyChampion = champSquadObj;

        champSquad.InitializeSquad();
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
        squadObj.transform.position = position;

        string tagToAssign = (teamID == 0) ? "PlayerUnit" : "EnemyUnit";
        int layerToAssign = (teamID == 0) ? LayerMask.NameToLayer("PlayerSoldier") : LayerMask.NameToLayer("EnemySoldier");

        squadObj.tag = tagToAssign;
        squadObj.layer = layerToAssign;

        Squad squad = squadObj.AddComponent<Squad>();

        Rigidbody rb = squadObj.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        SphereCollider detection = squadObj.AddComponent<SphereCollider>();
        detection.isTrigger = true;
        detection.radius = 25f;

        SquadCombatController ctrl = squadObj.AddComponent<SquadCombatController>();
        ctrl.squad = squad;

        squad.teamID = teamID;
        squad.owner = owner;
        squad.unitType = unit.type;
        squad.squadID = Random.Range(1000, 9999);
        squad.soldiers = new List<Transform>();

        for (int i = 0; i < Mathf.Max(1, count); i++)
        {
            Vector3 offset = position + new Vector3(i * 1.5f, 0, 0);
            GameObject soldier = Instantiate(prefab, offset, Quaternion.identity);
            soldier.transform.SetParent(squadObj.transform);

            var unitComp = soldier.GetComponent<Unit>();
            if (unitComp != null) unitComp.teamID = teamID;

            soldier.tag = tagToAssign;

            UnitCombat uc = soldier.GetComponent<UnitCombat>();
            if (uc != null) uc.squadRoot = squad;

            squad.soldiers.Add(soldier.transform);
        }

        SetLayerRecursive(squadObj, layerToAssign);

        if (squadHealthBarPrefab != null)
        {
            Transform parent = worldSpaceCanvas != null ? worldSpaceCanvas.transform : null;
            GameObject hb = parent != null ? Instantiate(squadHealthBarPrefab, parent) : Instantiate(squadHealthBarPrefab);

            SquadHealthBar hbComp = hb.GetComponent<SquadHealthBar>();
            if (hbComp != null)
            {
                hbComp.squad = squad;
                squad.healthBar = hbComp;
                hb.transform.position = squad.GetSquadCenter() + Vector3.up * 2f;
            }
            else
            {
                Debug.LogError("SquadSpawner: SquadHealthBar prefab is missing SquadHealthBar component!");
            }
        }

        squad.InitializeSquad();
    }

    private void RestoreBattlefield(BattlefieldState state)
    {
        if (state.squads != null)
        {
            foreach (var ss in state.squads)
            {
                GameObject prefab = GetPrefabForType(ss.unitType);
                if (prefab == null) continue;

                GameObject squadObj = new GameObject($"{ss.owner}_Squad_{ss.squadID}_{ss.unitType}");
                Squad squad = squadObj.AddComponent<Squad>();

                squadObj.transform.position = GetCenterOfPositions(ss.soldierPositions);

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
                    if (uc != null) uc.squadRoot = squad;

                    squad.soldiers.Add(soldier.transform);
                }

                if (squadHealthBarPrefab != null)
                {
                    Transform parent = worldSpaceCanvas != null ? worldSpaceCanvas.transform : null;
                    GameObject hb = parent != null ? Instantiate(squadHealthBarPrefab, parent) : Instantiate(squadHealthBarPrefab);
                    SquadHealthBar hbComp = hb.GetComponent<SquadHealthBar>();
                    if (hbComp != null)
                    {
                        hbComp.squad = squad;
                        squad.healthBar = hbComp;
                        hb.transform.position = squad.GetSquadCenter() + Vector3.up * 2f;
                    }
                }

                squad.InitializeSquad();
            }
        }
        
        if (state.playerChampionPosition.HasValue)
            CreateChampionSquad(state.playerChampionPosition.Value, state.playerChampionRotation ?? Quaternion.identity, "Player", 0);
        if (state.enemyChampionPosition.HasValue)
            CreateChampionSquad(state.enemyChampionPosition.Value, state.enemyChampionRotation ?? Quaternion.identity, "Enemy", 1);
    }

    private void CreateChampionSquad(Vector3 pos, Quaternion rot, string owner, int teamID)
    {
        GameObject champSquadObj = new GameObject($"{owner}_Champion_Squad");
        champSquadObj.transform.position = pos;
        champSquadObj.transform.rotation = rot;

        Squad champSquad = champSquadObj.AddComponent<Squad>();
        champSquad.teamID = teamID;
        champSquad.owner = owner;
        champSquad.unitType = "Champion";
        champSquad.isChampion = true;
        champSquad.soldiers = new List<Transform>();
        champSquad.formationWidth = 1;
        champSquad.spacing = 0f;

        // ======== ASSIGN ARMY TOKEN ========
        ArmyToken token = (teamID == 0) ? BattleManager.Instance.GetPlayerToken() : BattleManager.Instance.GetEnemyToken();
        champSquad.armyToken = token;
        Debug.Log($"[SquadSpawner] Restored armyToken for {owner}_Champion_Squad");

        Rigidbody rb = champSquadObj.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        SphereCollider detection = champSquadObj.AddComponent<SphereCollider>();
        detection.isTrigger = true;
        detection.radius = 25f;

        SquadCombatController ctrl = champSquadObj.AddComponent<SquadCombatController>();
        ctrl.squad = champSquad;

        GameObject champObj = Instantiate(championPrefab, pos, rot);
        champObj.transform.SetParent(champSquadObj.transform);
        champObj.tag = teamID == 0 ? "PlayerUnit" : "EnemyUnit";

        int layerToAssign = teamID == 0 ? LayerMask.NameToLayer("PlayerSoldier") : LayerMask.NameToLayer("EnemySoldier");
        SetLayerRecursive(champSquadObj, layerToAssign);

        champSquad.soldiers.Add(champObj.transform);

        if (squadHealthBarPrefab != null)
        {
            Transform parent = worldSpaceCanvas != null ? worldSpaceCanvas.transform : null;
            GameObject hb = parent != null ? Instantiate(squadHealthBarPrefab, parent) : Instantiate(squadHealthBarPrefab);
            SquadHealthBar hbComp = hb.GetComponent<SquadHealthBar>();
            if (hbComp != null)
            {
                hbComp.squad = champSquad;
                champSquad.healthBar = hbComp;
                hb.transform.position = champSquad.GetSquadCenter() + Vector3.up * 2f;
            }
        }

        if (teamID == 0) PlayerChampion = champSquadObj;
        else EnemyChampion = champSquadObj;

        champSquad.InitializeSquad();
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

        if (PlayerChampion != null)
        {
            state.playerChampionPosition = PlayerChampion.transform.position;
            state.playerChampionRotation = PlayerChampion.transform.rotation;
        }

        if (EnemyChampion != null)
        {
            state.enemyChampionPosition = EnemyChampion.transform.position;
            state.enemyChampionRotation = EnemyChampion.transform.rotation;
        }

        return state;
    }

    void OnDestroy()
    {
        if (BattleManager.Instance != null)
            BattleManager.Instance.SaveBattlefieldState(SaveBattlefieldStateNow());
    }
}
