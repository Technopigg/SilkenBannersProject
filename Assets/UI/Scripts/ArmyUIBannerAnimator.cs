using UnityEngine;
using DG.Tweening;

public class ArmyUIBannerAnimator : MonoBehaviour
{
    [Header("Assign the Banner RectTransform")]
    public RectTransform banner;

    [Header("Positions")]
    public float openPosY = 228f;      
    public float closedPosY = -142f;   

    [Header("Animation")]
    public float duration = 0.35f;
    public Ease ease = Ease.OutQuart;

    private bool isOpen = false;

    public void ToggleBanner()
    {
        if (isOpen)
            CloseBanner();
        else
            OpenBanner();
    }

    public void OpenBanner()
    {
        banner.DOAnchorPosY(openPosY, duration)
            .SetEase(ease);

        isOpen = true;
    }

    public void CloseBanner()
    {
        banner.DOAnchorPosY(closedPosY, duration)
            .SetEase(ease);

        isOpen = false;
    }
}