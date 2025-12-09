using UnityEngine;
using System.Collections.Generic;

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
        if (!Input.GetMouseButtonDown(0)) return;

        Ray ray = rtsCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 500f, soldierLayer))
        {
            Squad squad = hit.collider.GetComponentInParent<Squad>();

            // ★ BLOCK ENEMY SELECTION ★
            if (squad != null && squad.teamID == 0)
            {
                SelectOnlyThisSquad(squad);
            }
        }
        else
        {
            ClearSelection();
        }
    }

    void HandleRightClickMovement()
    {
        if (!Input.GetMouseButtonDown(1) || selectedSquads.Count == 0)
            return;

        Ray ray = rtsCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, 1000f, groundLayer))
            return;

        Vector3 destination = hit.point;

        float spacing = 8f;
        int squads = selectedSquads.Count;

        for (int i = 0; i < squads; i++)
        {
            Squad s = selectedSquads[i];
            if (s == null) continue;

            int row = i / 2;
            int col = i % 2;

            Vector3 offset = new Vector3(col * spacing, 0f, row * spacing);
            s.MoveSquad(destination + offset);
        }
    }

    public void SetSelectedSquads(List<Squad> squads)
    {
        ClearSelection();

        foreach (var s in squads)
        {
            if (s != null && s.teamID == 0)
            {
                s.SetSelected(true);
                selectedSquads.Add(s);

                if (s.healthBar != null)
                    s.healthBar.gameObject.SetActive(true);
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
    }
}
