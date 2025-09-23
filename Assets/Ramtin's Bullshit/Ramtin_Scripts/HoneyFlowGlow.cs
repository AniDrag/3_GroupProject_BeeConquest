using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class HoneyFlowGlow : MonoBehaviour
{
    [Header("Flow Settings")]
    [Tooltip("Direction and speed of the honey flow.")]
    public Vector2 flowSpeed = new Vector2(0f, -0.2f);

    [Tooltip("Set to true if you want random start offset.")]
    public bool randomizeOffset = true;

    [Header("Glow Settings")]
    [Tooltip("Base emission intensity (minimum).")]
    public float baseEmission = 0.5f;

    [Tooltip("Maximum emission intensity during glow pulses.")]
    public float maxEmission = 2.5f;

    [Tooltip("How fast the emission pulses.")]
    public float glowSpeed = 2f;

    private Renderer rend;
    private Material mat;
    private Vector2 uvOffset;
    private float glowTime;

    void Start()
    {
        rend = GetComponent<Renderer>();
        // Use unique material instance
        mat = rend.material;

        if (randomizeOffset)
            uvOffset = new Vector2(Random.value, Random.value);
        else
            uvOffset = Vector2.zero;

        glowTime = Random.Range(0f, 100f); // randomize starting glow
    }

    void Update()
    {
        // Animate texture flow
        uvOffset += flowSpeed * Time.deltaTime;
        mat.mainTextureOffset = uvOffset;

        // Animate emission intensity
        glowTime += Time.deltaTime * glowSpeed;
        float t = (Mathf.Sin(glowTime) + 1f) * 0.5f; // oscillates between 0–1
        float emissionStrength = Mathf.Lerp(baseEmission, maxEmission, t);

        // Set emission color (scaled)
        if (mat.HasProperty("_EmissionColor"))
        {
            Color baseColor = mat.GetColor("_EmissionColor");
            Color emissionColor = baseColor * emissionStrength;
            mat.SetColor("_EmissionColor", emissionColor);

            // Updates Unity's GI system so the glow lights nearby objects
            DynamicGI.SetEmissive(rend, emissionColor);
        }
    }
}
