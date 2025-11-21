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
                    // Deselect previous (but DO NOT clear UI here)
                    if (selectedToken != null)
                        selectedToken.SetSelected(false);

                    // Select new
                    selectedToken = token;
                    selectedToken.SetSelected(true);

                    armyUI.ShowArmy(selectedToken); // load new army
                }
                else
                {
                    // Clicked somewhere that is NOT a token → full deselect
                    if (selectedToken != null)
                    {
                        selectedToken.SetSelected(false);
                        selectedToken = null;

                        armyUI.Clear();  // HERE is where clear stays
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

                    armyUI.Clear(); // same as above
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
