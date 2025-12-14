using UnityEngine;

public static class PortraitHelper
{
    public static void ShowForChampion(GameObject championPrefab)
    {
        if (championPrefab == null)
        {
            Debug.LogWarning("[PortraitHelper] ShowForChampion called with null prefab.");
            return;
        }

        if (GeneralPortraitUI.Instance != null)
        {
            Debug.Log($"[PortraitHelper] Calling GeneralPortraitUI.Show for {championPrefab.name}");
            GeneralPortraitUI.Instance.Show(championPrefab);
        }
        else
        {
            Debug.LogWarning("[PortraitHelper] GeneralPortraitUI.Instance is null!");
        }
    }

    public static void HidePortrait()
    {
        if (GeneralPortraitUI.Instance != null)
        {
            Debug.Log("[PortraitHelper] Calling GeneralPortraitUI.Hide");
            GeneralPortraitUI.Instance.Hide();
        }
    }
}