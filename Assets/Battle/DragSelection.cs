using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DragSelection : MonoBehaviour
{
    [Header("UI")]
    public RectTransform selectionBox; // assign a transparent Image under a full-screen Canvas

    [Header("Settings")]
    public float dragThreshold = 8f;   // pixels: below this, treat as click
    public bool additiveWithShift = true;

    private Vector2 startPos;
    private Vector2 endPos;

    private Camera rtsCamera;

    void Awake()
    {
        // Cache RTS camera if ModeController is already alive
        if (ModeController.Instance != null)
        {
            rtsCamera = ModeController.Instance.rtsCamera;
        }

        if (selectionBox != null)
        {
            selectionBox.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Only active in RTS mode
        if (ModeController.Instance == null ||
            ModeController.Instance.currentMode != ControlMode.RTS)
        {
            if (selectionBox != null && selectionBox.gameObject.activeSelf)
                selectionBox.gameObject.SetActive(false);
            return;
        }

        // Ensure camera reference is always valid
        if (rtsCamera == null && ModeController.Instance != null)
        {
            rtsCamera = ModeController.Instance.rtsCamera;
        }
        if (rtsCamera == null)
        {
            Debug.LogError("RTS Camera is NULL in DragSelection!");
            return;
        }

        // === Start drag ===
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            endPos = startPos;

            if (selectionBox != null)
            {
                selectionBox.gameObject.SetActive(true);
                UpdateSelectionBox(); // Initialize box at 0 size
            }
        }

        // === Update drag ===
        if (Input.GetMouseButton(0))
        {
            endPos = Input.mousePosition;
            if (selectionBox != null)
            {
                UpdateSelectionBox();
            }
        }

        // === Release drag ===
        if (Input.GetMouseButtonUp(0))
        {
            Vector2 min = Vector2.Min(startPos, endPos);
            Vector2 max = Vector2.Max(startPos, endPos);
            Vector2 size = max - min;

            bool isDrag = size.x >= dragThreshold && size.y >= dragThreshold;

            if (isDrag)
            {
                SelectUnitsInBox(min, max);
            }
            else
            {
                // Small drag: fallback to single-click selection if desired
                // Example: raycast a single squad here
            }

            if (selectionBox != null)
            {
                selectionBox.gameObject.SetActive(false);
            }
        }
    }

    void UpdateSelectionBox()
    {
        if (selectionBox == null) return;

        Vector2 min = Vector2.Min(startPos, endPos);
        Vector2 max = Vector2.Max(startPos, endPos);

        // Convert screen-space to local-space of the parent canvas
        Vector2 localMin, localMax;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            selectionBox.parent as RectTransform, min, null, out localMin);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            selectionBox.parent as RectTransform, max, null, out localMax);

        // Calculate center and size
        Vector2 localCenter = (localMin + localMax) / 2f;
        Vector2 localSize = localMax - localMin;

        selectionBox.anchoredPosition = localCenter;
        selectionBox.sizeDelta = new Vector2(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y));
    }

    void SelectUnitsInBox(Vector2 min, Vector2 max)
    {
        // Refresh RTS camera every time
        if (ModeController.Instance != null)
            rtsCamera = ModeController.Instance.rtsCamera;

        if (rtsCamera == null)
        {
            Debug.LogError("RTS Camera is NULL when selecting units!");
            return;
        }

        bool additive = additiveWithShift &&
                        (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

        // Clear previous selection if not additive
        if (!additive)
        {
            foreach (Squad s in FindObjectsOfType<Squad>())
            {
                if (s != null) s.SetSelected(false);
            }
        }

        List<Squad> selectedNow = new List<Squad>();

        foreach (Squad squad in FindObjectsOfType<Squad>())
        {
            if (squad == null) continue; // skip destroyed squads

            Vector3 screenPos = rtsCamera.WorldToScreenPoint(squad.GetSquadCenter());

            // Ignore units behind the camera
            if (screenPos.z < 0f) continue;

            if (screenPos.x >= min.x && screenPos.x <= max.x &&
                screenPos.y >= min.y && screenPos.y <= max.y)
            {
                squad.SetSelected(true);
                selectedNow.Add(squad);
            }
        }

        // Optional: set a "primary" squad for move orders
        if (selectedNow.Count > 0)
        {
            RTSSelectionManager mgr = FindObjectOfType<RTSSelectionManager>();
            if (mgr != null)
            {
                mgr.SelectSquad(selectedNow[0]); // Ensure SelectSquad is public
            }
        }
    }
}