using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ArmyUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject bannerRoot;           
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
        bannerRoot.SetActive(true);
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