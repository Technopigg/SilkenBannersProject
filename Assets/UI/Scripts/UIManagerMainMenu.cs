using UnityEngine;
using UnityEngine.UIElements;

public class UIManagerMainMenu : MonoBehaviour
{
    private VisualElement root;

    private VisualElement mainMenuPanel;
    private VisualElement questsPanel;
    private VisualElement diplomacyPanel;

    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        mainMenuPanel = root.Q<VisualElement>("MainMenuPanel");
        questsPanel = root.Q<VisualElement>("QuestsPanel");
        diplomacyPanel = root.Q<VisualElement>("DiplomacyPanel");

        CheckForNulls();
        HideAll();
    }

    private void CheckForNulls()
    {
        if (mainMenuPanel == null)
            Debug.LogWarning("⚠ MainMenuPanel NOT FOUND in UXML!");

        if (questsPanel == null)
            Debug.LogWarning("⚠ QuestsPanel NOT FOUND in UXML!");

        if (diplomacyPanel == null)
            Debug.LogWarning("⚠ DiplomacyPanel NOT FOUND in UXML!");
    }

    private void HideAll()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.style.display = DisplayStyle.None;

        if (questsPanel != null)
            questsPanel.style.display = DisplayStyle.None;

        if (diplomacyPanel != null)
            diplomacyPanel.style.display = DisplayStyle.None;
    }

    public void ShowMainMenu()
    {
        // TOGGLE FIX: Check resolvedStyle instead of style
        if (mainMenuPanel.resolvedStyle.display == DisplayStyle.Flex)
        {
            HideAll();
            return;
        }

        HideAll();
        mainMenuPanel.style.display = DisplayStyle.Flex;
    }

    public void ShowQuests()
    {
        if (questsPanel.resolvedStyle.display == DisplayStyle.Flex)
        {
            HideAll();
            return;
        }

        HideAll();
        questsPanel.style.display = DisplayStyle.Flex;
    }

    public void ShowDiplomacy()
    {
        if (diplomacyPanel.resolvedStyle.display == DisplayStyle.Flex)
        {
            HideAll();
            return;
        }

        HideAll();
        diplomacyPanel.style.display = DisplayStyle.Flex;
    }
}
