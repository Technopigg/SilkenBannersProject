using UnityEngine;

public class RTSSelectionManager : MonoBehaviour
{
    [Header("References")]
    public Camera rtsCamera;
    public LayerMask soldierLayer;
    public LayerMask groundLayer;

    private Squad selectedSquad;

   public void Update()
    {
        if (ModeController.Instance == null ||
            ModeController.Instance.currentMode != ControlMode.RTS)
        {
            return; // only active in RTS mode
        }

        // === Left-click: select squad ===
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = rtsCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 500f, soldierLayer))
            {
                Squad squad = hit.collider.GetComponentInParent<Squad>();
                if (squad != null)
                {
                    SelectSquad(squad);
                }
            }
            else
            {
                DeselectSquad();
            }
        }

        // === Right-click: issue move order ===
        if (Input.GetMouseButtonDown(1) && selectedSquad != null)
        {
            Ray ray = rtsCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000f, groundLayer))
            {
                Vector3 destination = hit.point;
                selectedSquad.MoveSquad(destination);
            }
        }
    }

   public void SelectSquad(Squad squad)
    {
        if (selectedSquad != null)
        {
            selectedSquad.SetSelected(false);
        }

        selectedSquad = squad;
        selectedSquad.SetSelected(true);
        Debug.Log("Selected squad: " + squad.squadID);
    }

    public void DeselectSquad()
    {
        if (selectedSquad != null)
        {
            selectedSquad.SetSelected(false);
            selectedSquad = null;
        }
    }
}