using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ArmyUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject bannerRoot;            // The whole banner (set active when army selected)
    public TextMeshProUGUI ownerText;        // Displays owner
    public Transform unitCardContainer;      // Where cards spawn
    public GameObject unitCardPrefab;        // Prefab for each card

    private readonly List<GameObject> spawnedCards = new List<GameObject>();

    public void ShowArmy(ArmyToken token)
    {
        if (token == null)
        {
            Clear();
            return;
        }

        bannerRoot.SetActive(true);

        // Set owner name
        ownerText.text = token.owner;

        // Clear previous cards
        foreach (var c in spawnedCards)
            Destroy(c);

        spawnedCards.Clear();

        // Spawn a card for each unit in composition
        foreach (ArmyUnit unit in token.composition)
        {
            GameObject card = Instantiate(unitCardPrefab, unitCardContainer);
            spawnedCards.Add(card);

            // Fill the UI elements on the card
            UnitCardUI cardUI = card.GetComponent<UnitCardUI>();

            if (cardUI != null)
            {
                cardUI.Set(unit.type, unit.count);
            }
            else
            {
                Debug.LogWarning("UnitCard prefab missing UnitCardUI script!");
            }
        }
    }

    public void Clear()
    {
        bannerRoot.SetActive(false);

        foreach (var c in spawnedCards)
            Destroy(c);

        spawnedCards.Clear();
    }
}