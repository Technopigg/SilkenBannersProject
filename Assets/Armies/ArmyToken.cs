using System.Collections.Generic;
using UnityEngine;

public class ArmyToken : MonoBehaviour
{
    [Header("Army Settings")]
    public string owner = "Player";

    // NEW — team ID (0 = Player, 1 = Enemy)
    public int teamID = 0;

    public float moveSpeed = 5f;

    [Header("Visuals")]
    public Color playerColor = Color.blue;
    public Color aiColor = Color.red;
    public Color neutralColor = Color.gray;
    public Color selectedColor = Color.yellow;

    [Header("Destination Marker")]
    public GameObject destinationMarkerPrefab;
    private GameObject activeMarker;

    private Vector3 targetPosition;
    private bool isSelected;
    private Renderer[] renderers;

    [Header("Army Composition (squads only)")]
    public List<ArmyUnit> composition = new List<ArmyUnit>();

    [Header("General unit (single)")]
    public GameObject generalPrefab;

    [Header("Battle State")]
    public bool isLockedInBattle = false;
    public bool canMoveOnWorldMap = true;

    // double-click detection
    private float lastClickTime;
    private const float doubleClickThreshold = 0.3f;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        targetPosition = transform.position;

        AssignTeamFromOwner();
        ApplyOwnerColor();
    }

    // ------------------------------------------------------------
    // TEAM LOGIC
    // ------------------------------------------------------------
    private void AssignTeamFromOwner()
    {
        // Automatic assignment based on your previous system:
        // Player = 0, AI = 1
        if (owner == "Player") teamID = 0;
        else if (owner == "AI") teamID = 1;
        else teamID = 2; // neutral or other

        // Propagate to army composition
        foreach (var unit in composition)
        {
            if (unit != null)
                unit.teamID = teamID;
        }
    }

    public void ForceSetTeam(int id)
    {
        teamID = id;

        foreach (var unit in composition)
        {
            if (unit != null)
                unit.teamID = id;
        }
    }

    // ------------------------------------------------------------
    // UPDATE LOOP
    // ------------------------------------------------------------
    void Update()
    {
        if (isLockedInBattle || !canMoveOnWorldMap)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (activeMarker != null &&
            Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            Destroy(activeMarker);
            activeMarker = null;
        }
    }

    // ------------------------------------------------------------
    // INPUT HANDLING
    // ------------------------------------------------------------
    void OnMouseDown()
    {
        if (Time.time - lastClickTime < doubleClickThreshold)
            HandleDoubleClick();

        lastClickTime = Time.time;
    }

    private void HandleDoubleClick()
    {
        BattleEngagement engagement = BattleManager.Instance.GetCurrentEngagement();
        if (engagement == null)
        {
            Debug.Log($"{name}: No stored battle to resume.");
            return;
        }

        if (engagement.player == this || engagement.enemy == this)
        {
            Debug.Log($"{name}: Resuming active battle...");
            BattleManager.Instance.BeginBattle(engagement);
        }
    }

    // ------------------------------------------------------------
    // SELECTION
    // ------------------------------------------------------------
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (selected) ApplyColor(selectedColor);
        else ApplyOwnerColor();
    }

    // ------------------------------------------------------------
    // MOVEMENT COMMAND
    // ------------------------------------------------------------
    public void SetTarget(Vector3 pos)
    {
        if (isLockedInBattle || !canMoveOnWorldMap)
        {
            Debug.Log($"{name} cannot move (locked).");
            return;
        }

        targetPosition = new Vector3(pos.x, transform.position.y, pos.z);

        if (destinationMarkerPrefab != null)
        {
            if (activeMarker == null)
                activeMarker = Instantiate(destinationMarkerPrefab, targetPosition, Quaternion.identity);
            else
                activeMarker.transform.position = targetPosition;
        }
    }

    // ------------------------------------------------------------
    // COLOR SYSTEM
    // ------------------------------------------------------------
    private void ApplyOwnerColor()
    {
        switch (owner)
        {
            case "Player": ApplyColor(playerColor); break;
            case "AI": ApplyColor(aiColor); break;
            default: ApplyColor(neutralColor); break;
        }
    }

    private void ApplyColor(Color c)
    {
        if (renderers == null) return;

        foreach (var r in renderers)
            r.material.color = c;
    }

    // ------------------------------------------------------------
    // BATTLE STATE LOGIC
    // ------------------------------------------------------------
    public void LockInBattle()
    {
        isLockedInBattle = true;
        canMoveOnWorldMap = false;

        foreach (var unit in composition)
            if (unit != null)
                unit.isLockedInBattle = true;

        Debug.Log($"{name}: LOCKED in battle.");
    }

    public void UnlockFromBattle()
    {
        isLockedInBattle = false;
        canMoveOnWorldMap = true;

        foreach (var unit in composition)
            if (unit != null)
                unit.isLockedInBattle = false;

        Debug.Log($"{name}: UNLOCKED from battle.");
    }
}
