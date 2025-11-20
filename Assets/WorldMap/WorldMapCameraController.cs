using UnityEngine;

public class WorldMapCameraController : MonoBehaviour
{
    public Transform target;        // Optional: follow the selected army
    public float panSpeed = 20f;
    public float zoomSpeed = 200f;
    public float minZoom = 20f;
    public float maxZoom = 120f;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        HandlePan();
        HandleZoom();
        FollowTarget();
    }

    void HandlePan()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(h, 0, v) * panSpeed * Time.deltaTime;
        transform.position += move;
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            cam.fieldOfView -= scroll * zoomSpeed * Time.deltaTime;
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, minZoom, maxZoom);
        }
    }

    void FollowTarget()
    {
        if (target == null) return;

        Vector3 pos = transform.position;
        pos.x = target.position.x;
        pos.z = target.position.z;
        transform.position = pos;
    }

    public void SetFollowTarget(Transform t)
    {
        target = t;
    }
}