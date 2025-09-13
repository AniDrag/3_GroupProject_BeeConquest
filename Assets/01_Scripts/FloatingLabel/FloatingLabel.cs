// client
using UnityEngine;
using TMPro;
using System.Collections;

[DisallowMultipleComponent]
public class FloatingLabel : MonoBehaviour
{
    public TMP_Text tmpText;                 // assign in inspector or auto-find
    [Header("Timings")]
    public float visibleDuration = 3f;
    public float shrinkDuration = 0.5f;
    [Header("Motion")]
    public Vector3 risePerSecond = new Vector3(0f, 0.25f, 0f);

    Transform mainCam;
    Coroutine running;

    void Awake()
    {
        // try to auto-assign TMP if not assigned in inspector
        if (tmpText == null)
        {
            tmpText = GetComponentInChildren<TMP_Text>(true);
            if (tmpText == null)
            {
                Debug.LogError($"[FloatingLabel] tmpText not assigned and no TMP_Text found on '{gameObject.name}'. Assign a TextMeshPro component in the prefab.", this);
            }
            else
            {
                // optional: cache name to show in logs
                Debug.Log($"[FloatingLabel] auto-assigned tmpText for '{gameObject.name}'");
            }
        }

        mainCam = Camera.main ? Camera.main.transform : null;
        if (mainCam == null)
        {
            Debug.LogWarning("[FloatingLabel] Camera.main is null. Billboarding will be disabled until a main camera exists.");
        }
    }

    void OnValidate()
    {
        // allow editor-time auto-fix when you assign the prefab in inspector
        if (tmpText == null)
        {
            tmpText = GetComponentInChildren<TMP_Text>(true);
        }
    }

    /// <summary>
    /// Show the label. If tmpText is missing, this will safely return it to pool instead of throwing.
    /// </summary>
    public void Show(string text, Color color, Vector3 worldPos)
    {
        if (tmpText == null)
        {
            Debug.LogError($"[FloatingLabel.Show] tmpText is null on '{gameObject.name}'. Returning to pool to avoid crash.", this);
            // Return to pool if available to avoid losing the object
            if (FloatingLabelPool.Instance != null) FloatingLabelPool.Instance.Return(this);
            else gameObject.SetActive(false);
            return;
        }

        transform.position = worldPos;
        tmpText.text = text;
        tmpText.color = color;
        transform.localScale = Vector3.one;

        gameObject.SetActive(true);
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(AnimateAndReturn());
    }

    IEnumerator AnimateAndReturn()
    {
        float t = 0f;
        Color startColor = tmpText.color;

        // visible phase
        while (t < visibleDuration)
        {
            if (mainCam != null) transform.forward = mainCam.forward;
            transform.position += risePerSecond * Time.deltaTime;
            t += Time.deltaTime;
            yield return null;
        }

        // shrink + fade
        float s = 0f;
        while (s < shrinkDuration)
        {
            float p = s / shrinkDuration;
            float scale = Mathf.Lerp(1f, 0f, p);
            transform.localScale = Vector3.one * scale;

            float alpha = Mathf.Lerp(1f, 0f, p);
            tmpText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            if (mainCam != null) transform.forward = mainCam.forward;
            s += Time.deltaTime;
            yield return null;
        }

        // reset alpha (pool will reassign on reuse)
        tmpText.color = startColor;

        if (FloatingLabelPool.Instance != null) FloatingLabelPool.Instance.Return(this);
        else gameObject.SetActive(false);
    }
}


