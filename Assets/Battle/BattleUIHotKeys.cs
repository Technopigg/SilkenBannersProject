using UnityEngine;
using UnityEngine.UI;

public class BattleUIHotkeys : MonoBehaviour
{
    public Button exitButton;
    private BattleSceneController controller;

    void Awake()
    {
        controller = FindObjectOfType<BattleSceneController>();
    }

    void Start()
    {
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(() =>
            {
                if (ModeController.Instance != null)
                {
                    ModeController.Instance.ResetMode();  // now exists and is correct
                }

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                controller.ExitToWorldMapTemporary();
            });
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            controller.ExitToWorldMapTemporary();
        }
    }
}