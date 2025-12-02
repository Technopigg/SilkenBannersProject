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
        {
            return;
        }

       
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = rtsCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 500f, soldierLayer))
            {
                Squad squad = hit.collider.GetComponentInParent<Squad>();
                if (squad != null)
                {
                    SelectOnlyThisSquad(squad);
                }
            }
            else
            {
                ClearSelection();
            }
        }

      
        if (Input.GetMouseButtonDown(1) && selectedSquads.Count > 0)
        {
            Ray ray = rtsCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000f, groundLayer))
            {
                Vector3 destination = hit.point;

                // Move EACH selected squad
                foreach (Squad s in selectedSquads)
                {
                    if (s != null)
                        s.MoveSquad(destination);
                }
            }
        }
    }


    public void SetSelectedSquads(List<Squad> squads)
    {
        ClearSelection();

        foreach (var s in squads)
        {
            if (s != null)
            {
                s.SetSelected(true);
                selectedSquads.Add(s);
            }
        }
    }


    private void SelectOnlyThisSquad(Squad squad)
    {
        ClearSelection();

        squad.SetSelected(true);
        selectedSquads.Add(squad);
    }

    public void ClearSelection()
    {
        foreach (var s in selectedSquads)
        {
            if (s != null) s.SetSelected(false);
        }

        selectedSquads.Clear();
    }
}
