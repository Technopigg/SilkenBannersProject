using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DragSelection : MonoBehaviour
{
    [Header("UI")]
    public RectTransform selectionBox;

    [Header("Settings")]
    public float dragThreshold = 8f;
    public bool additiveWithShift = true;

    private Vector2 startPos;
    private Vector2 endPos;

    private Camera rtsCamera;

    void Awake()
    {
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
        if (ModeController.Instance == null ||
            ModeController.Instance.currentMode != ControlMode.RTS)
        {
            if (selectionBox != null && selectionBox.gameObject.activeSelf)
                selectionBox.gameObject.SetActive(false);
            return;
        }

        if (rtsCamera == null && ModeController.Instance != null)
        {
            rtsCamera = ModeController.Instance.rtsCamera;
        }
        if (rtsCamera == null)
        {
            Debug.LogError("RTS Camera is NULL in DragSelection!");
            return;
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            endPos = startPos;

            if (selectionBox != null)
            {
                selectionBox.gameObject.SetActive(true);
                UpdateSelectionBox();
            }
        }

        if (Input.GetMouseButton(0))
        {
            endPos = Input.mousePosition;
            if (selectionBox != null)
            {
                UpdateSelectionBox();
            }
        }

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

        Vector2 localMin, localMax;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            selectionBox.parent as RectTransform, min, null, out localMin);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            selectionBox.parent as RectTransform, max, null, out localMax);

        Vector2 localCenter = (localMin + localMax) / 2f;
        Vector2 localSize = localMax - localMin;

        selectionBox.anchoredPosition = localCenter;
        selectionBox.sizeDelta = new Vector2(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y));
    }

    void SelectUnitsInBox(Vector2 min, Vector2 max)
    {
        if (ModeController.Instance != null)
            rtsCamera = ModeController.Instance.rtsCamera;

        if (rtsCamera == null)
        {
            Debug.LogError("RTS Camera is NULL when selecting units!");
            return;
        }

        bool additive = additiveWithShift &&
                        (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

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
            if (squad == null) continue;

            bool squadSelected = false;

            foreach (var soldier in squad.soldiers)
            {
                if (soldier == null) continue;

                Vector3 sp = rtsCamera.WorldToScreenPoint(soldier.transform.position);

                if (sp.z < 0f) continue;

                if (sp.x >= min.x && sp.x <= max.x &&
                    sp.y >= min.y && sp.y <= max.y)
                {
                    squadSelected = true;
                    break;
                }
            }

            if (squadSelected)
            {
                // 🔥 BLOCK ENEMY SQUADS 🔥
                if (squad.teamID == 0) // 0 = Player team
                {
                    squad.SetSelected(true);
                    selectedNow.Add(squad);
                }
            }
        }

        if (selectedNow.Count > 0)
        {
            RTSSelectionManager mgr = FindObjectOfType<RTSSelectionManager>();
            if (mgr != null)
            {
                mgr.SetSelectedSquads(selectedNow);
            }
        }
    }
}
