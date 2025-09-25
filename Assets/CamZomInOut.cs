using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CamZomInOut : MonoBehaviour
{
    public CinemachineOrbitalFollow camFollow;
    public PlayerInput inputs;
    [SerializeField] float scrollSpeed;
    [SerializeField] float minRange = 4;
    [SerializeField] float maxRange = 10;

    [SerializeField] float topMinHeight = 4;
    [SerializeField] float topMaxHeight = 5.5f;


    // Update is called once per frame
    void Update()
    {
        float scroll = inputs.actions["ScrollWheel"].ReadValue<Vector2>().y;

        camFollow.Orbits.Center.Radius += scroll * scrollSpeed * Time.deltaTime;
        camFollow.Orbits.Top.Height += scroll * scrollSpeed * Time.deltaTime;

        // assign clamped value back
        camFollow.Orbits.Center.Radius = Mathf.Clamp(
            camFollow.Orbits.Center.Radius,
            minRange,
            maxRange
        );
        camFollow.Orbits.Top.Height = Mathf.Clamp(
            camFollow.Orbits.Top.Height,
            topMinHeight,
            topMaxHeight
        );
    }
}
