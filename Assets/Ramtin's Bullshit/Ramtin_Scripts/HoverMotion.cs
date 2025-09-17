using UnityEngine;

public class HoverMotion : MonoBehaviour
{
    public float amplitude = 0.5f; // how high it moves up/down
    public float frequency = 1f;   // how fast it moves up/down

    private Vector3 startPos;

    void Start()
    {
        // Save starting position so it oscillates relative to this point
        startPos = transform.position;
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}
