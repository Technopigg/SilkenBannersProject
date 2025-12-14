using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RTSSelectionManager : MonoBehaviour
{
    [Header("References")]
    public Camera rtsCamera;
    public LayerMask soldierLayer;
    public LayerMask groundLayer;

    private List<Squad> selectedSquads = new List<Squad>();

    void Update()
    {
        if (ModeController.Instance == null ||
            ModeController.Instance.currentMode != ControlMode.RTS)
            return;

        HandleLeftClickSelection();
        HandleRightClickMovement();
    }

    void HandleLeftClickSelection()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (!Input.GetMouseButtonDown(0)) return;

        Ray ray = rtsCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 500f, soldierLayer))
        {
            Squad squad = hit.collider.GetComponentInParent<Squad>();
            if (squad != null && squad.teamID == 0)
            {
                Debug.Log($"[RTSSelection] Left-click selected squad: {squad.name}, Champion? {squad.isChampion}");
                SelectOnlyThisSquad(squad);

                if (squad.armyToken != null && squad.armyToken.championPrefab != null)
                {
                    Debug.Log($"[RTSSelection] Showing portrait for champion prefab: {squad.armyToken.championPrefab.name}");
                    PortraitHelper.ShowForChampion(squad.armyToken.championPrefab);
                }
                else
                {
                    Debug.Log("[RTSSelection] Squad has no championPrefab assigned.");
                }
            }
        }
        else
        {
            ClearSelection();
        }
    }

    void HandleRightClickMovement()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (!Input.GetMouseButtonDown(1) || selectedSquads.Count == 0) return;

        Ray ray = rtsCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer)) return;

        Vector3 destination = hit.point;
        float spacing = 8f;

        for (int i = 0; i < selectedSquads.Count; i++)
        {
            Squad s = selectedSquads[i];
            if (s == null) continue;

            if (s.isChampion)
            {
                s.MoveSquad(destination);
            }
            else
            {
                int row = i / 2;
                int col = i % 2;
                Vector3 offset = new Vector3(col * spacing, 0f, row * spacing);
                s.MoveSquad(destination + offset);
            }
        }
    }

    public void SetSelectedSquads(List<Squad> squads)
    {
        ClearSelection();

        foreach (var s in squads)
        {
            if (s == null || s.teamID != 0)
                continue;

            if (s.isChampion)
            {
                Debug.Log($"[RTSSelection] Multi-select found champion: {s.name}");
                SelectOnlyThisSquad(s);
                return;
            }

            s.SetSelected(true);
            selectedSquads.Add(s);

            if (s.healthBar != null)
                s.healthBar.gameObject.SetActive(true);

            if (s.armyToken != null && s.armyToken.championPrefab != null)
            {
                Debug.Log($"[RTSSelection] Multi-select showing portrait for: {s.armyToken.championPrefab.name}");
                PortraitHelper.ShowForChampion(s.armyToken.championPrefab);
            }
        }
    }

    private void SelectOnlyThisSquad(Squad squad)
    {
        ClearSelection();
        squad.SetSelected(true);
        selectedSquads.Add(squad);

        if (squad.healthBar != null)
            squad.healthBar.gameObject.SetActive(true);

        if (squad.armyToken != null && squad.armyToken.championPrefab != null)
        {
            Debug.Log($"[RTSSelection] Single select showing portrait for: {squad.armyToken.championPrefab.name}");
            PortraitHelper.ShowForChampion(squad.armyToken.championPrefab);
        }
    }

    public void ClearSelection()
    {
        foreach (var s in selectedSquads)
        {
            if (s != null)
            {
                s.SetSelected(false);
                if (s.healthBar != null)
                    s.healthBar.gameObject.SetActive(false);
            }
        }

        selectedSquads.Clear();
        Debug.Log("[RTSSelection] Clearing portrait");
        PortraitHelper.HidePortrait();
    }
}
