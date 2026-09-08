using UnityEngine;

public class AnomalyGlow : MonoBehaviour
{
    [SerializeField] private Material glowingMaterial;
    [SerializeField] private float pulseSpeed = 1.5f;
    [SerializeField] private float minIntensity = 8f;
    [SerializeField] private float maxIntensity = 12f;

    private Renderer targetRenderer;
    private Material pulseMaterial;
    private Color initialEmission;

    void Start()
    {
        targetRenderer = GetComponent<Renderer>();

        pulseMaterial = new Material(glowingMaterial);
        initialEmission = pulseMaterial.GetColor("_EmissionColor");
    }

    void Update()
    {
        if (targetRenderer.sharedMaterial == glowingMaterial)
            targetRenderer.sharedMaterial = pulseMaterial;

        if (targetRenderer.sharedMaterial != pulseMaterial)
            return;

        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);
        float brightness = Mathf.Pow(2f, intensity);

        pulseMaterial.SetColor("_EmissionColor", initialEmission * brightness);
    }

    void OnDisable()
    {
        if (targetRenderer != null &&
            targetRenderer.sharedMaterial == pulseMaterial)
        {
            targetRenderer.sharedMaterial = glowingMaterial;
        }
    }

    void OnDestroy()
    {
        if (pulseMaterial != null)
            Destroy(pulseMaterial);
    }
}