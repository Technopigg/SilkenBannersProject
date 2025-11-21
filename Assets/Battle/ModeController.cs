using UnityEngine;

public class ModeController : MonoBehaviour
{
    public static ModeController Instance { get; private set; }

    [Header("Camera references (assign in inspector)")]
    public Camera playerCamera;
    public Camera rtsCamera;

    [Header("Controller scripts (assign in inspector)")]
    public Player3PCamera playerController;
    public RTSCameraController rtsController;

    [Header("Zoom thresholds (general <-> rts)")]
    public float switchToRTS_Threshold = 30f;
    public float switchToGeneral_Threshold = 20f;

    [HideInInspector] public ControlMode currentMode = ControlMode.General;

    private float lastReportedZoom = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        TryAutoBind();
        SetMode(ControlMode.General, instant: true);
    }

    void TryAutoBind()
    {
        if (playerCamera == null)
            playerCamera = GameObject.Find("PlayerCamera")?.GetComponent<Camera>();

        if (rtsCamera == null)
            rtsCamera = GameObject.Find("RTSCamera")?.GetComponent<Camera>();

        if (playerController == null && playerCamera != null)
            playerController = playerCamera.GetComponent<Player3PCamera>();

        if (rtsController == null && rtsCamera != null)
            rtsController = rtsCamera.GetComponent<RTSCameraController>();
    }

    // -------------------------------------------------------
    //     Zoom-driven switching
    // -------------------------------------------------------
    public void ReportZoom(float zoomValue)
    {
        lastReportedZoom = zoomValue;
        Debug.Log("[ModeController] Zoom reported = " + zoomValue);

        if (currentMode == ControlMode.General)
        {
            if (zoomValue > switchToRTS_Threshold)
                SetMode(ControlMode.RTS);
        }
        else
        {
            if (zoomValue < switchToGeneral_Threshold)
                SetMode(ControlMode.General);
        }
    }

    // -------------------------------------------------------
    //     Main mode switch entry
    // -------------------------------------------------------
    public void SetMode(ControlMode mode, bool instant = false)
    {
        if (currentMode == mode) return;

        currentMode = mode;

        if (mode == ControlMode.General)
            ActivateGeneralMode(instant);
        else
            ActivateRTSMode(instant);
    }

    // -------------------------------------------------------
    //     GENERAL MODE
    // -------------------------------------------------------
    private void ActivateGeneralMode(bool instant)
    {
        TryAutoBind();

        // Sync positions just once if needed
        if (instant)
        {
            playerCamera.transform.position = rtsCamera.transform.position;
            playerCamera.transform.rotation = rtsCamera.transform.rotation;
        }

        // Always disable both first
        playerCamera.enabled = false;
        rtsCamera.enabled = false;
        playerController.enabled = false;
        rtsController.enabled = false;

        // Enable correct
        playerCamera.enabled = true;
        playerController.enabled = true;

        // Cursor lock for 3rd person
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Notify script
        playerController.OnActivated();

        Debug.Log("[ModeController] Switched to GENERAL mode");
    }

    // -------------------------------------------------------
    //     RTS MODE
    // -------------------------------------------------------
    private void ActivateRTSMode(bool instant)
    {
        TryAutoBind();

        if (instant)
        {
            rtsCamera.transform.position = playerCamera.transform.position;
            rtsCamera.transform.rotation = playerCamera.transform.rotation;
        }

        // Always disable both first
        playerCamera.enabled = false;
        rtsCamera.enabled = false;
        playerController.enabled = false;
        rtsController.enabled = false;

        // Enable correct
        rtsCamera.enabled = true;
        rtsController.enabled = true;

        // RTS cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        rtsController.OnActivated();

        Debug.Log("[ModeController] Switched to RTS mode");
    }

    // -------------------------------------------------------
    // Compatibility helpers
    // -------------------------------------------------------
    public void ApplyCameraState(ControlMode mode, bool align = true)
    {
        SetMode(mode, align);
    }

    public void ResetMode()
    {
        SetMode(ControlMode.General, instant: true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("[ModeController] ResetMode() called.");
    }

    public void SetTarget(Transform t)
    {
        if (playerController != null)
            playerController.target = t;
    }
}
