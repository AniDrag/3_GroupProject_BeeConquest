using UnityEngine;

public class ForwardBackPatrol : MonoBehaviour
{
    [Header("Movement")]
    public float moveDistance = 5f;   // how far forward from the start position
    public float moveSpeed = 2f;      // units per second

    [Header("Rotation")]
    public float rotationSpeed = 180f; // degrees per second
    public bool smoothRotation = true; // smooth rotate vs instant flip

    [Header("Tweak")]
    public float stopThreshold = 0.01f; // how close is "arrived"

    Vector3 startPos;
    Vector3 endPos;
    Vector3 currentTarget;
    Quaternion targetRotation;
    bool movingToEnd = true;
    bool isRotating = false;

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
        // compute the end position using the object's initial facing direction
        endPos = startPos + (transform.rotation * Vector3.forward) * moveDistance;
        currentTarget = endPos;
    }

    void Update()
    {
        // If we have a Rigidbody, do movement in FixedUpdate; otherwise do it here
        if (rb == null) Step(Time.deltaTime);
    }

    void FixedUpdate()
    {
        if (rb != null) Step(Time.fixedDeltaTime);
    }

    void Step(float deltaTime)
    {
        if (moveDistance <= 0f || moveSpeed <= 0f) return;

        if (!isRotating)
        {
            // Move towards current target
            Vector3 pos = (rb != null) ? rb.position : transform.position;
            Vector3 next = Vector3.MoveTowards(pos, currentTarget, moveSpeed * deltaTime);

            if (rb != null) rb.MovePosition(next);
            else transform.position = next;

            if (Vector3.Distance(next, currentTarget) <= stopThreshold)
            {
                // Arrived → prepare rotation of 180° from current rotation
                isRotating = true;
                Quaternion cur = (rb != null) ? rb.rotation : transform.rotation;
                targetRotation = cur * Quaternion.Euler(0f, 180f, 0f);
            }
        }
        else
        {
            // Rotate (smooth or instant)
            if (smoothRotation && rotationSpeed > 0f)
            {
                Quaternion cur = (rb != null) ? rb.rotation : transform.rotation;
                Quaternion nextRot = Quaternion.RotateTowards(cur, targetRotation, rotationSpeed * deltaTime);
                if (rb != null) rb.MoveRotation(nextRot);
                else transform.rotation = nextRot;

                if (Quaternion.Angle(nextRot, targetRotation) <= 0.1f)
                {
                    FinishRotation();
                }
            }
            else
            {
                if (rb != null) rb.MoveRotation(targetRotation);
                else transform.rotation = targetRotation;
                FinishRotation();
            }
        }
    }

    void FinishRotation()
    {
        isRotating = false;
        movingToEnd = !movingToEnd;
        currentTarget = movingToEnd ? endPos : startPos;
    }

    // Quick visual aid in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 s = Application.isPlaying ? startPos : transform.position;
        Vector3 dir = transform.rotation * Vector3.forward;
        Vector3 e = Application.isPlaying ? endPos : s + dir * moveDistance;
        Gizmos.DrawLine(s, e);
        Gizmos.DrawSphere(s, 0.08f);
        Gizmos.DrawSphere(e, 0.08f);
    }
}
