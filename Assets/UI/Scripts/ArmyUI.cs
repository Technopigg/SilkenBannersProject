using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ArmyUI : MonoBehaviour
{
    [Header("UI References")]
    public ArmyUIBannerAnimator bannerAnimator;  
    public TextMeshProUGUI ownerText;
    public Transform unitCardContainer;
    public GameObject unitCardPrefab;

    private readonly List<GameObject> spawnedCards = new List<GameObject>();

    public void ShowArmy(ArmyToken token)
    {
        if (token == null)
        {
            Clear();
            return;
        }
        
        bannerAnimator.OpenBanner();

        ownerText.text = token.owner;

        foreach (var c in spawnedCards)
            Destroy(c);
        spawnedCards.Clear();

        foreach (ArmyUnit unit in token.composition)
        {
            GameObject card = Instantiate(unitCardPrefab, unitCardContainer);
            spawnedCards.Add(card);

            UnitCardUI cardUI = card.GetComponent<UnitCardUI>();

            if (cardUI != null)
                cardUI.Set(unit.type, unit.count);
        }
    }

    public void Clear()
    {
        // --- animate closed ---
        bannerAnimator.CloseBanner();

        foreach (var c in spawnedCards)
            Destroy(c);

        spawnedCards.Clear();
    }
}