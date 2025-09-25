using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CamZoomInOut : MonoBehaviour
{

    [Header("---------- Refrences ----------")]
    public CinemachineOrbitalFollow camFollow;
    public PlayerInput inputs;

    [Header("---------- ScrollSettings ----------")]
    [SerializeField] float scrollSpeed = 40f;

    [Header("---------- Center orbit limits ----------")]
    // Center orbit limits
    [SerializeField] float centerMinRadius = 2;
    [SerializeField] float centerMaxRadius = 10f;
    [SerializeField] float centerMinHeight = 1f;
    [SerializeField] float centerMaxHeight = 5f;

    [Header("---------- Top orbit limits ----------")]
    // Top orbit limits
    [SerializeField] float topMinHeight = 2f;
    [SerializeField] float topMaxHeight = 10f;
    [SerializeField] float topMinRadius = 0.73f;
    [SerializeField] float topMaxRadius = 3;
    [Header("---------- Bottom orbit limits ----------")]
    // Bottom orbit limits
    [SerializeField] float bottomMinHeight = -1f;
    [SerializeField] float bottomMaxHeight = -.4f;
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
            centerMinHeight,
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
