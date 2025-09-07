using UnityEngine;

public class MoveOnInteract : MonoBehaviour
{
    public enum MoveDirections
    {
        None,
        Left,
        Right,
        Up,
        Down,
        Back,
        Forward,
    }
    public enum MoveType
    {
        Once,
        Constant
    }
    public MoveDirections moveDirection = MoveDirections.None;
    public MoveType moveType = MoveType.Once;
    [SerializeField][Range(1, 200)] int distanceToTravel;
    [SerializeField][Range(0.01f, 50)] float moveSpeed = 5;
    [SerializeField] Transform targetDestination;

    Vector3 _originPos;
    Vector3 _targetPos;
    [SerializeField] bool _enableMovement;
    bool _isAtDestination;
    private void Start()
    {
        _originPos = transform.position;
        _targetPos = CalculateTargetPosition();

        if (moveType == MoveType.Constant)
        {
            _enableMovement = true;
        }

    }
    private void Update()
    {
        if (_enableMovement)
        {
            MoveObject();
        }
    }
    public void OBSTICLE_TriggerMovement()
    {
        if (moveType == MoveType.Once)
        {
            _targetPos = _isAtDestination ? _originPos : CalculateTargetPosition();
            _enableMovement = true;
        }
    }

    private void MoveObject()
    {
        transform.position = Vector3.MoveTowards(transform.position, _targetPos, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, _targetPos) < 0.01f)
        {
            transform.position = _targetPos;
            _enableMovement = (moveType == MoveType.Constant); // keep moving only if constant
            _isAtDestination = !_isAtDestination;

            Debug.Log("Destination reached");
        }
    }

    private Vector3 CalculateTargetPosition()
    {
        if (moveDirection == MoveDirections.None && targetDestination != null)
        {
            return targetDestination.position;
        }

        return _originPos + GetDirectionVector(moveDirection);
    }

    private Vector3 GetDirectionVector(MoveDirections dir)
    {
        return dir switch
        {
            MoveDirections.Left => Vector3.left * distanceToTravel,
            MoveDirections.Right => Vector3.right * distanceToTravel,
            MoveDirections.Up => Vector3.up * distanceToTravel,
            MoveDirections.Down => Vector3.down * distanceToTravel,
            MoveDirections.Forward => Vector3.forward * distanceToTravel,
            MoveDirections.Back => Vector3.back * distanceToTravel,
            _ => Vector3.zero
        };
    }
    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && moveDirection == MoveDirections.None && targetDestination == null)
            return;

        Vector3 originPos = Application.isPlaying ? _originPos : transform.position;
        Vector3 targetPos = (moveDirection == MoveDirections.None && targetDestination != null)
            ? targetDestination.position
            : originPos + GetDirectionVector(moveDirection).normalized * distanceToTravel;

        // Draw the path line
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(originPos, targetPos);

        // Draw final destination
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawCube(targetPos, transform.localScale);

        // Simulate object movement every 0.5s
        Gizmos.color = Color.red;
        float timeStep = 0.5f;
        float currentTime = 0f;
        Vector3 simPos = originPos;

        while (Vector3.Distance(simPos, targetPos) > 0.01f && currentTime < 30f)
        {
            currentTime += timeStep;
            simPos = Vector3.MoveTowards(simPos, targetPos, moveSpeed * timeStep);
            Gizmos.DrawSphere(simPos, 0.05f);
        }
#endif
    }
}
