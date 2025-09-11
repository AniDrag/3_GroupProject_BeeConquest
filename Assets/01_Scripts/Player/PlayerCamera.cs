using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Transform player;          // player root
    [SerializeField] private Transform orientation;     // used for player movement yaw
    [SerializeField] private Transform camPivot;        // pivot behind player head

    [Header("Camera Settings")]
    [SerializeField] private float sensitivityX = 1f;
    [SerializeField] private float sensitivityY = 1f;
    [SerializeField] private float minPitch = -40f;
    [SerializeField] private float maxPitch = 70f;

    [Header("Distance & Collision")]
    [SerializeField] private float camDistance = 4f;
    [SerializeField] private float camSmooth = 10f;
    [SerializeField] private LayerMask collisionMask;

    [Header("Offset Settings")]
    [SerializeField] private float rightOffset = 0.5f;   // side offset
    [SerializeField] private float upOffset = 0.2f;      // optional vertical offset

    private float yaw;
    private float pitch;
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
        playerInput = player.GetComponent<PlayerInput>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleRotation();
    }

    private void LateUpdate()
    {
        HandleCameraCollision();
    }

    private void HandleRotation()
    {
        Vector2 look = playerInput.actions["Look"].ReadValue<Vector2>();
        yaw += look.x * sensitivityX;
        pitch -= look.y * sensitivityY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Rotate orientation for player movement (yaw only)
        orientation.rotation = Quaternion.Euler(0f, yaw, 0f);

        // Rotate camPivot for camera orbit (yaw + pitch)
        camPivot.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void HandleCameraCollision()
    {
        Vector3 pivotPos = camPivot.position + camPivot.up * upOffset;

        // Sideways offset
        Vector3 offset = camPivot.right * rightOffset;

        // Desired camera position
        Vector3 camDir = camPivot.forward;
        Vector3 desiredPos = pivotPos + offset - camDir * camDistance;

        // SphereCast for walls
        if (Physics.SphereCast(pivotPos + offset, 0.2f, -camDir, out RaycastHit hit, camDistance, collisionMask))
        {
            float dist = Mathf.Clamp(hit.distance, 0.5f, camDistance);
            desiredPos = pivotPos + offset - camDir * dist;
        }

        // Smooth camera movement
        cam.transform.position = Vector3.Lerp(cam.transform.position, desiredPos, Time.deltaTime * camSmooth);

        // Always look at pivot
        cam.transform.rotation = camPivot.rotation;
    }

    /// <summary>
    /// Optional: adjust offsets at runtime
    /// </summary>
    public void SetOffsets(float right, float up)
    {
        rightOffset = right;
        upOffset = up;
    }
}



    
    /*

    [SerializeField] PlayerInput _playerInput;
    [SerializeField] Transform Orientation;

    [SerializeField][Range(0.01f,2f)] float sensitivityVert = 1;
    [SerializeField][Range(0.01f,2f)] float sensitivityHoriz = 1;

    float rotationX;
    float rotationY;
    // Update is called once per frame
    private void Start()
    {
        if (_playerInput == null) Debug.LogWarning("No player Input inside camera");
        if(Orientation == null) Debug.LogWarning("No Orientaion inside camera");
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Update()
    {
        Vector2 lookDirection = _playerInput.actions["Look"].ReadValue<Vector2>();
        if (lookDirection == Vector2.zero) return;

        rotationY += lookDirection.x * sensitivityHoriz;
        rotationX -= lookDirection.y * sensitivityVert;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f); // Prevent camera from flipping

        // Apply vertical rotation to the camera (pitch)
        transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);

        // Apply horizontal rotation to the player orientation (yaw only)
        if (Orientation != null)
            Orientation.rotation = Quaternion.Euler(0f, rotationY, 0f);
    }
}*/
