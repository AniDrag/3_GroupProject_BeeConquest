using TMPro;
using UnityEngine;

public class FloatingNumbers : MonoBehaviour
{
    public float floatSpeed = 1f;         // how fast it rises
    public float fadeDuration = 1f;       // how long until it disappears

    public TMP_Text textMesh;
    private Color initialColor;
    private float elapsedTime;

    public void Initialize(long value)
    {        
        textMesh.text = value.ToString();
        initialColor = textMesh.color;
        elapsedTime = 0f;
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        // float upwards
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // face camera
        if (Camera.main != null)
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);

        // fade out
        if (textMesh != null)
        {
            float alpha = Mathf.Lerp(initialColor.a, 0f, elapsedTime / fadeDuration);
            textMesh.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
        }

        // destroy after fade
        if (elapsedTime >= fadeDuration)
        {
            Destroy(gameObject);
        }
    }
}

