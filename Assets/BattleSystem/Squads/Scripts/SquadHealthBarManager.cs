using UnityEngine;

public class SquadHealthBarManager : MonoBehaviour
{
    [Header("References")]
    public SquadHealthBar healthBar;  
    public Camera mainCamera;

    void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (healthBar != null)
            healthBar.gameObject.SetActive(false);
    }

    void Update()
    {
        if (healthBar == null) return;

        Squad selectedSquad = FindSelectedSquad();

        if (selectedSquad != null)
        {
            healthBar.gameObject.SetActive(true);
            healthBar.squad = selectedSquad;
        }
        else
        {
            healthBar.gameObject.SetActive(false);
        }
    }

    Squad FindSelectedSquad()
    {
        Squad[] squads = FindObjectsOfType<Squad>();
        foreach (var s in squads)
        {
            if (s.isSelected)
                return s;
        }
        return null;
    }
}