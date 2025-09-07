using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
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
}
