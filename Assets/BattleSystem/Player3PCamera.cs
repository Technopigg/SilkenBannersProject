using UnityEngine;

public class Player3PCamera : MonoBehaviour
{
    [Header("References")]
    public Camera generalCam;   // THIS camera's Camera component
    public Camera rtsCam;       // The RTS camera (separate object)
    public Transform target;    // General / Player unit

    [Header("General Camera Settings")]
    public Vector3 offset = new Vector3(0f, 5f, -10f);
    public float rotateSpeed = 80f;
    public float zoomSpeed = 10f;
    public float minZoom = 5f;
    public float maxZoom = 30f;
    public float smooth = 10f;

    [Header("RTS Switching")]
    public float rtsZoomThreshold = 25f;
    public float modeSwitchHysteresis = 1f;

    private float yaw;
    private float currentZoom;


    void Awake()
    {
        if (generalCam == null)
            generalCam = GetComponent<Camera>();

        currentZoom = offset.magnitude;

        if (target != null)
        {
            transform.position = target.position + offset;
            transform.LookAt(target);
        }
    }

    void OnEnable()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.LookAt(target);
        }

  
        if (ModeController.Instance != null &&
            ModeController.Instance.currentMode != ControlMode.General)
        {
            generalCam.enabled = false;
        }
    }

    void LateUpdate()
    {
        if (!generalCam.enabled) return;
        if (target == null) return;

        HandleOrbit();
        HandleZoom();
        ApplyCameraPosition();
        CheckForRTSSwitch();
    }


    //────────────────────────────────────────────────────────────
    //  General-mode orbit + rotation
    //────────────────────────────────────────────────────────────
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


    //────────────────────────────────────────────────────────────
    //  Switching to RTS mode by zoom threshold
    //────────────────────────────────────────────────────────────
    void CheckForRTSSwitch()
    {
        if (ModeController.Instance == null) return;

        // Switch TO RTS mode
        if (currentZoom > rtsZoomThreshold + modeSwitchHysteresis &&
            ModeController.Instance.currentMode != ControlMode.RTS)
        {
            ModeController.Instance.SetMode(ControlMode.RTS);
            DisableGeneralCamera();
        }

        // Switch BACK to General mode
        if (currentZoom < rtsZoomThreshold - modeSwitchHysteresis &&
            ModeController.Instance.currentMode != ControlMode.General)
        {
            ModeController.Instance.SetMode(ControlMode.General);
            EnableGeneralCamera();
        }
    }


    //────────────────────────────────────────────────────────────
    //  Camera activation helpers
    //────────────────────────────────────────────────────────────
    void DisableGeneralCamera()
    {
        generalCam.enabled = false;
    }

    void EnableGeneralCamera()
    {
        generalCam.enabled = true;
    }


    //────────────────────────────────────────────────────────────
    //  External API for the battle system
    //────────────────────────────────────────────────────────────
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        yaw = 0f;
        transform.position = target.position + offset;
        transform.LookAt(target);

        Debug.Log("Player3PCamera: Target explicitly set via SetTarget().");
    }
}
