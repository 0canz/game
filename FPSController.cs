using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 11.5f;

    [Header("Dashing")]
    public float dashForce = 25f;           // Main forward dash strength
    public float dashUpwardForce = 4f;      // How much upward boost you want
    public float dashDuration = 0.25f;      // How long the dash "boost" lasts
    public float dashCooldown = 1.2f;       // Cooldown after dashing

    [Header("Jump & Gravity")]
    public float jumpPower = 9f;
    public float gravityForce = 10f;

    [Header("Camera Look")]
    public Camera playerCamera;
    public float lookSpeed = 3f;
    public float lookXLimit = 55f;

    // Private variables
    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0f;

    private bool canMove = true;

    // Dash variables
    private bool isDashing = false;
    private float dashTimeLeft = 0f;
    private float dashCooldownLeft = 0f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleRotation();
        HandleDashing();
        HandleMovement();
        HandleJumpingAndGravity();

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void HandleMovement()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float curSpeedX = canMove ? walkSpeed * Input.GetAxisRaw("Vertical") : 0;
        float curSpeedZ = canMove ? walkSpeed * Input.GetAxisRaw("Horizontal") : 0;

        // Normal movement
        moveDirection.x = (forward.x * curSpeedX) + (right.x * curSpeedZ);
        moveDirection.z = (forward.z * curSpeedX) + (right.z * curSpeedZ);

        // During dash apply extra velocity
        if (isDashing)
        {
            Vector3 dashVelocity = transform.forward * dashForce + transform.up * dashUpwardForce;
            moveDirection.x += dashVelocity.x;
            moveDirection.z += dashVelocity.z;
            moveDirection.y += dashVelocity.y;     // Apply upward force
        }
    }

    private void HandleJumpingAndGravity()
    {
        if (Input.GetButtonDown("Jump") && characterController.isGrounded && canMove)
        {
            moveDirection.y = jumpPower;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravityForce * Time.deltaTime;
        }
    }

    private void HandleDashing()
    {
        // Cooldown countdown
        if (dashCooldownLeft > 0)
            dashCooldownLeft -= Time.deltaTime;

        // Trigger Dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && dashCooldownLeft <= 0 && canMove)
        {
            StartDash();
        }

        // Countdown dash duration
        if (isDashing)
        {
            dashTimeLeft -= Time.deltaTime;

            if (dashTimeLeft <= 0)
            {
                EndDash();
            }
        }
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimeLeft = dashDuration;
        dashCooldownLeft = dashCooldown;

        // Optional: Small initial upward boost for better feel
        // moveDirection.y += dashUpwardForce * 0.5f;
    }

    private void EndDash()
    {
        isDashing = false;
    }

    private void HandleRotation()
    {
        if (!canMove) return;

        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
    }
}