using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 11.5f;
    public float slideSpeedMultiplier = 2.2f;

    [Header("Crouch & Slide")]
    public float normalHeight = 1.8f;
    public float crouchHeight = 0.9f;
    public float slideForce = 22f;
    public float slideCooldown = 0.6f;
    
    [Header("Dashing")]
    public float dashForce = 25f;
    public float dashUpwardForce = 4f;
    public float dashDuration = 0.25f;
    public float dashCooldown = 1.2f;

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
    private bool isDashing = false;
    private float dashTimeLeft = 0f;
    private float dashCooldownLeft = 0f;
    private bool isSliding = false;
    private bool isCrouching = false;
    private float slideCooldownLeft = 0f;
    private float originalHeight;
    private Vector3 originalCenter;
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        originalHeight = characterController.height;
        originalCenter = characterController.center;
    }

    void Update()
    {
        HandleCrouchAndSlide();
        HandleRotation();
        HandleDashing();
        HandleMovement();
        HandleJumpingAndGravity();

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void HandleCrouchAndSlide()
    {
        if (slideCooldownLeft > 0)
            slideCooldownLeft -= Time.deltaTime;

        bool wantsToCrouch = Input.GetKey(KeyCode.LeftControl);

        if (wantsToCrouch && characterController.isGrounded && !isSliding && 
            slideCooldownLeft <= 0 && moveDirection.magnitude > 0.5f)
        {
            StartSlide();
        }

        if (!wantsToCrouch && isSliding)
        {
            EndSlide();
        }

        if (isSliding && moveDirection.magnitude < 0.3f)
        {
            EndSlide();
        }
    }

    private void StartSlide()
    {
        isSliding = true;
        isCrouching = true;

        // Lower player
        characterController.height = crouchHeight;
        characterController.center = new Vector3(0, crouchHeight / 2, 0);

        moveDirection.y = -3f;
    }

    private void EndSlide()
    {
        isSliding = false;
        isCrouching = false;

        // Return to normal height
        characterController.height = normalHeight;
        characterController.center = originalCenter;

        slideCooldownLeft = slideCooldown;
    }

    private void StopCrouch()
    {
        if (!isSliding)
        {
            characterController.height = normalHeight;
            characterController.center = originalCenter;
        }
    }

    private void HandleMovement()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float curSpeedX = canMove ? walkSpeed * Input.GetAxisRaw("Vertical") : 0;
        float curSpeedZ = canMove ? walkSpeed * Input.GetAxisRaw("Horizontal") : 0;

        Vector3 horizontalMove = (forward * curSpeedX) + (right * curSpeedZ);

        if (isSliding)
        {
            moveDirection.x = horizontalMove.x * slideSpeedMultiplier;
            moveDirection.z = horizontalMove.z * slideSpeedMultiplier;

            moveDirection += transform.forward * slideForce * Time.deltaTime;
        }
        else
        {
            moveDirection.x = horizontalMove.x;
            moveDirection.z = horizontalMove.z;
        }

        if (isDashing)
        {
            Vector3 dashVelocity = transform.forward * dashForce + transform.up * dashUpwardForce;
            moveDirection.x += dashVelocity.x;
            moveDirection.z += dashVelocity.z;
            moveDirection.y += dashVelocity.y;
        }
    }

    private void HandleJumpingAndGravity()
    {
        if (Input.GetButtonDown("Jump") && characterController.isGrounded && canMove && !isSliding)
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
        if (dashCooldownLeft > 0) dashCooldownLeft -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && dashCooldownLeft <= 0 && canMove && !isSliding)
        {
            StartDash();
        }

        if (isDashing)
        {
            dashTimeLeft -= Time.deltaTime;
            if (dashTimeLeft <= 0) EndDash();
        }
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimeLeft = dashDuration;
        dashCooldownLeft = dashCooldown;
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