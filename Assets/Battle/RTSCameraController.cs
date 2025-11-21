using UnityEngine;

public class RTSCameraController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 35f;
    public float dragSpeed = 3f;
    public float edgeSpeed = 35f;
    public float edgeThickness = 20f;

    [Header("Zoom")]
    public float zoomSpeed = 80f;
    public float minHeight = 20f;
    public float maxHeight = 120f;

    private Camera cam;
    private Vector3 lastMousePos;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    // Called ONLY when ModeController switches to RTS mode
    public void OnActivated()
    {
        enabled = true;
        cam.enabled = true;

        lastMousePos = Vector3.zero;

        // Make sure ModeController receives correct initial height
        ModeController.Instance.ReportZoom(transform.position.y);

        // Unlock cursor for RTS
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        // ❌ DO NOT RUN IF NOT ACTIVE CAMERA
        if (!cam.enabled) return;
        if (!enabled) return;

        // ❌ DO NOT RUN IN GENERAL MODE
        if (ModeController.Instance.currentMode != ControlMode.RTS) return;

        Vector3 pos = transform.position;

        // ============================================================
        //                WASD MOVEMENT
        // ============================================================
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 forward = transform.forward; forward.y = 0f; forward.Normalize();
        Vector3 right = transform.right;     right.y = 0f;     right.Normalize();

        pos += (forward * v + right * h) * moveSpeed * Time.deltaTime;

        // ============================================================
        //                MIDDLE MOUSE DRAG
        // ============================================================
        if (Input.GetMouseButton(2))
        {
            if (lastMousePos != Vector3.zero)
            {
                Vector3 delta = Input.mousePosition - lastMousePos;
                pos -= (right * delta.x + forward * delta.y) * dragSpeed * Time.deltaTime;
            }
            lastMousePos = Input.mousePosition;
        }
        else
        {
            lastMousePos = Vector3.zero;
        }

        // ============================================================
        //                EDGE SCROLLING
        // ============================================================
        Vector3 mouse = Input.mousePosition;

        if (mouse.x <= edgeThickness)
            pos -= right * edgeSpeed * Time.deltaTime;
        else if (mouse.x >= Screen.width - edgeThickness)
            pos += right * edgeSpeed * Time.deltaTime;

        if (mouse.y <= edgeThickness)
            pos -= forward * edgeSpeed * Time.deltaTime;
        else if (mouse.y >= Screen.height - edgeThickness)
            pos += forward * edgeSpeed * Time.deltaTime;

        // ============================================================
        //                ZOOM (height based)
        // ============================================================
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            pos.y -= scroll * zoomSpeed;
            pos.y = Mathf.Clamp(pos.y, minHeight, maxHeight);
        }

        // ============================================================
        //           REPORT HEIGHT TO MODE CONTROLLER
        // (Used for switching back to General mode)
        // ============================================================
        ModeController.Instance.ReportZoom(pos.y);

        transform.position = pos;
    }
}
