using UnityEngine;

public class RTSCameraController : MonoBehaviour
{
    [Header("Pan")]
    public float panSpeed = 25f;

    [Header("Mouse Drag")]
    public float dragSpeed = 3f;

    [Header("Edge Scrolling")]
    public float edgeScrollSpeed = 25f;
    public int edgeThickness = 20;

    [Header("Zoom")]
    public float zoomSpeed = 60f;
    public float minHeight = 10f;
    public float maxHeight = 120f;
    public float pitchAtMin = 45f;
    public float pitchAtMax = 75f;

    [Header("Bounds")]
    public Vector2 xLimits = new Vector2(0f, 100f);
    public Vector2 zLimits = new Vector2(0f, 100f);
    public float borderPadding = 5f;

    private Camera cam;
    private Vector3 lastMousePos;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("RTSCameraController requires a Camera component!");
        }
    }

    void Start()
    {
        
        if (Terrain.activeTerrain != null)
        {
            var size = Terrain.activeTerrain.terrainData.size;
            xLimits = new Vector2(borderPadding, size.x - borderPadding);
            zLimits = new Vector2(borderPadding, size.z - borderPadding);
        }
    }


    // Called by ModeController
    public void ActivateRTS()
    {
        if (cam) cam.enabled = true;
        enabled = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Called by ModeController
    public void DeactivateRTS()
    {
        if (cam) cam.enabled = false;
        enabled = false;
    }

    void Update()
    {
        // Prevent movement when camera is off
        if (!cam || !cam.enabled) return;

        // Prevent movement if NOT in RTS mode
        if (ModeController.Instance == null ||
            ModeController.Instance.currentMode != ControlMode.RTS)
        {
            lastMousePos = Vector3.zero;
            return;
        }

        Vector3 forward = transform.forward; forward.y = 0f; forward.Normalize();
        Vector3 right = transform.right; right.y = 0f; right.Normalize();

        Vector3 next = transform.position;

        // === WASD movement ===
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        next += (forward * v + right * h) * panSpeed * Time.deltaTime;

        // === Middle mouse drag ===
        if (Input.GetMouseButton(2))
        {
            if (lastMousePos != Vector3.zero)
            {
                Vector3 delta = Input.mousePosition - lastMousePos;
                next -= (right * delta.x + forward * delta.y) * dragSpeed * Time.deltaTime;
            }
            lastMousePos = Input.mousePosition;
        }
        else
        {
            lastMousePos = Vector3.zero;
        }

        // === Edge scrolling ===
        Vector3 mousePos = Input.mousePosition;
        if (mousePos.x <= edgeThickness)
            next -= right * edgeScrollSpeed * Time.deltaTime;
        else if (mousePos.x >= Screen.width - edgeThickness)
            next += right * edgeScrollSpeed * Time.deltaTime;

        if (mousePos.y <= edgeThickness)
            next -= forward * edgeScrollSpeed * Time.deltaTime;
        else if (mousePos.y >= Screen.height - edgeThickness)
            next += forward * edgeScrollSpeed * Time.deltaTime;

        // === Zoom ===
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            float targetY = Mathf.Clamp(next.y - scroll * zoomSpeed, minHeight, maxHeight);
            next.y = targetY;

            float t = Mathf.InverseLerp(minHeight, maxHeight, targetY);
            float pitch = Mathf.Lerp(pitchAtMin, pitchAtMax, t);

            Vector3 euler = transform.rotation.eulerAngles;
            euler.x = pitch;
            transform.rotation = Quaternion.Euler(euler);
        }

        // === Apply battlefield bounds ===
        next.x = Mathf.Clamp(next.x, xLimits.x, xLimits.y);
        next.z = Mathf.Clamp(next.z, zLimits.x, zLimits.y);

        transform.position = next;
    }
}
