using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.Windows;
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovemant : MonoBehaviour
{

    public Rigidbody _body;
    public PlayerInput input;
    public Transform orientation;

    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float crouchSpeed = 3f;
    public float slideSpeed = 10f;
    public float dashForce = 20f;
    public float jumpForce = 7f;
    public float _currentSpeed;
    public float _acceleration = 0.7f;
    public float _deceleration = 0.2f;

    [Header("=== Ground check ===")]
    [SerializeField, Range(0.01f, 1f)] private float _groundDrag = 1f;
    [SerializeField, Range(20, 80)] private float _maxSlopeAngle = 50f;
    [SerializeField, Range(0.01f, 1f)] private float _groundCheckRadious = 1f;
    public LayerMask groundMask;
    public bool _isGrounded;
    public bool _onSlope;
    private RaycastHit _slopeHit;

    [Header("=== Air check ===")]
    [SerializeField, Range(0.01f, 0.8f)] private float _airDrag = 0.2f;
    public bool _inAir;

    [Header("=== Water check ===")]
    [SerializeField, Range(0.5f,2)] private float _waterDrag = 1.5f;
    [SerializeField, Range(0.01f, 1f)] private float _waterCheckRadious = 1f;
    public LayerMask waterMask;
    public bool _inWater;

    [Header("=========== References ===========")]
    [SerializeField] private Transform waterCheckTransform;
    [SerializeField] private Transform groundCheckTransform;

    #region State machine Vars
    private PlayerStateMachine stateMachine;
    public STATE_Idle idleState;
    public STATE_Walking walkingState;
    public STATE_Running runningState;
    public STATE_Jumping jumpingState;
    public STATE_Crouching crouchingState;
    public STATE_Falling fallingState;
    #endregion



    public enum AnimationTriggers
    {
        Damaged,
        FootstepSound
    }
    private void Awake()
    {
        stateMachine = new PlayerStateMachine();
        idleState = new STATE_Idle(stateMachine,this);
        walkingState = new STATE_Walking(stateMachine, this);
        runningState = new STATE_Running(stateMachine, this);
        jumpingState = new STATE_Jumping(stateMachine, this);
        crouchingState = new STATE_Crouching(stateMachine, this);
        fallingState = new STATE_Falling(stateMachine, this);
        stateMachine.Initialize(idleState);

    }

    private void Update()
    {
        stateMachine.currentState.FrameUpdate();
    }

    private void FixedUpdate()
    {
        GroundCheck();
        stateMachine.currentState.PhysicsUpdate();
    }
    private void AnimationTriggerEvent(AnimationTriggers triggerType)
    {
        stateMachine.currentState.AnimationTriggerEvent(triggerType);
    }
    public void OnLevelUpCall()
    {

    }
    public void ClampVelocity(float speed)
    {
        Vector2 flatVelocity = new Vector2(_body.linearVelocity.x, _body.linearVelocity.z);
        if (flatVelocity.magnitude > speed)
        {
            Vector2 limitedVelocity = flatVelocity.normalized * speed;
            _body.linearVelocity = new Vector3(limitedVelocity.x, _body.linearVelocity.y, limitedVelocity.y);
        }
    }
    
    private void GroundCheck()
    {
        _inWater = Physics.CheckSphere(waterCheckTransform.position, _waterCheckRadious, waterMask);

        // If not in water, check ground
        _isGrounded = !_inWater && Physics.CheckSphere(groundCheckTransform.position, _groundCheckRadious, groundMask);

        // Airborne only if neither grounded nor in water
        _inAir = !_isGrounded && !_inWater;

        // Only check slope when grounded
        _onSlope = _isGrounded && OnSlope();

        HandleGravity();
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(groundCheckTransform.position, Vector3.down, out _slopeHit, 0.1f))
        {
            float angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
            return angle < _maxSlopeAngle && angle != 0;
        }
        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 moveDirection)
    {
        //return Vector3.ProjectOnPlane(_move, _slopeHit.normal).normalized;
        return Vector3.ProjectOnPlane(moveDirection, _slopeHit.normal).normalized;
    }

    void HandleGravity()
    {
        if (_inWater)
        {
            _body.useGravity = false;
            _body.linearDamping = _waterDrag;
        }
        else
        {
            _body.useGravity = true;
            _body.linearDamping = _isGrounded ? _groundDrag : _airDrag;
        }
    }

    public Vector3 GetMoveDirection(Vector2 rawInput)
    {
        //Vector2 rawInput = input.actions["Move"].ReadValue<Vector2>();
        Vector3 moveDirection = orientation.forward * rawInput.y + orientation.right * rawInput.x;
        moveDirection.Normalize();
        if (rawInput.sqrMagnitude < 0.01f) 
            return Vector3.zero;
        else 
            return moveDirection;
    }
}
    /*
    public enum MovemantStates
    {
        Idle,
        Walking,
        Runing,
        Crouching,
        Sliding,
        Dashing,
        Jumping,
        Falling
    }
    public MovemantStates currentMovemantState = MovemantStates.Idle;
    public MovemantStates newMovementState;


    [Header(" ===== References and private vars ===== ")]
    [SerializeField] PlayerCore playerCore; // main player details such as stats and other variables dat determine the player movemant in this case
    [SerializeField] PlayerInput inputs; // all keys and inputs
    [SerializeField] Rigidbody _body;
    [Tooltip("an empty in the middle of a player. used do determine direction of movemant")]
    [SerializeField] Transform Orientation;// an empty in the middle of a player. used do tetermine direction of movemant
    [Tooltip("At this position the ground check sphere will spawn and if it collides with ground it is true else false")]
    [SerializeField] Transform _groundCheck;// an empty in the middle of a player. used do tetermine direction of movemant
    [SerializeField] [Range(0.01f,0.5f)]float groundCheckRadious;


    //Hiden private vars
    private Vector2 _moveDirection;
    private bool _isGrounded;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        InputCalculations();
    }
    private void FixedUpdate()
    {
        PhysicsCalculations();
    }
    /// <summary>
    /// Are we crouching?
    /// Are we running?
    /// Are we Dashing?
    /// Are we walking?
    /// If we are walking and SHIF is pressed we start running. But we must be grounded
    /// if we are walking and controll is pressed we start crouching. No need to be grounded
    /// IF we are Runing and we let go we start walking. But if we can toggle we keep on running.
    /// If we are running but there is no directional input we stop.
    /// if we are running and we press crouch we slide. But if we are in air we just crouch
    /// if we are Sliding but se go below a certian speed we start walking. But if crouch is held down we start crouching.
    /// if we are sliding and we jump we jump in the air.
    /// while exeting jump if shift is held we start running. if Crouch is held we start sliding. if nothing we start walking, if no input we stand still.
    /// if we are crouching and we press shift we start running. or we have a fast crouch.
    /// 
    /// crouch from any state.
    /// Jump if grounded.
    /// Run if grounded.
    /// Walk any state alows it. is the default state if there is directional input.
    /// sliding executed if falling and crouched, or sprinting and crouching is pressed. Must be grounded
    /// so
    ///  If grounded(){
    ///     if(Input isnt 0){   
    ///     
    ///     
    ///     }
    ///     else {
    ///     
    ///     }
    /// }
    /// else{
    ///     if(Input isnt 0){  
    ///     
    ///     }
    ///     else {
    ///     
    ///     }
    /// }
    /// 
    /// 
    /// 
    /// ah this can get soooo flipn long bro
    /// /// </summary>
    void InputCalculations()
    {
        _moveDirection = inputs.actions["Move"].ReadValue<Vector2>();
        _moveDirection.Normalize();
        if(currentMovemantState == newMovementState)
        if (_isGrounded)
        {
            if(_moveDirection != Vector2.zero)
            {
                switch (currentMovemantState)
                {
                    case MovemantStates.Walking:
                        if (Input.GetKeyDown(KeyCode.Space))
                        {
                            //jump budy jump
                            // ig call the switchig movemat state something and lat it handle the physics input?
                            // more like set new state to this and then send it tho physics? so we get transitions and shit down. soo lets say curerent state is falling then we pass that and ig sliding so we will get the fall slide transition
                        }
                        else if (Input.GetKeyDown(KeyCode.LeftShift))
                        {
                                //Sprint boy sprint
                        }
                        else if (Input.GetKeyDown(KeyCode.LeftControl))
                        {
                            //Crouch boy crouch
                        }
                            break;
                    case MovemantStates.Jumping:
                        break;
                }
            }
            else
            {

            }

        }
        else
        {
            if (_moveDirection != Vector2.zero)
            {

            }
            else
            {

            }
        }

    }

    void PhysicsCalculations()
    {
        
    }

   
}
*/