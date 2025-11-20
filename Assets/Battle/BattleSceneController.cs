using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BattleSceneController : MonoBehaviour
{
    private BattleEngagement currentBattle;
    private SquadSpawner spawner;

    void Start()
    {
        StartCoroutine(InitRoutine());
    }

    private IEnumerator InitRoutine()
    {
        // Wait two frames to allow SquadSpawner / PlayerGeneral to initialize
        yield return null;
        yield return null;

        currentBattle = BattleManager.Instance != null
            ? BattleManager.Instance.GetCurrentEngagement()
            : null;

        Debug.Log("BattleSceneController: Fetched engagement = " + currentBattle);

        spawner = FindObjectOfType<SquadSpawner>();
        if (spawner == null)
        {
            Debug.LogWarning("BattleSceneController: No SquadSpawner found in scene after wait!");
        }
        else
        {
            StartCoroutine(AssignCameraWithDelay());
        }
    }

    private IEnumerator AssignCameraWithDelay()
    {
        Debug.Log("BattleSceneController: Waiting before assigning camera...");
        yield return new WaitForSeconds(0.05f);

        if (spawner != null && spawner.PlayerGeneral != null)
        {
            Player3PCamera cam = FindObjectOfType<Player3PCamera>();
            if (cam != null)
            {
                cam.SetTarget(spawner.PlayerGeneral.transform);
                Debug.Log("BattleSceneController: Camera target set to Player General.");
            }
            else
            {
                Debug.LogWarning("BattleSceneController: No Player3PCamera found!");
            }
        }
        else
        {
            Debug.LogWarning("BattleSceneController: Player General not found!");
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // TEMPORARY EXIT — Handled ENTIRELY by BattleManager now
    // ─────────────────────────────────────────────────────────────────────
    public void ExitToWorldMapTemporary()
    {
        if (BattleManager.Instance != null)
            BattleManager.Instance.TemporaryExitToWorldMap();
        else
            Debug.LogWarning("BattleSceneController: BattleManager missing!");
    }

    // ─────────────────────────────────────────────────────────────────────
    // FINAL EXIT — Ends battle + unloads scene
    // ─────────────────────────────────────────────────────────────────────
    public void ExitToWorldMapFinal()
    {
        var engagement = BattleManager.Instance != null
            ? BattleManager.Instance.GetCurrentEngagement()
            : null;

        if (engagement == null)
        {
            Debug.LogWarning("BattleSceneController: No active battle to exit!");
            return;
        }

        Debug.Log("BattleSceneController: FINAL exit → end battle & unload scene.");
        BattleManager.Instance.EndBattle();

        // Switch to WorldMap
        Scene world = SceneManager.GetSceneByName("WorldMap");
        if (world.IsValid() && world.isLoaded)
        {
            SceneManager.SetActiveScene(world);
            Debug.Log("BattleSceneController: WorldMap active.");
        }
        else
        {
            Debug.LogWarning("BattleSceneController: WorldMap scene not found!");
        }

        // Unload battlefield
        Scene battlefield = SceneManager.GetSceneByName("BattlefieldScene");
        if (battlefield.IsValid() && battlefield.isLoaded)
        {
            if (spawner != null)
            {
                spawner.SaveBattlefieldStateNow();
            }

            SceneManager.UnloadSceneAsync(battlefield);
            Debug.Log("BattleSceneController: Unloaded BattlefieldScene.");
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
