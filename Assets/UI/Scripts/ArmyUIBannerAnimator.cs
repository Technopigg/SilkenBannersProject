using UnityEngine;
using DG.Tweening;

public class ArmyUIBannerAnimator : MonoBehaviour
{
    public RectTransform banner;          
    public float hiddenY = -150f;         
    public float shownY = 0f;             
    public float slideDuration = 0.5f;    

    private Tween currentTween;

    private void Start()
    {
        
        Vector2 pos = banner.anchoredPosition;
        pos.y = hiddenY;
        banner.anchoredPosition = pos;
    }

    public void ShowBanner()
    {
        if (currentTween != null) currentTween.Kill();

        currentTween = banner.DOAnchorPosY(shownY, slideDuration)
            .SetEase(Ease.OutCubic);
    }

    public void HideBanner()
    {
        if (currentTween != null) currentTween.Kill();

        currentTween = banner.DOAnchorPosY(hiddenY, slideDuration)
            .SetEase(Ease.InCubic);
    }
}
