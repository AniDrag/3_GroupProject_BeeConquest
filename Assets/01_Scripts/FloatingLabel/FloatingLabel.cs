// client
using UnityEngine;
using TMPro;
using System.Collections;

public class FloatingLabel : MonoBehaviour
{
    [Header("Refs")]
    public TMP_Text tmpText;

    [Header("Movement")]
    public Vector3 baseRisePerSecond = new Vector3(0f, 1f, 0f);
    public float visibleDuration = 1f;
    public float shrinkDuration = 0.5f;
    public float riseRampPortion = 0.25f; // portion of visibleDuration used to ramp from initial -> target speed

    [Header("Shake")]
    public float shakeMaxByValue = 0.8f; // max amplitude per value (you asked max 0.8)
    public float shakeFrequency = 8f;

    [Header("Tilt / Roll")]
    public float maxTiltDegrees = 18f;     // max static tilt (roll) at max value
    public float tiltOscillationDegrees = 6f; // oscillation amplitude added on top
    public float tiltFrequency = 2f;       // oscillation speed

    [Header("Height mapping")]
    public float valueMin = 1f;
    public float valueMax = 100000f;
    public float maxExtraHeightMultiplier = 4f;

    [Header("Start-rise mapping (interpreted)")]
    [Tooltip("Normalized value (0..1) at which startRisePeak is reached (e.g. 0.4)")]
    [Range(0.01f, 0.99f)] public float startRiseCap = 0.4f;
    [Tooltip("How much upward speed (fraction of full) to start with when value==startRiseCap (e.g. 0.1)")]
    [Range(0f, 1f)] public float startRisePeak = 0.1f;

    [Header("Coloring")]
    public bool useGradientForColor = false;
    public Gradient colorGradient;
    public float gradientValueMax = 100f;
    [Range(0f, 1f)] public float maxRedOverlay = 0.5f;

    [HideInInspector] public long numericValue = 1;

    // internal
    Coroutine running;
    float _randSeed;
    float _randSign;

    // Show overloads
    public void Show(string text, Color color, Vector3 worldPos)
    {
        // try to parse numeric value from text; fallback to 1 if parse fails
        long parsed = 1;
        if (!long.TryParse(text, out parsed)) parsed = 1;
        Show(text, color, worldPos, parsed);
    }

    public void Show(string text, Color color, Vector3 worldPos, long value)
    {
        if (tmpText == null)
        {
            Debug.LogError($"[FloatingLabel.Show] tmpText is null on '{gameObject.name}'. Returning to pool to avoid crash.", this);
            if (FloatingLabelPool.Instance != null) FloatingLabelPool.Instance.Return(this);
            else gameObject.SetActive(false);
            return;
        }

        numericValue = Mathf.Max(0, (int)value); // allow zero per earlier examples
        transform.position = worldPos;
        tmpText.text = text;
        tmpText.color = color; // base color is the color of the collected thing
        transform.localScale = Vector3.one;

        // unique randomness per-show so shakes differ between labels
        _randSeed = Random.Range(0f, 1000f);
        _randSign = Random.value > 0.5f ? 1f : -1f;

        gameObject.SetActive(true);
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(AnimateAndReturn());
    }

    // helper to billboard to camera while applying a roll (degrees)
    private void FaceCameraWithRoll(float rollDegrees)
    {

        // direction from label -> camera ensures the front of the mesh faces the camera
        Vector3 toCamera = Camera.main.transform.position - transform.position;
        if (toCamera.sqrMagnitude < 0.000001f) return;

        Quaternion face = Quaternion.LookRotation(-toCamera, Vector3.up);

        // apply roll on top (tilt around forward axis)
        transform.rotation = face * Quaternion.Euler(0f, 0f, rollDegrees);
    }

    // full coroutine
    IEnumerator AnimateAndReturn()
    {
        float t = 0f;
        Color startColor = tmpText.color;
        Vector3 startScale = transform.localScale;
        Vector3 logicalPos = transform.position; // accumulates rise without shake

        // normalize numeric value for use in various mappings (0..1)
        float valueNorm = Mathf.InverseLerp(valueMin, valueMax, Mathf.Clamp((float)numericValue, valueMin, valueMax));

        // height multiplier (1..maxExtraHeightMultiplier)
        float heightMultiplier = 1f + valueNorm * (maxExtraHeightMultiplier - 1f);

        // piecewise start-rise factor:
        float startRiseFactor;
        if (valueNorm <= startRiseCap)
            startRiseFactor = Mathf.Lerp(0f, startRisePeak, (startRiseCap <= 0f) ? 1f : (valueNorm / startRiseCap));
        else
            startRiseFactor = Mathf.Lerp(startRisePeak, 1f, (valueNorm - startRiseCap) / (1f - startRiseCap));

        // rise speeds
        Vector3 targetRiseSpeed = baseRisePerSecond * heightMultiplier;
        Vector3 initialRiseSpeed = targetRiseSpeed * startRiseFactor;

        // shake amplitude derived from value (min 0, max shakeMaxByValue)
        float shakeAmpFromValue = Mathf.Lerp(0f, shakeMaxByValue, valueNorm);

        // gradient sample if used (maps numeric value -> 0..1 over gradientValueMax)
        float gradientSample = Mathf.Clamp01((float)numericValue / Mathf.Max(0.0001f, gradientValueMax));

        // ramp time for rising from initial -> target
        float rampTime = visibleDuration * Mathf.Clamp01(riseRampPortion);

        // VISIBLE PHASE
        while (t < visibleDuration)
        {
            float p = t / visibleDuration;

            // compute roll: static tilt depending on value + oscillation using seed
            float staticTilt = Mathf.Lerp(0f, maxTiltDegrees, valueNorm) * _randSign;
            float osc = Mathf.Sin((Time.time + _randSeed) * tiltFrequency) * tiltOscillationDegrees * valueNorm;
            float roll = staticTilt + osc;

            // face camera + apply roll
            FaceCameraWithRoll(roll);

            // ramp current speed from initial -> target over rampTime
            float rampT = (rampTime <= 0f) ? 1f : Mathf.Clamp01(t / rampTime);
            Vector3 currSpeed = Vector3.Lerp(initialRiseSpeed, targetRiseSpeed, rampT);

            // integrate logical position (no shake)
            logicalPos += currSpeed * Time.deltaTime;

            // generate unique Perlin-based shake per label (seeded by _randSeed)
            float shakeAmp = shakeAmpFromValue * (1f - p); // taper shake during visible phase
            float nx = (Mathf.PerlinNoise(Time.time * shakeFrequency + _randSeed, _randSeed) * 2f - 1f);
            float ny = (Mathf.PerlinNoise(_randSeed, Time.time * shakeFrequency + _randSeed) * 2f - 1f);
            float nz = (Mathf.PerlinNoise(Time.time * shakeFrequency + _randSeed * 0.73f, Time.time * (shakeFrequency * 0.37f) + _randSeed) * 2f - 1f);
            Vector3 shake = new Vector3(nx, ny, nz) * shakeAmp;

            transform.position = logicalPos + shake;

            // color: either gradient sampled by numeric value OR add red overlay to base color
            if (useGradientForColor && colorGradient != null)
            {
                Color g = colorGradient.Evaluate(gradientSample);
                g.a = startColor.a;
                tmpText.color = g;
            }
            else
            {
                float redOverlay = Mathf.Lerp(0f, maxRedOverlay, valueNorm);
                Color added = startColor + Color.red * redOverlay;
                tmpText.color = new Color(Mathf.Clamp01(added.r), Mathf.Clamp01(added.g), Mathf.Clamp01(added.b), startColor.a);
            }

            t += Time.deltaTime;
            yield return null;
        }

        // end-of-visible logical position
        Vector3 visibleEndPos = logicalPos;

        // SHRINK + FADE phase
        float s = 0f;
        while (s < shrinkDuration)
        {
            float p = s / shrinkDuration;

            // keep facing camera while having a smaller tilt/oscillation during shrink
            float staticTilt = Mathf.Lerp(0f, maxTiltDegrees * 0.5f, valueNorm) * _randSign;
            float osc = Mathf.Sin((Time.time + _randSeed) * tiltFrequency * 1.3f) * tiltOscillationDegrees * valueNorm * (1f - p);
            float roll = staticTilt + osc;
            FaceCameraWithRoll(roll);

            // scale down
            float scale = Mathf.Lerp(1f, 0f, p);
            transform.localScale = startScale * scale;

            // fade alpha
            float alpha = Mathf.Lerp(1f, 0f, p);
            if (useGradientForColor && colorGradient != null)
            {
                Color g = colorGradient.Evaluate(gradientSample);
                g.a = startColor.a * alpha;
                tmpText.color = g;
            }
            else
            {
                float redOverlay = Mathf.Lerp(0f, maxRedOverlay, valueNorm);
                Color added = startColor + Color.red * redOverlay;
                tmpText.color = new Color(Mathf.Clamp01(added.r), Mathf.Clamp01(added.g), Mathf.Clamp01(added.b), startColor.a * alpha);
            }

            // small residual shrink shake
            float shrinkShakeAmp = shakeAmpFromValue * 0.35f * (1f - p);
            float rx = (Mathf.PerlinNoise(Time.time * shakeFrequency * 1.5f + _randSeed * 2f, 77f) * 2f - 1f);
            float ry = (Mathf.PerlinNoise(99f, Time.time * shakeFrequency * 1.5f + _randSeed * 2f) * 2f - 1f);
            Vector3 shrinkShake = new Vector3(rx, ry, 0f) * shrinkShakeAmp;

            transform.position = visibleEndPos + shrinkShake;

            s += Time.deltaTime;
            yield return null;
        }

        // reset state expected by pooling
        tmpText.color = startColor;
        transform.localScale = startScale;
        FaceCameraWithRoll(0f); // align to camera without extra roll

        if (FloatingLabelPool.Instance != null) FloatingLabelPool.Instance.Return(this);
        else gameObject.SetActive(false);
    }

}
