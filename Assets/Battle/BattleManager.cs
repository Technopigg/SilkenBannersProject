using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    private List<ArmyUnit> playerArmy;
    private List<ArmyUnit> enemyArmy;

    private BattleEngagement currentEngagement;
    private BattlefieldState lastBattlefieldState;

    private bool battlefieldLoading = false;
    
    private BattleRootController battleRoot;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // NEW: Create an engagement from two ArmyTokens and begin the battle.
    // ─────────────────────────────────────────────────────────────────────
    public BattleEngagement CreateEngagementAndStart(ArmyToken a, ArmyToken b)
    {
        if (a == null || b == null)
        {
            Debug.LogError("CreateEngagementAndStart: one or both ArmyTokens are null.");
            return null;
        }

        if (a == b)
        {
            Debug.LogWarning("CreateEngagementAndStart: same token passed for both sides.");
            return null;
        }
        
        if (currentEngagement != null)
        {
            if ((currentEngagement.player == a && currentEngagement.enemy == b) ||
                (currentEngagement.player == b && currentEngagement.enemy == a))
            {
                Debug.Log("CreateEngagementAndStart: engagement already active for these tokens — reusing.");
                BeginBattle(currentEngagement);
                return currentEngagement;
            }
        }
        
        BattleEngagement engagement = new BattleEngagement(a, b);
        Debug.Log($"New engagement created between {a.name} and {b.name}.");

        BeginBattle(engagement);
        return engagement;
    }

    // ─────────────────────────────────────────────────────────────────────
    // MAIN ENTRY: Begin Battle
    // ─────────────────────────────────────────────────────────────────────
    public void BeginBattle(BattleEngagement engagement)
    {
        if (engagement == null)
        {
            Debug.LogError("BeginBattle called with null engagement!");
            return;
        }

        // If same engagement → reactivate battlefield
        if (currentEngagement == engagement)
        {
            StartCoroutine(ActivateBattleSceneRoutine());
            return;
        }

        currentEngagement = engagement;

        if (engagement.player != null)
        {
            playerArmy = engagement.player.composition;
            engagement.player.LockInBattle();
        }

        if (engagement.enemy != null)
        {
            enemyArmy = engagement.enemy.composition;
            engagement.enemy.LockInBattle();
        }

        StartCoroutine(BeginBattleRoutine());
    }

    private IEnumerator BeginBattleRoutine()
    {
        while (battlefieldLoading) yield return null;

        if (IsSceneLoaded("BattlefieldScene"))
        {
            yield return StartCoroutine(ActivateBattleSceneRoutine());
            yield break;
        }

        battlefieldLoading = true;

        AsyncOperation op = SceneManager.LoadSceneAsync("BattlefieldScene", LoadSceneMode.Additive);
        while (!op.isDone)
            yield return null;

        // ✔ MAKE BATTLEFIELD SCENE ACTIVE BEFORE ANYTHING ELSE
        Scene battlefieldScene = SceneManager.GetSceneByName("BattlefieldScene");
        SceneManager.SetActiveScene(battlefieldScene);
        Debug.Log("BattlefieldScene is now ACTIVE scene.");

        battlefieldLoading = false;

        // ✔ Now activate battlefield AND then activate SquadSpawner
        yield return StartCoroutine(ActivateBattleSceneRoutine());
    }

    // ─────────────────────────────────────────────────────────────────────
    // ACTIVATE BATTLEFIELD SCENE
    // ─────────────────────────────────────────────────────────────────────
    private IEnumerator ActivateBattleSceneRoutine()
    {
        Scene battle = SceneManager.GetSceneByName("BattlefieldScene");

        if (!battle.IsValid() || !battle.isLoaded)
            yield break;

        SceneManager.SetActiveScene(battle);

        if (battleRoot == null)
        {
            foreach (GameObject rootObj in battle.GetRootGameObjects())
            {
                battleRoot = rootObj.GetComponentInChildren<BattleRootController>(true);
                if (battleRoot != null) break;
            }
        }

        if (battleRoot == null)
        {
            Debug.LogError("BattleRootController NOT FOUND in BattlefieldScene!");
            yield break;
        }

        HideWorldMapScene();
        battleRoot.ShowBattlefield();

        // ────────────────────────────────────────────────────────────────
        // 🔥 NEW: ENABLE SQUADSPAWNER NOW (OPTION 1 FIX)
        // SquadSpawner object must be disabled in scene beforehand!
        // ────────────────────────────────────────────────────────────────
        EnableSquadSpawner();
        // ────────────────────────────────────────────────────────────────

        yield return null;
    }

    // ─────────────────────────────────────────────────────────────────────
    // NEW FUNCTION: MANUALLY ACTIVATE DISABLED SQUADSPAWNER
    // ─────────────────────────────────────────────────────────────────────
    private void EnableSquadSpawner()
    {
        if (battleRoot == null)
        {
            Debug.LogError("EnableSquadSpawner: battleRoot is NULL.");
            return;
        }

        if (battleRoot.squadSpawner == null)
        {
            Debug.LogError("EnableSquadSpawner: battleRoot.squadSpawner is NULL.");
            return;
        }

        Debug.Log("Enabling SquadSpawner...");

        battleRoot.squadSpawner.gameObject.SetActive(true);

        Debug.Log("SquadSpawner ENABLED and will now run Start().");
    }


    // ─────────────────────────────────────────────────────────────────────
    // TEMPORARY EXIT TO WORLD MAP
    // ─────────────────────────────────────────────────────────────────────
    public void TemporaryExitToWorldMap()
    {
        if (battleRoot == null)
        {
            Debug.LogWarning("TemporaryExitToWorldMap: No battleRoot!");
            return;
        }

        if (battleRoot.squadSpawner != null)
        {
            battleRoot.squadSpawner.SaveBattlefieldStateNow();
            lastBattlefieldState = GetLastBattlefieldState();
        }

        battleRoot.HideBattlefield();
        ShowWorldMapScene();

        Scene world = SceneManager.GetSceneByName("WorldMap");
        if (world.IsValid())
            SceneManager.SetActiveScene(world);
    }

    // ─────────────────────────────────────────────────────────────────────
    // FINAL EXIT — BATTLE ENDED
    // ─────────────────────────────────────────────────────────────────────
    public void EndBattle()
    {
        if (currentEngagement != null)
        {
            currentEngagement.player?.UnlockFromBattle();
            currentEngagement.enemy?.UnlockFromBattle();
        }

        ShowWorldMapScene();
        ClearBattlefieldState();
        currentEngagement = null;
    }

    // ─────────────────────────────────────────────────────────────────────
    // GETTERS
    // ─────────────────────────────────────────────────────────────────────
    public BattleEngagement GetCurrentEngagement() => currentEngagement;
    public ArmyToken GetPlayerToken() => currentEngagement?.player;
    public ArmyToken GetEnemyToken() => currentEngagement?.enemy;
    public List<ArmyUnit> GetPlayerArmy() => playerArmy;
    public List<ArmyUnit> GetEnemyArmy() => enemyArmy;

    // ─────────────────────────────────────────────────────────────────────
    // STATE STORAGE
    // ─────────────────────────────────────────────────────────────────────
    public void SaveBattlefieldState(BattlefieldState state)
    {
        lastBattlefieldState = state;
    }

    public BattlefieldState GetLastBattlefieldState() => lastBattlefieldState;

    public void ClearBattlefieldState()
    {
        lastBattlefieldState = null;
    }

    // ─────────────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────────────
    private bool IsSceneLoaded(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).name == sceneName)
                return true;

        return false;
    }

    private void HideWorldMapScene()
    {
        Scene world = SceneManager.GetSceneByName("WorldMap");
        if (!world.IsValid()) return;

        foreach (var obj in world.GetRootGameObjects())
            obj.SetActive(false);

        Debug.Log("WorldMap hidden while in battlefield.");
    }
    
    private void ShowWorldMapScene()
    {
        Scene world = SceneManager.GetSceneByName("WorldMap");
        if (!world.IsValid()) return;

        foreach (var obj in world.GetRootGameObjects())
            obj.SetActive(true);

        Debug.Log("WorldMap reactivated.");
    }
}
