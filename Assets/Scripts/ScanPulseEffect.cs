using UnityEngine;

public class ScanPulseEffect : MonoBehaviour
{
    [SerializeField] private float duration = 5f;
    [SerializeField] private float maxScale = 100f;
    private float timer = 0f;
    private Material material;
    private Color startColor;
    public float Duration
    {
        get { return duration; }
    }
    void Start()
    {
        material = GetComponent<Renderer>().material;
        startColor = material.color;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float progress = timer / duration;

        float scale = Mathf.Lerp(0f, maxScale, progress);
        transform.localScale = new Vector3(scale, scale, scale);

        float alpha = Mathf.Lerp(startColor.a, 0f, progress);
        material.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

        if (progress >= 1f)
            Destroy(gameObject);
    }
}
