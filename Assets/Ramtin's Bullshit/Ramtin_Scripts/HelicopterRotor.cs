using UnityEngine;

public class HelicopterRotor : MonoBehaviour
{
    public float spinSpeed = 1000f; // super fast!

    void Update()
    {
        // Spin around the Y axis (up/down axis in Unity)
        transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime, Space.Self);
    }
}
