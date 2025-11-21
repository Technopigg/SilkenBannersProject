using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UnitCardUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI countText;

    // A mapping from text type -> sprite
    public Sprite spearmanIcon;
    public Sprite archerIcon;
    public Sprite cavalryIcon;
    public Sprite generalIcon;

    public void Set(string type, int count)
    {
        countText.text = count.ToString();

        // Choose icon based on string
        switch (type.ToLower())
        {
            case "spearman":
            case "spearmen":
                icon.sprite = spearmanIcon;
                break;

            case "archer":
            case "archers":
                icon.sprite = archerIcon;
                break;

            case "cavalry":
                icon.sprite = cavalryIcon;
                break;

            case "general":
                icon.sprite = generalIcon;
                break;

            default:
                icon.sprite = null;
                Debug.LogWarning("Unknown unit type: " + type);
                break;
        }
    }
}