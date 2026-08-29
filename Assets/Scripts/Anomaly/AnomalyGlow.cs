using UnityEngine;

public class AnomalyGlow : MonoBehaviour
{
    [SerializeField] private float pulseSpeed = 1.5f;
    [SerializeField] private float minIntensity = 1f;
    [SerializeField] private float maxIntensity = 4f;
    [SerializeField] private Color glowBaseColor = new Color(0.3f, 0.6f, 1f);
    private Material material;

    void Start()
    {
        material = GetComponent<Renderer>().material;
    }

    void Update()
    {
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);
        material.SetColor("_EmissionColor", glowBaseColor * intensity);
    }
}
