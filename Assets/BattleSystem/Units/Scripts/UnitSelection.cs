using UnityEngine;
using System.Linq;

public class UnitSelection : MonoBehaviour
{
    private GameObject selectionCircle;

    void Awake()
    {
        var allChildren = GetComponentsInChildren<Transform>(true);
        var match = allChildren.FirstOrDefault(t => t.name == "SelectionCircle");

        if (match != null)
        {
            selectionCircle = match.gameObject;
            selectionCircle.SetActive(false); // hidden by default
        }
        else
        {
            Debug.LogWarning($"SelectionCircle not found under {name}");
        }
    }

    public void SetSelected(bool isSelected)
    {
        Debug.Log($"{name} SetSelected({isSelected})");
        if (selectionCircle != null)
        {
            selectionCircle.SetActive(isSelected);
        }
    }
}