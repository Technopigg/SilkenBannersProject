using UnityEngine;
using DG.Tweening;

public class ArmyUIBannerAnimator : MonoBehaviour
{
    [Header("Assign the Banner RectTransform")]
    public RectTransform banner;

    [Header("Optional: Another banner animator to animate together")]
    public ArmyUIBannerAnimator linkedBanner;   // ← Add this

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

        // ALSO animate linked banner
        if (linkedBanner != null)
        {
            linkedBanner.banner.DOAnchorPosY(linkedBanner.openPosY, linkedBanner.duration)
                .SetEase(linkedBanner.ease);

            linkedBanner.isOpen = true;
        }

        isOpen = true;
    }

    public void CloseBanner()
    {
        banner.DOAnchorPosY(closedPosY, duration)
            .SetEase(ease);

        // ALSO animate linked banner
        if (linkedBanner != null)
        {
            linkedBanner.banner.DOAnchorPosY(linkedBanner.closedPosY, linkedBanner.duration)
                .SetEase(linkedBanner.ease);

            linkedBanner.isOpen = false;
        }

        isOpen = false;
    }
}