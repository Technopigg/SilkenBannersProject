using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Squad : MonoBehaviour
{
    [Header("Team Info")]
    public int teamID = -1;

    [Header("Champion")]
    [Tooltip("If true, this squad represents a single Champion/General unit")]
    public bool isChampion = false;

    [Header("Squad Info")]
    public int squadID;
    public string owner;
    public string unitType;
    public List<Transform> soldiers = new List<Transform>();

    [Header("Formation Settings")]
    public int formationWidth = 5;
    public float spacing = 2f;
    public float arriveSlowDistance = 6f;
    public float minSpeedFactor = 0.3f;

    [Header("Movement")]
    public float squadBaseSpeed = 3.5f;

    [Header("Health System")]
    public float totalMaxHealth = 0f;
    public float totalCurrentHealth = 0f;
    public bool isSelected = false;

    [Header("UI")]
    public SquadHealthBar healthBar;

    [Header("Parent Army")]
    public ArmyToken armyToken; 

    void Awake()
    {
        if (soldiers == null)
            soldiers = new List<Transform>();
    }

    public void InitializeSquad()
    {
        foreach (Transform soldier in soldiers)
        {
            if (soldier == null) continue;

            UnitHealth uh = soldier.GetComponent<UnitHealth>();
            if (uh != null)
                uh.OnHealthChanged += OnUnitHealthChanged;

            UnitMovement mover = soldier.GetComponent<UnitMovement>();
            if (mover != null && isChampion)
            {
                mover.boostForChampion = true; 
            }
        }

   
        if (isChampion)
            squadBaseSpeed *= 1.3f;

        RecalculateMaxHealth();
        RecalculateCurrentHealth();
    }

    private void OnUnitHealthChanged(UnitHealth unit)
    {
        RecalculateCurrentHealth();
    }

    public void RecalculateMaxHealth()
    {
        totalMaxHealth = 0f;
        foreach (Transform soldier in soldiers)
        {
            if (soldier == null) continue;
            UnitHealth u = soldier.GetComponent<UnitHealth>();
            if (u != null)
                totalMaxHealth += u.maxHealth;
        }

        if (healthBar != null)
            healthBar.SetMaxHealth(totalMaxHealth);
    }

    public void RecalculateCurrentHealth()
    {
        totalCurrentHealth = 0f;
        foreach (Transform soldier in soldiers)
        {
            if (soldier == null) continue;
            UnitHealth u = soldier.GetComponent<UnitHealth>();
            if (u != null && !u.IsDead)
                totalCurrentHealth += u.currentHealth;
        }

        if (healthBar != null)
            healthBar.SetHealth(totalCurrentHealth);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (healthBar != null)
            healthBar.gameObject.SetActive(selected);

        foreach (var soldier in soldiers)
        {
            if (soldier == null) continue;

            var selection = soldier.GetComponent<UnitSelection>();
            if (selection != null)
                selection.SetSelected(selected);
        }
    }

    public Vector3 GetSquadCenter()
    {
        if (soldiers == null || soldiers.Count == 0)
            return transform.position;

        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (Transform soldier in soldiers)
        {
            if (soldier == null) continue;
            UnitHealth u = soldier.GetComponent<UnitHealth>();
            if (u != null && !u.IsDead)
            {
                sum += soldier.position;
                count++;
            }
        }

        return count > 0 ? sum / count : transform.position;
    }

    public void MoveSquad(Vector3 destination)
    {
        if (soldiers == null || soldiers.Count == 0)
            return;

        int count = soldiers.Count;
        int width = formationWidth;

        for (int i = 0; i < count; i++)
        {
            Transform soldier = soldiers[i];
            if (soldier == null) continue;

            int row = i / width;
            int col = i % width;

            Vector3 offset = new Vector3(
                col * spacing - ((width - 1) * spacing * 0.5f),
                0f,
                row * spacing
            );

            Vector3 targetPos = destination + offset;

            UnitMovement mover = soldier.GetComponent<UnitMovement>();
            if (mover != null)
            {
                float speed = squadBaseSpeed;
                mover.SetMovementTarget(targetPos, speed);
            }
        }
    }
}
