using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerInput playerInput;
    [SerializeField] Transform player;          // the actual player root
    [SerializeField] Transform orientation;     // orientation used by MovementCore
    [SerializeField] Transform camPivot;        // empty GameObject behind head

    [Header("Camera Settings")]
    [SerializeField] float sensitivityX = 1f;
    [SerializeField] float sensitivityY = 1f;
    [SerializeField] float minY = -40f;
    [SerializeField] float maxY = 70f;

    [Header("Distance Settings")]
    [SerializeField] float camDistance = 4f;         // desired distance
    [SerializeField] float camSmooth = 10f;          // smoothing
    [SerializeField] LayerMask collisionMask;        // walls, environment

    float yaw;
    float pitch;
    Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void LateUpdate()
    {
        HandleRotation();
        HandleCameraCollision();
    }

    void HandleRotation()
    {
        Vector2 look = playerInput.actions["Look"].ReadValue<Vector2>();
        yaw += look.x * sensitivityX;
        pitch -= look.y * sensitivityY;
        pitch = Mathf.Clamp(pitch, minY, maxY);

        // Rotate pivot (yaw + pitch)
        camPivot.rotation = Quaternion.Euler(pitch, yaw, 0f);

        // Only rotate orientation on yaw → for movement
        orientation.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    void HandleCameraCollision()
    {
        Vector3 pivotPos = camPivot.position;
        Vector3 desiredPos = pivotPos - camPivot.forward * camDistance;

        if (Physics.SphereCast(pivotPos, 0.2f, -camPivot.forward,
            out RaycastHit hit, camDistance, collisionMask))
        {
            // Pull camera closer if wall blocks it
            float dist = Mathf.Clamp(hit.distance, 0.5f, camDistance);
            Vector3 newPos = pivotPos - camPivot.forward * dist;
            cam.transform.position = Vector3.Lerp(cam.transform.position, newPos, Time.deltaTime * camSmooth);
        }
        else
        {
            // No wall, move to desired
            cam.transform.position = Vector3.Lerp(cam.transform.position, desiredPos, Time.deltaTime * camSmooth);
        }

        // Always look at pivot
        cam.transform.rotation = camPivot.rotation;
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
