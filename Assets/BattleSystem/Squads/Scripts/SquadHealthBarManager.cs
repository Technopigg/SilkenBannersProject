using UnityEngine;
using UnityEngine.UI;

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
        {
            healthBar.gameObject.SetActive(false); 
        }
    }

    void Update()
    {
        if (healthBar == null) return;

        // Find the currently selected squad
        Squad selectedSquad = FindSelectedSquad();
        if (selectedSquad != null)
        {
            healthBar.gameObject.SetActive(true);
            healthBar.squad = selectedSquad;


            if (healthBar.followOffset == null)
                healthBar.transform.position = selectedSquad.GetSquadCenter() + Vector3.up * 2f;


            if (mainCamera != null)
                healthBar.transform.LookAt(mainCamera.transform);
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