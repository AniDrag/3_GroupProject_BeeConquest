using TMPro;
using UnityEngine;

public class FpsCounter : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("Assign a TextMeshProUGUI element to display FPS.")]
    [SerializeField] private TMP_Text fpsText;

    [Header("Settings")]
    [Tooltip("How often (seconds) the FPS display updates.")]
    [SerializeField] private float updateInterval = 0.25f;
    [Tooltip("Smooth the FPS value instead of showing raw changes.")]
    [SerializeField] private bool smoothing = true;
    [Range(0.01f, 1f)][SerializeField] private float smoothingFactor = 0.1f;

    private float accum = 0f;    // Accumulated FPS
    private int frames = 0;      // Frame count
    private float timeLeft;      // Interval countdown
    private float smoothedFps;   // Smoothed FPS

    void Awake()
    {
        if (fpsText == null)
        {
            Debug.LogWarning("FPSCounterTMP: No TMP_Text assigned! Creating one automatically.");

            // Create a default TMP object if none assigned
            GameObject go = new GameObject("FPS Counter TMP");
            go.transform.SetParent(transform, false);
            fpsText = go.AddComponent<TextMeshProUGUI>();
            fpsText.fontSize = 20;
            fpsText.alignment = TextAlignmentOptions.TopLeft;

            // Anchor top-left
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(10f, -10f);
            rt.sizeDelta = new Vector2(200f, 50f);
        }

        timeLeft = updateInterval;
    }

    void Update()
    {
        float delta = Time.unscaledDeltaTime;
        timeLeft -= delta;
        accum += (1f / Mathf.Max(delta, 0.00001f));
        frames++;

        if (timeLeft <= 0f)
        {
            float fps = accum / frames;

            if (smoothing)
            {
                if (smoothedFps <= 0f) smoothedFps = fps;
                smoothedFps = Mathf.Lerp(smoothedFps, fps, smoothingFactor);
                fps = smoothedFps;
            }

            float ms = 1000f / Mathf.Max(fps, 0.0001f);
            fpsText.text = $"FPS: {fps:0.0}\n{ms:0.0} ms";

            // Reset counters
            timeLeft = updateInterval;
            accum = 0f;
            frames = 0;
        }
    }
}
