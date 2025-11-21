using UnityEngine;

[RequireComponent(typeof(Camera))]
public class Player3PCamera : MonoBehaviour
{
    [Header("References")]
    public Camera generalCam;   // This object's Camera (auto-assigned)
    public Transform target;    // Assigned by BattleSceneController

    [Header("General Camera Settings")]
    public Vector3 offset = new Vector3(0f, 5f, -10f);
    public float rotateSpeed = 80f;
    public float zoomSpeed = 10f;
    public float minZoom = 5f;
    public float maxZoom = 30f;
    public float smooth = 10f;

    private float yaw;
    private float currentZoom;

    void Awake()
    {
        if (generalCam == null)
            generalCam = GetComponent<Camera>();

        currentZoom = offset.magnitude;

        // Initialize position if target exists
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.LookAt(target);
        }
    }

    // Called by ModeController when switching BACK to General mode
    public void OnActivated()
    {
        enabled = true;
        generalCam.enabled = true;

        if (target != null)
        {
            transform.position = target.position + offset;
            transform.LookAt(target);
        }

        // lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        // 🚨 CRITICAL FIX: DO NOT RUN WHILE IN RTS MODE
        if (ModeController.Instance != null &&
            ModeController.Instance.currentMode != ControlMode.General)
            return;

        if (!generalCam || !generalCam.enabled) return;
        if (target == null) return;

        HandleOrbit();
        HandleZoom();
        ApplyCameraPosition();

        // Report zoom for hysteresis switching (ModeController handles switching)
        ReportZoomToModeController();
    }

    void HandleOrbit()
    {
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
            yaw += mouseX;
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentZoom = Mathf.Clamp(currentZoom - scroll * zoomSpeed, minZoom, maxZoom);
    }

    void ApplyCameraPosition()
    {
        Vector3 baseOffset = new Vector3(0f, offset.y, offset.z).normalized * currentZoom;
        Quaternion orbit = Quaternion.Euler(0f, yaw, 0f);
        Vector3 desiredPos = target.position + orbit * baseOffset;

        transform.position = Vector3.Lerp(transform.position, desiredPos, smooth * Time.deltaTime);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(target.position - transform.position, Vector3.up),
            smooth * Time.deltaTime
        );
    }

    // 🚨 We removed CheckForRTSSwitch() COMPLETELY.
    // ModeController is now the ONLY system that triggers switching.

    void ReportZoomToModeController()
    {
        if (ModeController.Instance != null)
            ModeController.Instance.ReportZoom(currentZoom);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        yaw = 0f;

        if (target != null)
        {
            transform.position = target.position + offset;
            transform.LookAt(target);
        }

        Debug.Log("Player3PCamera: Target explicitly set via SetTarget().");
    }
}
