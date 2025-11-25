using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonMovement : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    private CharacterController controller;

    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float rotationSpeed = 10f;
    public float gravity = -9.81f;

    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        bool isMoving = direction.magnitude >= 0.1f;
        animator.SetBool("isMoving", isMoving);

        bool isRunning = isMoving && Input.GetKey(KeyCode.LeftShift);
        animator.SetBool("isRunning", isRunning);


        float targetSpeed = isRunning ? runSpeed : walkSpeed;

        if (isMoving)
        {
   
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Move in the facing direction
            Vector3 moveDir = transform.forward;
            controller.Move(moveDir * targetSpeed * Time.deltaTime);
        }

    
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f; 

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
