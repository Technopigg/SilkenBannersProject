using System.Collections;
using UnityEngine;

/// <summary>
/// Controls show/hide lifecycle of the battlefield scene.
/// Contains updated camera system compatibility.
/// </summary>
public class BattleRootController : MonoBehaviour
{
    [Header("References (optional - auto-find if left empty)")]
    public SquadSpawner squadSpawner;
    public GameObject[] uiRoots;
    public Camera[] sceneCameras;

    void Awake()
    {
        if (squadSpawner == null)
            squadSpawner = GetComponentInChildren<SquadSpawner>();

        if (sceneCameras == null || sceneCameras.Length == 0)
            sceneCameras = GetComponentsInChildren<Camera>(true);
    }

    // -------------------------------------------------------------------------
    // SHOW — Called when entering or re-entering battlefield
    // -------------------------------------------------------------------------
    public void ShowBattlefield()
    {
        gameObject.SetActive(true);

        if (sceneCameras != null)
            foreach (var cam in sceneCameras)
                if (cam != null) cam.enabled = true;

        if (uiRoots != null)
            foreach (var u in uiRoots)
                if (u != null) u.SetActive(true);

        // Reapply the camera mode via our new clean API
        if (ModeController.Instance != null)
        {
            ModeController.Instance.ApplyCameraState(
                ModeController.Instance.currentMode,
                align: true
            );

            Debug.Log("BattleRootController: Camera state reapplied through ModeController.");
        }

        StartCoroutine(AllowSpawnerToInitialize());
        Debug.Log("BattleRootController: ShowBattlefield called.");
    }

    IEnumerator AllowSpawnerToInitialize()
    {
        yield return null;
        if (squadSpawner != null)
            Debug.Log("BattleRootController: SquadSpawner present on show.");
    }

    // -------------------------------------------------------------------------
    // HIDE — Temporary exit from battle
    // -------------------------------------------------------------------------
    public void HideBattlefield()
    {
        if (squadSpawner != null)
        {
            squadSpawner.SaveBattlefieldStateNow();
            Debug.Log("BattleRootController: Saved battlefield state before hiding.");
        }

        if (sceneCameras != null)
            foreach (var cam in sceneCameras)
                if (cam != null) cam.enabled = false;

        if (uiRoots != null)
            foreach (var u in uiRoots)
                if (u != null) u.SetActive(false);

        gameObject.SetActive(false);
        Debug.Log("BattleRootController: HideBattlefield called — root disabled.");
    }

    public void ToggleBattlefield(bool show)
    {
        if (show) ShowBattlefield();
        else HideBattlefield();
    }
}
