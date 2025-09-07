using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovemant : MonoBehaviour
{
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

    //[Header("=== Ground check ===")]
    //[SerializeField, Range(0.01f, 1f)] private float _groundDrag = 1f;
    //[SerializeField, Range(20, 80)] private float _maxSlopeAngle = 50f;
    //[SerializeField, Range(0.01f, 1f)] private float _groundCheckRadious = 1f;
    //public LayerMask groundMask;
    //public bool _isGrounded;
    //public bool _onSlope;
    //private RaycastHit _slopeHit;

   
    public enum AnimationTriggers
    {
        Damaged,
        FootstepSound,
        Walking,
        Idle
    }
    private void Awake()
    {
       

    }

    private void Update()
    {
        
    }
}