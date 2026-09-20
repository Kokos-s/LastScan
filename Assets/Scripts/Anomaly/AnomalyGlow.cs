using UnityEngine;

public class AnomalyGlow : MonoBehaviour
{
    [SerializeField] private Material glowingMaterial;
    [SerializeField] private float flashDuration = 0.5f;
    [SerializeField] private float pulseSpeed = 1.5f;
    [SerializeField] private float minIntensity = 8f;
    [SerializeField] private float maxIntensity = 12f;

    private Renderer targetRenderer;
    private Material transparentMaterial;
    private Material pulseMaterial;
    private Color initialEmission;
    private float flashTimer;

    private void Awake()
    {
        targetRenderer = GetComponent<Renderer>();
        transparentMaterial = targetRenderer.sharedMaterial;

        pulseMaterial = new Material(glowingMaterial);
        initialEmission = pulseMaterial.GetColor("_EmissionColor");
    }

    public void Flash()
    {
        targetRenderer.sharedMaterial = pulseMaterial;
        flashTimer = flashDuration;
    }

    private void Update()
    {
        if (flashTimer <= 0f)
            return;

        flashTimer -= Time.deltaTime;

        if (flashTimer <= 0f)
        {
            targetRenderer.sharedMaterial = transparentMaterial;
            return;
        }

        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);
        float brightness = Mathf.Pow(2f, intensity);

        pulseMaterial.SetColor("_EmissionColor", initialEmission * brightness);
    }

    private void OnDisable()
    {
        flashTimer = 0f;

        if (targetRenderer != null)
            targetRenderer.sharedMaterial = transparentMaterial;
    }

    private void OnDestroy()
    {
        if (pulseMaterial != null)
            Destroy(pulseMaterial);
    }
}