using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to the BattleRoot GameObject in the BattlefieldScene.
/// Provides simple show/hide and lifecycle hooks (save/restore) for the whole battle.
/// </summary>
public class BattleRootController : MonoBehaviour
{
    [Header("References (optional - auto-find if left empty)")]
    public SquadSpawner squadSpawner;
    public GameObject[] uiRoots;         // any UI GameObjects under BattleRoot you want to toggle separately
    public Camera[] sceneCameras;        // battlefield cameras (player/rts) if you want explicit control

    void Awake()
    {
        // auto-bind if not assigned
        if (squadSpawner == null)
        {
            squadSpawner = GetComponentInChildren<SquadSpawner>();
        }

        if (sceneCameras == null || sceneCameras.Length == 0)
        {
            sceneCameras = GetComponentsInChildren<Camera>(true);
        }
    }

    /// <summary>
    /// Show the whole battle (enable BattleRoot and re-enable cameras & UI).
    /// Called when entering/re-entering the battle.
    /// </summary>
    public void ShowBattlefield()
    {
        gameObject.SetActive(true);

        // enable cameras explicitly
        if (sceneCameras != null)
        {
            for (int i = 0; i < sceneCameras.Length; i++)
            {
                if (sceneCameras[i] != null)
                    sceneCameras[i].enabled = true;
            }
        }

        // enable UI roots
        if (uiRoots != null)
        {
            foreach (var u in uiRoots)
                if (u != null) u.SetActive(true);
        }

        // ⭐ Reapply current camera mode when returning to the battlefield
        if (ModeController.Instance != null)
        {
            ModeController.Instance.ApplyCameraState(
                ModeController.Instance.currentMode,
                align: true
            );

            Debug.Log("BattleRootController: Camera state reapplied through ModeController.");
        }

        // Allow squad spawner a frame to re-link PlayerGeneral if needed
        StartCoroutine(AllowSpawnerToInitialize());

        Debug.Log("BattleRootController: ShowBattlefield called.");
    }


    IEnumerator AllowSpawnerToInitialize()
    {
        // small delay to let scene Start() and Awake() run
        yield return null;

        if (squadSpawner != null)
        {
            Debug.Log("BattleRootController: SquadSpawner present on show.");
        }
    }

    /// <summary>
    /// Hide the whole battle without unloading (called on temporary exit).
    /// This saves state first (via SquadSpawner) then disables this root.
    /// </summary>
    public void HideBattlefield()
    {
        // Save state explicitly before hiding
        if (squadSpawner != null)
        {
            squadSpawner.SaveBattlefieldStateNow();
            Debug.Log("BattleRootController: Saved battlefield state via SquadSpawner before hiding.");
        }

        // disable cameras
        if (sceneCameras != null)
        {
            for (int i = 0; i < sceneCameras.Length; i++)
            {
                if (sceneCameras[i] != null) sceneCameras[i].enabled = false;
            }
        }

        // hide UI
        if (uiRoots != null)
        {
            foreach (var u in uiRoots) 
                if (u != null) u.SetActive(false);
        }

        // disable root
        gameObject.SetActive(false);
        Debug.Log("BattleRootController: HideBattlefield called, BattleRoot disabled.");
    }

    /// <summary>
    /// Convenience: toggles the BattleRoot visibility
    /// </summary>
    public void ToggleBattlefield(bool show)
    {
        if (show) ShowBattlefield();
        else HideBattlefield();
    }
}
