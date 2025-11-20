using UnityEngine;
using TMPro; // TextMeshPro
using System.Text;

public class ArmyUI : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI armyInfoText; // drag your ArmyInfoText TMP object here in Inspector

    // Show selected army’s composition
    public void ShowArmy(ArmyToken token)
    {
        if (token == null)
        {
            Clear();
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Owner: {token.owner}");

        if (token.composition.Count == 0)
        {
            sb.AppendLine("No units assigned.");
        }
        else
        {
            foreach (var unit in token.composition)
            {
                sb.AppendLine($"{unit.type} x{unit.count}");
            }
        }

        armyInfoText.text = sb.ToString();
    }

    // Clear panel when deselected
    public void Clear()
    {
        armyInfoText.text = "";
    }
}