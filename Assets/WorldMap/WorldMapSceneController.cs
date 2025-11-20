using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldMapSceneController : MonoBehaviour
{
    [Header("Scene References")]
    public Camera worldMapCamera;   // Assign in Inspector (recommended)

    void Start()
    {
        Debug.Log("WorldMapSceneController: WorldMap Active.");

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        EnableWorldMapCamera(true);
        DisableBattlefieldCameras();
        ReactivateWorldMapTokens();
    }

    // ----------------------------------------------------------
    // ENABLE / DISABLE CAMERAS
    // ----------------------------------------------------------
    public void EnableWorldMapCamera(bool enable)
    {
        if (worldMapCamera != null)
        {
            worldMapCamera.gameObject.SetActive(enable);
            return;
        }

        // Fallback: find any camera named "WorldMapCamera"
        Camera[] cams = FindObjectsOfType<Camera>(true);
        foreach (var cam in cams)
        {
            if (cam.name.Contains("WorldMap"))
            {
                cam.gameObject.SetActive(enable);
                return;
            }
        }

        Debug.LogWarning("WorldMapSceneController: No world map camera found!");
    }

    private void DisableBattlefieldCameras()
    {
        Camera[] cams = FindObjectsOfType<Camera>(true);
        foreach (var cam in cams)
        {
            if (cam.name.Contains("Battle") || cam.name.Contains("Player3P"))
            {
                cam.gameObject.SetActive(false);
            }
        }
    }

    // ----------------------------------------------------------
    // RE-ENABLE TOKEN MOVEMENT ON MAP
    // ----------------------------------------------------------
    private void ReactivateWorldMapTokens()
    {
        ArmyToken[] tokens = FindObjectsOfType<ArmyToken>(true);

        foreach (var token in tokens)
        {
            // Allow movement only if not locked in battle
            if (!token.isLockedInBattle)
                token.canMoveOnWorldMap = true;
        }
    }
}
