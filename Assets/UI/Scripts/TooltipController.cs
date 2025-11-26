using UnityEngine;
using TMPro;

public class TooltipController : MonoBehaviour
{
    public static TooltipController Instance;

    [SerializeField] private RectTransform tooltip;
    [SerializeField] private TextMeshProUGUI tooltipText;

    private void Awake()
    {
        Instance = this;
        Hide();
    }

    public void Show(string text, Vector3 position)
    {
        tooltip.gameObject.SetActive(true);
        tooltipText.text = text;

        tooltip.position = position;
    }

    public void Hide()
    {
        tooltip.gameObject.SetActive(false);
    }
}
