using UnityEngine;
using UnityEngine.InputSystem;

public class BeePreviewRotator : MonoBehaviour
{
    [Header("Object Reference")]
    [Tooltip("The 3D object to rotate inside the UI preview")]
    public Transform objectToRotate;

    [Header("Camera / Zoom Settings")]
    [Tooltip("Camera or pivot that moves closer/farther for zooming")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 5f;
    [SerializeField] private float zoomSpeed = 2f;

    private float targetDistance;

    [Header("Rotation Settings")]
    [SerializeField] private float sensitivityX = 1f;
    [SerializeField] private float sensitivityY = 1f;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 70f;

    [Header("Input Settings")]
    public PlayerInput playerInput;
    public string lookActionName = "Look";
    public string clickActionName = "RightClick";
    public string zoomActionName = "Zoom"; // scroll wheel or axis

    [Header("Inversion Options")]
    [SerializeField] private bool invertX = false;
    [SerializeField] private bool invertY = false;

    [Header("UI Raycast")]
   // [SerializeField] private Camera uiCamera; // camera that renders the RawImage
    [SerializeField] private LayerMask interactLayer; // layer of the RawImage plane or object

    private InputAction lookAction;
    private InputAction clickAction;
    private InputAction zoomAction;
    private bool isDragging = false;

    private float yaw;
    private float pitch;

    private void OnEnable()
    {
        if (playerInput == null)
        {
            Debug.LogError("PlayerInput reference is missing!");
            return;
        }

        lookAction = playerInput.actions[lookActionName];
        clickAction = playerInput.actions[clickActionName];
        zoomAction = playerInput.actions[zoomActionName];

        clickAction.performed += OnClick;
        clickAction.canceled += OnRelease;

        targetDistance = cameraTransform.localPosition.z;
    }

    private void OnDisable()
    {
        if (clickAction != null)
        {
            clickAction.performed -= OnClick;
            clickAction.canceled -= OnRelease;
        }
    }

    private void Update()
    {
        if (objectToRotate == null || cameraTransform == null) return;

        HandleRotation();
        //HandleZoom();
    }

    private void HandleRotation()
    {
        if (!isDragging) return;

        Vector2 delta = lookAction.ReadValue<Vector2>();
        float horizontal = delta.x * sensitivityX * (invertX ? -1f : 1f);
        float vertical = delta.y * sensitivityY * (invertY ? -1f : 1f);

        // Horizontal rotation around world up
        yaw += horizontal;
        objectToRotate.Rotate(Vector3.up, horizontal, Space.World);

        // Vertical rotation (local)
        pitch = Mathf.Clamp(pitch + vertical, minPitch, maxPitch);
        objectToRotate.localRotation = Quaternion.Euler(pitch, objectToRotate.localEulerAngles.y, 0f);
    }

    private void HandleZoom()
    {
        float zoomInput = zoomAction.ReadValue<float>();
        if (Mathf.Approximately(zoomInput, 0f)) return;

        targetDistance = Mathf.Clamp(targetDistance + -zoomInput * zoomSpeed, -maxDistance, -minDistance);
        Vector3 localPos = cameraTransform.localPosition;
        localPos.z = Mathf.Lerp(localPos.z, targetDistance, Time.deltaTime * zoomSpeed * 5f);
        cameraTransform.localPosition = localPos;
    }

    private void OnClick(InputAction.CallbackContext ctx)
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactLayer))
        {
            if (hit.collider != null)
            {
                isDragging = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    private void OnRelease(InputAction.CallbackContext ctx)
    {
        isDragging = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
