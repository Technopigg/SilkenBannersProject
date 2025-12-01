using UnityEngine;
using UnityEngine.EventSystems; // <<< IMPORTANT

public class ArmyManager : MonoBehaviour
{
    private ArmyToken selectedToken;

    [Header("UI Reference")]
    public ArmyUI armyUI; 

    void Update()
    {

        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                ArmyToken token = hit.collider.GetComponentInParent<ArmyToken>();

                if (token != null)
                {
                    if (selectedToken != null)
                        selectedToken.SetSelected(false);
                    selectedToken = token;
                    selectedToken.SetSelected(true);
                    armyUI.ShowArmy(selectedToken); 
                }
                else
                {
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
                if (selectedToken != null)
                {
                    selectedToken.SetSelected(false);
                    selectedToken = null;
                    armyUI.Clear(); 
                }
            }
        }


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