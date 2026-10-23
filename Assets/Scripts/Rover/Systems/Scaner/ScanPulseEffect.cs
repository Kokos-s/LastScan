using System.Collections.Generic;
using UnityEngine;

public class ScanPulseEffect : MonoBehaviour
{
    public static readonly List<ScanPulseEffect> All = new List<ScanPulseEffect>();

    [SerializeField] private float duration = 5f;
    [SerializeField] private float maxScale = 800f;
    private float timer = 0f;
    private Material material;
    private Color startColor;
    private Renderer[] renderers;
    private int colorProperty = -1;   // -1: у шейдера нет цветового свойства

    private static readonly string[] ColorNames = { "_Color", "_BaseColor", "_TintColor", "_MainColor" };

    public float Duration
    {
        get { return duration; }
    }

    private void OnEnable() => All.Add(this);
    private void OnDisable() => All.Remove(this);

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();

        Renderer main = GetComponent<Renderer>();
        if (main == null) return;

        material = main.material;

        foreach (string name in ColorNames)
        {
            if (material.HasProperty(name))
            {
                colorProperty = Shader.PropertyToID(name);
                startColor = material.GetColor(colorProperty);
                break;
            }
        }
    }

    // Прячет или показывает сферу, не останавливая сам скан
    public void SetVisible(bool visible)
    {
        if (renderers == null)
            renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            if (r != null)
                r.enabled = visible;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        float progress = timer / duration;

        float scale = Mathf.Lerp(0f, maxScale, progress);
        transform.localScale = new Vector3(scale, scale, scale);

        // Затухание через цвет, если шейдер его поддерживает
        if (material != null && colorProperty != -1)
        {
            float alpha = Mathf.Lerp(startColor.a, 0f, progress);
            material.SetColor(colorProperty, new Color(startColor.r, startColor.g, startColor.b, alpha));
        }

        if (progress >= 1f)
            Destroy(gameObject);
    }
}