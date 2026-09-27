using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DamageVignette : MonoBehaviour
{
    public static DamageVignette Instance { get; private set; }

    [SerializeField] private Volume globalVolume;

    [SerializeField] private float peakIntensity = 0.4f;
    [SerializeField] private float fadeInTime = 0.05f;
    [SerializeField] private float fadeOutTime = 0.4f;

    private Vignette vignette;
    private Coroutine currentRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (globalVolume != null && globalVolume.profile.TryGet(out vignette))
        {
            vignette.intensity.value = 0f;
        }
        else
        {
            Debug.LogWarning("DamageVignette: не удалось найти Vignette override в профиле Volume!");
        }
    }

    public void FlashDamage()
    {
        if (vignette == null) return;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float t = 0f;

        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            vignette.intensity.value = Mathf.Lerp(0f, peakIntensity, t / fadeInTime);
            yield return null;
        }

        vignette.intensity.value = peakIntensity;

        t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            vignette.intensity.value = Mathf.Lerp(peakIntensity, 0f, t / fadeOutTime);
            yield return null;
        }

        vignette.intensity.value = 0f;
    }
}