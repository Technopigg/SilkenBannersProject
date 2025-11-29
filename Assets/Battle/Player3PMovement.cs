using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class Player3PMovement : MonoBehaviour
{
    [Header("Control")]
    public bool isPlayerControlled = false;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float sprintMultiplier = 1.6f;
    public float acceleration = 15f;

    [Header("Rotation")]
    public float mouseSensitivity = 80f;
    public float turnSmoothing = 10f;

    [Header("Grounding")]
    public float bodyHeightOffset = 0.5f;

    private Rigidbody rb;
    private Animator anim;
    private float yaw;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        yaw = transform.eulerAngles.y;
    }

    void Update()
    {
        if (!isPlayerControlled) return;

        if (ModeController.Instance != null &&
            ModeController.Instance.currentMode == ControlMode.RTS)
        {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        yaw += mouseX;
        Quaternion targetRot = Quaternion.Euler(0f, yaw, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSmoothing * Time.deltaTime);
    }

    void FixedUpdate()
    {
        if (!isPlayerControlled) return;

        if (ModeController.Instance != null &&
            ModeController.Instance.currentMode == ControlMode.RTS)
        {
            Vector3 vel = rb.linearVelocity;
            rb.linearVelocity = new Vector3(0f, vel.y, 0f);

            Vector3 pos = rb.position;
            if (Terrain.activeTerrain != null && Terrain.activeTerrain.terrainData != null)
            {
                float terrainY = Terrain.activeTerrain.SampleHeight(pos) + bodyHeightOffset;
                if (Mathf.Abs(pos.y - terrainY) > 0.001f)
                {
                    rb.MovePosition(new Vector3(pos.x, terrainY, pos.z));
                }
            }
            return;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Camera playerCam = ModeController.Instance != null ? ModeController.Instance.playerCamera : Camera.main;
        if (playerCam == null) return;

        Vector3 camForward = playerCam.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = playerCam.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 moveDir = (camForward * v + camRight * h).normalized;

        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f);

        Vector3 targetVel = moveDir * speed;
        Vector3 currentVel = rb.linearVelocity;
        Vector3 desiredVel = new Vector3(targetVel.x, currentVel.y, targetVel.z);

        rb.linearVelocity = Vector3.MoveTowards(currentVel, desiredVel, acceleration * Time.fixedDeltaTime);

        if (moveDir != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(moveDir);
            rb.rotation = Quaternion.Slerp(rb.rotation, lookRot, turnSmoothing * Time.fixedDeltaTime);
        }

        Vector3 posGeneral = rb.position;
        if (Terrain.activeTerrain != null && Terrain.activeTerrain.terrainData != null)
        {
            float terrainY = Terrain.activeTerrain.SampleHeight(posGeneral) + bodyHeightOffset;
            if (Mathf.Abs(posGeneral.y - terrainY) > 0.001f)
            {
                rb.MovePosition(new Vector3(posGeneral.x, terrainY, posGeneral.z));
            }
        }

        // Guard animator calls
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            float horizontalSpeed = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;
            anim.SetFloat("Speed", horizontalSpeed);
        }
    }
}
