using UnityEngine;
using DG.Tweening;

public class ArmyUIBannerAnimator : MonoBehaviour
{
    [Header("Assign the Banner RectTransform")]
    public RectTransform banner;

    [Header("Optional: Other banners to animate together")]
    public ArmyUIBannerAnimator[] linkedBanners;   // ← now an array

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
        banner.DOAnchorPosY(openPosY, duration).SetEase(ease);

        foreach (var b in linkedBanners)
        {
            if (b == null) continue;
            b.banner.DOAnchorPosY(b.openPosY, b.duration).SetEase(b.ease);
            b.isOpen = true;
        }

        isOpen = true;
    }

    public void CloseBanner()
    {
        banner.DOAnchorPosY(closedPosY, duration).SetEase(ease);

        foreach (var b in linkedBanners)
        {
            if (b == null) continue;
            b.banner.DOAnchorPosY(b.closedPosY, b.duration).SetEase(b.ease);
            b.isOpen = false;
        }

        isOpen = false;
    }
}