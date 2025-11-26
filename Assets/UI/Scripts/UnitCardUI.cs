using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UnitCardUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI countText;

    public Sprite spearmanIcon;
    public Sprite archerIcon;
    public Sprite cavalryIcon;
    public Sprite generalIcon;

    private UnitCardTooltip tooltip;    

    private void Awake()
    {
        tooltip = GetComponent<UnitCardTooltip>();
    }

    public void Set(string type, int count)
    {
        countText.text = count.ToString();
        string tooltipMessage;

        switch (type.ToLower())
        {
            case "spearman":
            case "spearmen":
                icon.sprite = spearmanIcon;
                tooltipMessage = $"Spearmen\nCount: {count}\nStrong against cavalry.";
                break;

            case "archer":
            case "archers":
                icon.sprite = archerIcon;
                tooltipMessage = $"Archers\nCount: {count}\nRanged attackers. Weak in melee.";
                break;

            case "cavalry":
                icon.sprite = cavalryIcon;
                tooltipMessage = $"Cavalry\nCount: {count}\nFast and powerful charge attacks.";
                break;

            case "general":
                icon.sprite = generalIcon;
                tooltipMessage = $"General\nCount: {count}\nProvides leadership and morale bonus.";
                break;

            default:
                icon.sprite = null;
                tooltipMessage = $"Unknown Unit\nCount: {count}";
                Debug.LogWarning("Unknown unit type: " + type);
                break;
        }

       
        if (tooltip != null)
            tooltip.SetTooltip(tooltipMessage);
        else
            Debug.LogWarning("UnitCardTooltip component missing on UnitCard prefab.");
    }
}