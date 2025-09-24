using Unity.Cinemachine;
using UnityEngine;

public class CoppyRotationOfObject : MonoBehaviour
{
    [SerializeField] private CinemachineOrbitalFollow cam;
    void Update()
    {
        // Get the camera's yaw angle from the Cinemachine orbital follow
        float yaw = cam.HorizontalAxis.Value;

        // Apply rotation only on the Y axis
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }
}
