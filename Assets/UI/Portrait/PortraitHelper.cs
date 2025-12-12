using UnityEngine;

/// <summary>
/// Helper to call GeneralPortraitUI easily from selection logic.
/// </summary>
public static class PortraitHelper
{
    public static void ShowForChampion(GameObject championPrefab)
    {
        if (GeneralPortraitUI.Instance != null)
            GeneralPortraitUI.Instance.Show(championPrefab);
    }

    public static void HidePortrait()
    {
        if (GeneralPortraitUI.Instance != null)
            GeneralPortraitUI.Instance.Hide();
    }
}