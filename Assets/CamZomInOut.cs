using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CamZoomInOut : MonoBehaviour
{
    public CinemachineOrbitalFollow camFollow;
    public PlayerInput inputs;

    [SerializeField] float scrollSpeed = 3f;

    // Center orbit limits
    [SerializeField] float centerMinRadius = 0.73f;
    [SerializeField] float centerMaxRadius = 5.5f;
    [SerializeField] float centerMinHeight = 1f;
    [SerializeField] float centerMaxHeight = 10f;


    // Top orbit limits
    [SerializeField] float topMinHeight = 1f;
    [SerializeField] float topMaxHeight = 10f;
    [SerializeField] float topMinRadius = 0.73f;
    [SerializeField] float topMaxRadius = 5.5f;

    // Bottom orbit limits
    [SerializeField] float bottomMinHeight = 1f;
    [SerializeField] float bottomMaxHeight = 10f;
    [SerializeField] float bottomMinRadius = 0.73f;
    [SerializeField] float bottomMaxRadius = 5.5f;

    void Update()
    {
        float scroll = inputs.actions["ScrollWheel"].ReadValue<Vector2>().y;

        // Center orbit zoom
        camFollow.Orbits.Center.Radius -= scroll * scrollSpeed * Time.deltaTime;
        camFollow.Orbits.Center.Radius = Mathf.Clamp(
            camFollow.Orbits.Center.Radius,
            centerMinRadius,
            centerMaxRadius
        );
        camFollow.Orbits.Center.Height -= scroll * scrollSpeed * Time.deltaTime;
        camFollow.Orbits.Center.Height = Mathf.Clamp(
            camFollow.Orbits.Center.Height,
            centerMinHeight ,
            centerMaxHeight
        );

        // Top orbit zoom
        camFollow.Orbits.Top.Height -= scroll * scrollSpeed * Time.deltaTime;
        camFollow.Orbits.Top.Height = Mathf.Clamp(
            camFollow.Orbits.Top.Height,
            topMinHeight,
            topMaxHeight
        );
        camFollow.Orbits.Top.Radius -= scroll * scrollSpeed * Time.deltaTime;
        camFollow.Orbits.Top.Radius = Mathf.Clamp(
            camFollow.Orbits.Top.Radius,
            topMinRadius,
            topMaxRadius
        );

        // Bottom orbit zoom
        camFollow.Orbits.Bottom.Height -= scroll * scrollSpeed * Time.deltaTime;
        camFollow.Orbits.Bottom.Height = Mathf.Clamp(
            camFollow.Orbits.Bottom.Height,
            bottomMinHeight,
            bottomMaxHeight
        );
        camFollow.Orbits.Bottom.Radius -= scroll * scrollSpeed * Time.deltaTime;
        camFollow.Orbits.Bottom.Radius = Mathf.Clamp(
            camFollow.Orbits.Bottom.Radius,
            bottomMinRadius,
            bottomMaxRadius
        );
    }
}
