using System.Collections.Generic;
using UnityEngine;

public class ArmyToken : MonoBehaviour
{
    [Header("Army Settings")]
    public string owner = "Player"; 
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
        ApplyOwnerColor();
    }

    void Update()
    {
        if (isLockedInBattle || !canMoveOnWorldMap) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (activeMarker != null && Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            Destroy(activeMarker);
            activeMarker = null;
        }
    }

    void OnMouseDown()
    {
        if (Time.time - lastClickTime < doubleClickThreshold)
        {
            HandleDoubleClick();
        }

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

        // ensure this token belongs to the active engagement
        if (engagement.player == this || engagement.enemy == this)
        {
            Debug.Log($"{name}: Resuming active battle...");
            BattleManager.Instance.BeginBattle(engagement);
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (selected) ApplyColor(selectedColor);
        else ApplyOwnerColor();
    }

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
        foreach (var r in renderers)
            r.material.color = c;
    }

    public void LockInBattle()
    {
        isLockedInBattle = true;
        canMoveOnWorldMap = false;

        foreach (var unit in composition)
        {
            if (unit != null)
                unit.isLockedInBattle = true;
        }

        Debug.Log($"{name}: LOCKED in battle.");
    }

    public void UnlockFromBattle()
    {
        isLockedInBattle = false;
        canMoveOnWorldMap = true;

        foreach (var unit in composition)
        {
            if (unit != null)
                unit.isLockedInBattle = false;
        }

        Debug.Log($"{name}: UNLOCKED from battle.");
    }
}
