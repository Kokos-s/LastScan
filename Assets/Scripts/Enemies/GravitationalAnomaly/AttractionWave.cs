using UnityEngine;

public class AttractionWave : MonoBehaviour
{
    [SerializeField] private float startSize = 30f;
    [SerializeField] private float endSize = 1f;
    [SerializeField] private float duration = 3f;
    [SerializeField] private float startDelay = 0f;
    [SerializeField] private float maxOpacity = 0.15f;
    [SerializeField] private float fadeInDuration = 1f;

    private float timer;
    private Material waveMaterial;

    private void Awake()
    {
        waveMaterial = GetComponent<Renderer>().material;
    }

    private void OnEnable()
    {
        timer = -startDelay;
        transform.localScale = Vector3.one * startSize;
        waveMaterial.SetFloat("_WaveOpacity", 0f);
    }

    private void Update()
    {
        if (duration <= 0f)
            return;

        timer += Time.deltaTime;

        if (timer < 0f)
            return;

        timer = Mathf.Repeat(timer, duration);

        float progress = timer / duration;
        float acceleratedProgress = progress * progress;

        float size = Mathf.Lerp(startSize, endSize, acceleratedProgress);

        transform.localScale = Vector3.one * size;

        float opacity = maxOpacity;

        if (fadeInDuration > 0f)
        {
            float fadeProgress = Mathf.Clamp01(timer / fadeInDuration);
            opacity = Mathf.SmoothStep(0f, maxOpacity, fadeProgress);
        }

        waveMaterial.SetFloat("_WaveOpacity", opacity);
    }

    private void OnDisable()
    {
        if (waveMaterial != null)
            waveMaterial.SetFloat("_WaveOpacity", 0f);
    }

    private void OnDestroy()
    {
        if (waveMaterial != null)
            Destroy(waveMaterial);
    }
}