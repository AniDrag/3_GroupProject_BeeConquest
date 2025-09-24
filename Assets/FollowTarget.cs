using UnityEngine;
using UnityEngine.InputSystem;

public class FollowTarget : MonoBehaviour
{
    [SerializeField] private Transform followTarget;
    [SerializeField] private float followSensitivity = 1;
    [SerializeField] public Transform OrientationTarget { get; private set; }
    [SerializeField] public PlayerInput inputs { get; private set; }
    public void SetUo(Transform followPos, Transform orientationTransform, PlayerInput playerInput)
    {
        followTarget = followPos;
        OrientationTarget = orientationTransform;
        inputs = playerInput;
    }

    // Update is called once per frame
    void Update()
    {
        
        transform.position = Vector3.Lerp(transform.position, followTarget.position, followSensitivity * Time.deltaTime);
    }
}
