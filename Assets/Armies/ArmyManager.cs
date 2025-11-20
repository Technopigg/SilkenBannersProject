using UnityEngine;

public class ArmyManager : MonoBehaviour
{
    private ArmyToken selectedToken;

    [Header("UI Reference")]
    public ArmyUI armyUI; // assign in Inspector

    void Update()
    {
        // --- Left-click: select OR deselect ---
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                ArmyToken token = hit.collider.GetComponentInParent<ArmyToken>();
                if (token != null)
                {
                    // Deselect previous
                    if (selectedToken != null)
                    {
                        selectedToken.SetSelected(false);
                        armyUI.Clear();
                    }

                    // Select new
                    selectedToken = token;
                    selectedToken.SetSelected(true);
                    armyUI.ShowArmy(selectedToken);
                }
                else
                {
                    // Clicked something that's NOT an ArmyToken → deselect
                    if (selectedToken != null)
                    {
                        selectedToken.SetSelected(false);
                        selectedToken = null;
                        armyUI.Clear();
                    }
                }
            }
            else
            {
                // Raycast hit nothing → deselect
                if (selectedToken != null)
                {
                    selectedToken.SetSelected(false);
                    selectedToken = null;
                    armyUI.Clear();
                }
            }
        }

        // --- Right-click: move selected token ---
        if (Input.GetMouseButtonDown(1) && selectedToken != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                selectedToken.SetTarget(hit.point);
            }
        }
    }
}