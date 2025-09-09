using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(CharacterController))]
//[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovemant : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public PlayerInput input;
    private CharacterController controller;

    [Header("Speeds")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float crouchSpeed = 3f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    [Header("Acceleration")]
    public float acceleration = 20f;
    public float airControl = 0.5f;
    public float jumpResetTime = 0.3f;

    [Header("Crouch")]
    public float crouchHeight = 1f;
    private float originalHeight;


    public enum MovementState { Idle, Walking, Sprinting, Crouching, Jumping, Falling }

    [Header("States")]
    public MovementState currentState;
    private Vector2 moveInput;
    private Vector3 moveDir;
    private Vector3 velocity;
    [SerializeField] private bool isSprinting;
    [SerializeField] private bool isCrouching;
    [SerializeField] private bool jumped;
    //[SerializeField] private bool falling;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction crouchAction;


    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        moveAction = input.actions["Move"];
        jumpAction = input.actions["Jump"];
        sprintAction = input.actions["Sprint"];
        crouchAction = input.actions["Crouch"];

        originalHeight = controller.height;
    }
    private void OnEnable()
    {
        jumpAction.performed += _ => Jump();
        crouchAction.performed += _ => ToggleCrouch();
    }

    private void OnDisable()
    {
        jumpAction.performed -= _ => Jump();
        crouchAction.performed -= _ => ToggleCrouch();
    }
    private void Update()
    {
        ReadInputs();
        StateHandler();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }

    // -----------------------------
    // INPUT
    // -----------------------------
    void ReadInputs()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        moveDir = orientation.forward * moveInput.y + orientation.right * moveInput.x;
        moveDir.Normalize();

        isSprinting = sprintAction.IsPressed();
    }

    // -----------------------------
    // STATE HANDLER
    // -----------------------------
    void StateHandler()
    {
        if (!controller.isGrounded)
        {
            if (velocity.y > 0f && jumped)
                currentState = MovementState.Jumping;  // Moving upwards
            else
                currentState = MovementState.Falling;  // Moving downwards
            return;
        }

        if (isCrouching)
        {
            currentState = MovementState.Crouching;
        }
        else if (moveDir.magnitude > 0.01f)
        {
            currentState = isSprinting ? MovementState.Sprinting : MovementState.Walking;
        }
        else
        {
            currentState = MovementState.Idle;
        }
    }
    // -----------------------------
    // MOVEMENT
    // -----------------------------
    void ApplyMovement()
    { // Horizontal movement
        Vector3 horizontalMove = orientation.forward * moveInput.y + orientation.right * moveInput.x;
        horizontalMove.Normalize();

        float targetSpeed = walkSpeed;
        if (isSprinting) targetSpeed = sprintSpeed;
        if (isCrouching) targetSpeed = crouchSpeed;

        float accel = controller.isGrounded ? acceleration : acceleration * airControl;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, horizontalMove * targetSpeed, accel * Time.deltaTime);

        // Gravity
        if (controller.isGrounded)
        {
            if (velocity.y < 0)
                velocity.y = -0.1f; // small negative to stick to ground
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        // Apply horizontal velocity
        velocity.x = horizontalVelocity.x;
        velocity.z = horizontalVelocity.z;

        // Move character
        controller.Move(velocity * Time.deltaTime);
    }

    // -----------------------------
    // JUMPING
    // -----------------------------
    private void Jump()
    {
        if (!controller.isGrounded) return;
        velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        jumped = true;
    }

    private void ToggleCrouch()
    {
        if (isSprinting) isSprinting = false;

        isCrouching = !isCrouching;
        controller.height = isCrouching ? crouchHeight : originalHeight;
    }
    private void OnDrawGizmosSelected()
    {
        if (orientation != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, orientation.forward * 2f);
        }
    }
}

/*
[Header("References")]
public Rigidbody rb;
public Transform orientation;
public Transform groundCheckPoint;
public PlayerInput input; // link your PlayerInput component

[Header("Speeds")]
public float walkSpeed = 5f;
public float sprintSpeed = 8f;
public float crouchSpeed = 3f;
public float jumpForce = 5f;
public float clampVelSpeed = 2f;

[Header("Acceleration")]
public float walkAccel = 10f;
public float sprintAccel = 12f;
public float crouchAccel = 8f;
public float deceleration = 0.2f;
public float jumpResetTime = 0.3f;

[Header("Ground & Slopes")]
public float groundCheckDist = 0.4f;
public LayerMask groundMask;
public float maxSlopeAngle = 45f;
private RaycastHit slopeHit;

[Header("States")]
public MovementState currentState;
private Vector2 moveInput;
private Vector3 moveDir;
[SerializeField] private bool isGrounded;
[SerializeField] private bool isSprinting;
[SerializeField] private bool isCrouching;
[SerializeField] private bool jumped;
[SerializeField] private bool falling;

private InputAction moveAction;
private InputAction jumpAction;
private InputAction sprintAction;
private InputAction crouchAction;

public enum MovementState { Idle, Walking, Sprinting, Crouching, Jumping, Falling }

private void Awake()
{
    rb = GetComponent<Rigidbody>();
    rb.freezeRotation = true;

    // Get input actions from your PlayerInput
    moveAction = input.actions["Move"];
    jumpAction = input.actions["Jump"];
    sprintAction = input.actions["Sprint"];
    crouchAction = input.actions["Crouch"];
}
private void OnEnable()
{
    jumpAction.performed += _ => Jump();
    crouchAction.performed += _ => ToggleCrouch();
    sprintAction.performed += _ => HandleSprintInput();
}

private void OnDisable()
{
    jumpAction.performed -= _ => Jump();
    crouchAction.performed -= _ => ToggleCrouch();
    sprintAction.performed -= _ => HandleSprintInput();
}
private void Update()
{
    ReadInputs();
    GroundCheck();
    StateHandler();
}

private void FixedUpdate()
{
    ApplyMovement();
}

// -----------------------------
// INPUT
// -----------------------------
void ReadInputs()
{
    moveInput = moveAction.ReadValue<Vector2>();
    moveDir = orientation.forward * moveInput.y + orientation.right * moveInput.x;
    moveDir.Normalize();

    isSprinting = sprintAction.IsPressed();
}

// -----------------------------
// STATE HANDLER
// -----------------------------
void StateHandler()
{
    if (!isGrounded)
    {
        currentState = !falling ? MovementState.Jumping : MovementState.Falling;
        return;
    }

    if (isCrouching)
    {
        currentState = MovementState.Crouching;
    }
    else if (moveDir.magnitude > 0.01f)
    {
        currentState = isSprinting ? MovementState.Sprinting : MovementState.Walking;
    }
    else
    {
        currentState = MovementState.Idle;
    }
}

// -----------------------------
// MOVEMENT
// -----------------------------
void ApplyMovement()
{
    float targetSpeed = 0f;
    float accel = 0f;

    switch (currentState)
    {
        case MovementState.Walking:
            targetSpeed = walkSpeed;
            accel = walkAccel;
            break;
        case MovementState.Sprinting:
            //HandleSprintInput();
            targetSpeed = sprintSpeed;
            accel = sprintAccel;
            break;
        case MovementState.Crouching:
            targetSpeed = crouchSpeed;
            accel = crouchAccel;
            break;
        case MovementState.Jumping:
            targetSpeed = walkSpeed/2;
            accel = walkAccel * .2f;
            break;
        case MovementState.Falling:
            targetSpeed = walkSpeed/2;
            accel = walkAccel*.2f;
            break;
    }

    if (moveDir.magnitude > 0.01f)
    {
        Vector3 direction = OnSlope() ? GetSlopeMoveDir(moveDir) : moveDir;
        rb.AddForce(direction * targetSpeed * accel, ForceMode.Force);
        ClampVelocity(targetSpeed);
    }
    else if (isGrounded)
    {
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, deceleration);
    }

    // Slope sliding
    if (OnSlope() && slopeHit.normal.y < 1f && Vector3.Angle(Vector3.up, slopeHit.normal) > maxSlopeAngle)
    {
        rb.AddForce(Vector3.down * 20f, ForceMode.Force); // slide down
    }
}
void HandleSprintInput()
{
    if (sprintAction.IsPressed())
    {
        if (isCrouching)
        {
            isCrouching = false;
            // Reset collider if you shrunk it
        }
        isSprinting = true;
    }
    else
    {
        isSprinting = false;
    }
}

void ClampVelocity(float maxSpeed)
{
    Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

    if (flatVel.magnitude > maxSpeed)
    {
        // Smoothly reduce velocity toward maxSpeed
        Vector3 limited = flatVel.normalized * maxSpeed;

        if (isGrounded)
        {
            // Grounded: snap immediately
            flatVel = limited;
        }
        else
        {
            // Air: smooth
            flatVel = Vector3.Lerp(flatVel, limited, Time.fixedDeltaTime * clampVelSpeed); // 5 = smoothing speed
        }

        rb.linearVelocity = new Vector3(flatVel.x, rb.linearVelocity.y, flatVel.z);
    }
}

// -----------------------------
// JUMP & CROUCH
// -----------------------------
void Jump()
{
    if (!isGrounded) return;

    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
    rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    currentState = MovementState.Jumping;
    jumped = true;
}

void ToggleCrouch()
{
    if (isSprinting)
    {
        isSprinting = false;
    }
    isCrouching = !isCrouching;

    // Optional: shrink collider here
}

// -----------------------------
// GROUND & SLOPE CHECK
// -----------------------------
void GroundCheck()
{
    isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down,
        out slopeHit, groundCheckDist, groundMask);
    if (!isGrounded)
    {
        falling = rb.linearVelocity.y < 0f;
    }

    if (isGrounded && !jumped) Invoke(nameof(ResetJump), jumpResetTime);
}

bool OnSlope()
{
    if (!isGrounded) return false;
    float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
    return angle > 0f && angle <= maxSlopeAngle;
}

Vector3 GetSlopeMoveDir(Vector3 dir)
{
    return Vector3.ProjectOnPlane(dir, slopeHit.normal).normalized;
}
void ResetJump()
{
    jumped = false;
}
// -----------------------------
// GIZMOS
// -----------------------------
private void OnDrawGizmosSelected()
{
    if (groundCheckPoint != null)
    {
        // Ground check ray
        Gizmos.color = Color.yellow;
        Vector3 origin = groundCheckPoint.position;
        Vector3 end = origin + Vector3.down * groundCheckDist;
        Gizmos.DrawLine(origin, end);
        Gizmos.DrawWireSphere(end, 0.05f);
    }

    if (slopeHit.collider != null)
    {
        // Slope normal
        Gizmos.color = Color.red;
        Gizmos.DrawRay(slopeHit.point, slopeHit.normal);
    }

    if (orientation != null)
    {
        // Move direction
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, orientation.forward);
    }

    if (rb != null)
    {
        // Velocity vector
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, rb.linearVelocity);
    }

    // Capsule radius at ground check
    CapsuleCollider col = GetComponent<CapsuleCollider>();
    if (col != null && groundCheckPoint != null)
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(groundCheckPoint.position, col.radius);
    }
}
}*/