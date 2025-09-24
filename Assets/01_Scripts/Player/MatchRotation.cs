using UnityEngine;

public class MatchRotation : MonoBehaviour
{
    [Header("Reference Transform to Follow")]
    [SerializeField] private Transform target;

    [Header("Options")]
    public bool matchX = false;
    public bool matchY = true;
    public bool matchZ = false;
    public bool smooth = true;
    public float smoothSpeed = 10f;

    private void LateUpdate()
    {
        if (target == null) return;

        Quaternion targetRot = target.rotation;

        // Optionally zero out axes
        Vector3 euler = targetRot.eulerAngles;
        if (!matchX) euler.x = transform.eulerAngles.x;
        if (!matchY) euler.y = transform.eulerAngles.y;
        if (!matchZ) euler.z = transform.eulerAngles.z;

        Quaternion finalRot = Quaternion.Euler(euler);

        if (smooth)
            transform.rotation = Quaternion.Lerp(transform.rotation, finalRot, Time.deltaTime * smoothSpeed);
        else
            transform.rotation = finalRot;
    }
}
