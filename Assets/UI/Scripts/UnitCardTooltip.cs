using UnityEngine;
using UnityEngine.EventSystems;

public class UnitCardTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private string dynamicMessage;    
    [TextArea] public string fallbackMessage;  

    /// <summary>
    /// Called by UnitCardUI when creating the card.
    /// </summary>
    public void SetTooltip(string msg)
    {
        dynamicMessage = msg;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        string msgToUse = string.IsNullOrEmpty(dynamicMessage) ? fallbackMessage : dynamicMessage;

        TooltipController.Instance.Show(
            msgToUse,
            Input.mousePosition
        );
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipController.Instance.Hide();
    }
}