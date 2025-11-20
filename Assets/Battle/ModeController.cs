using UnityEngine;

public enum ControlMode
{
    General,
    RTS,
    Neutral
}

public class ModeController : MonoBehaviour
{
    public static ModeController Instance { get; private set; }

    [Header("Battle Cameras")]
    public Camera playerCamera;
    public Camera rtsCamera;

    private RTSCameraController rtsController;
    private Player3PCamera generalController;

    [Header("Current Mode (read-only)")]
    public ControlMode currentMode = ControlMode.Neutral;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Debug.Log("<color=yellow>[ModeController] Awake()</color>");

        BindBattleCameras();
    }

    void OnEnable()
    {
        BindBattleCameras();
    }

    /// <summary>
    /// Safely binds ONLY Battlefield cameras.
    /// Never binds WorldMap cameras.
    /// </summary>
    public void BindBattleCameras()
    {
        if (!BattleRootExists())
        {
            // We are in WorldMap or loading state — clear camera refs
            playerCamera = null;
            rtsCamera = null;
            rtsController = null;
            generalController = null;
            return;
        }

        // Find cameras
        if (playerCamera == null)
            playerCamera = GameObject.Find("PlayerCamera")?.GetComponent<Camera>();

        if (rtsCamera == null)
            rtsCamera = GameObject.Find("RTSCamera")?.GetComponent<Camera>();

        // Bind controllers
        if (playerCamera != null)
            generalController = playerCamera.GetComponent<Player3PCamera>();

        if (rtsCamera != null)
            rtsController = rtsCamera.GetComponent<RTSCameraController>();

        Debug.Log($"<color=cyan>[ModeController] Bound Cameras → Player={playerCamera != null}  RTS={rtsCamera != null}</color>");
    }

    private bool BattleRootExists()
    {
        return GameObject.Find("BattleCameraRoot") != null;
    }

    /// <summary>
    /// Public API: Switch mode (called by input system or game logic)
    /// </summary>
    public void SetMode(ControlMode mode)
    {
        if (currentMode == mode)
            return;

        currentMode = mode;
        ApplyCameraState(mode, align: true);

        Debug.Log("<color=green>[ModeController] Switched to mode: " + currentMode + "</color>");
    }

    /// <summary>
    /// Actually switches camera states.
    /// </summary>
    public void ApplyCameraState(ControlMode mode, bool align)
{
    BindBattleCameras();

    if (playerCamera == null || rtsCamera == null)
    {
        Debug.LogWarning("[ModeController] Cameras not bound yet — ignoring mode switch");
        return;
    }

    // --- ENSURE BOTH CAMERA GAMEOBJECTS ARE ACTIVE ---
    playerCamera.gameObject.SetActive(true);
    rtsCamera.gameObject.SetActive(true);

    // ==========================
    // GENERAL MODE
    // ==========================
    if (mode == ControlMode.General)
    {
        if (align)
        {
            playerCamera.transform.position = rtsCamera.transform.position;
            playerCamera.transform.rotation = rtsCamera.transform.rotation;
        }

        playerCamera.enabled = true;

        if (rtsController != null)
            rtsController.DeactivateRTS();

        rtsCamera.enabled = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("<color=orange>[ModeController] GENERAL camera active</color>");
        return;
    }

    // ==========================
    // RTS MODE
    // ==========================
    if (mode == ControlMode.RTS)
    {
        if (align)
        {
            rtsCamera.transform.position = playerCamera.transform.position;
            rtsCamera.transform.rotation = playerCamera.transform.rotation;
        }

        playerCamera.enabled = false;

        if (rtsController != null)
            rtsController.ActivateRTS();

        rtsCamera.enabled = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("<color=orange>[ModeController] RTS camera active</color>");
        return;
    }

    // ==========================
    // NEUTRAL
    // ==========================
    playerCamera.enabled = false;
    rtsCamera.enabled = false;

    if (rtsController != null)
        rtsController.DeactivateRTS();

    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;

    Debug.Log("<color=grey>[ModeController] Neutral mode</color>");
}


    public void ResetMode()
    {
        currentMode = ControlMode.Neutral;
        ApplyCameraState(currentMode, align: false);
    }
    
    void Update()
    {
        // TEMP DEBUG INPUT
        if (Input.GetKeyDown(KeyCode.F1))
            
        {
            Debug.Log("F1 → Switch to General mode");
            SetMode(ControlMode.General);
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            Debug.Log("F2 → Switch to RTS mode");
            SetMode(ControlMode.RTS);
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            Debug.Log("F3 → Switch to Neutral mode");
            SetMode(ControlMode.Neutral);
        }
    }
}
