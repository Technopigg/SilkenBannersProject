using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuffIconUI : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TextMeshProUGUI stackText;
    public Image timerOverlay;

    private string id;
    
    public void Setup(string buffID, Sprite sprite, int stacks, float duration)
    {
        this.id = buffID;

        iconImage.sprite = sprite;

        stackText.text = stacks > 1 ? stacks.ToString() : "";

        // Full timer bar at start
        timerOverlay.fillAmount = 1f;
    }
    
    public void UpdateTimer(float remaining, float duration)
    {
        timerOverlay.fillAmount = Mathf.Clamp01(remaining / duration);
    }
    
    public void UpdateBuff(int stacks, float newDuration)
    {
        stackText.text = stacks > 1 ? stacks.ToString() : "";
    }
}