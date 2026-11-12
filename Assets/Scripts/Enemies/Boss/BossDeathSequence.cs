using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class BossDeathSequence : MonoBehaviour
{
    [SerializeField] private GameObject brokenPrefab;

    [SerializeField] private float chargeTime = 3.5f;
    [SerializeField] private float shakeMax = 0.6f;
    [SerializeField] private Color glowColor = new Color(1f, 0.55f, 0.15f);
    [SerializeField] private float glowMaxIntensity = 6f;
    [SerializeField] private float strobeSpeed = 25f;

    [SerializeField] private Color lightColor = new Color(1f, 0.7f, 0.35f);
    [SerializeField] private float lightMaxIntensity = 40f;
    [SerializeField] private float lightMaxRange = 60f;
    [SerializeField] private float flashLightIntensity = 400f;
    [SerializeField] private float lightHeight = 3f;

    [SerializeField] private float flashRiseTime = 0.3f;
    [SerializeField] private float flashHoldTime = 0.15f;
    [SerializeField] private float flashFadeTime = 1.0f;
    [SerializeField] private float flashSphereRadius = 40f;
    [SerializeField] private Material flashSphereMaterial;
    [SerializeField] private ParticleSystem explosionFx;
    [SerializeField] private float explosionFxScale = 4f;

    [SerializeField] private float cameraShakeMax = 0.8f;
    [SerializeField] private float explosionCameraShake = 2.5f;

    [SerializeField] private AudioClip humClip;
    [SerializeField] private AudioClip explosionClip;
    [SerializeField] private float humMaxVolume = 1f;
    [SerializeField] private float humMinPitch = 0.5f;
    [SerializeField] private float humMaxPitch = 1.8f;

    private Health health;
    private bool playing;

    private void OnEnable()
    {
        health = GetComponent<Health>();
        if (health != null) health.onDeath.AddListener(StartSequence);
    }

    private void OnDisable()
    {
        if (health != null) health.onDeath.RemoveListener(StartSequence);
    }

    [ContextMenu("Test Death Sequence")]
    public void StartSequence()
    {
        if (playing) return;
        playing = true;
        StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        var controller = GetComponent<BossController>();
        if (controller != null)
        {
            controller.StopAllCoroutines();
            controller.enabled = false;
        }

        foreach (var h in GetComponentsInChildren<DrillHitbox>(true))
            h.gameObject.SetActive(false);

        var agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        var wheels = GetComponent<WheelSpinner>();
        if (wheels != null) wheels.enabled = false;

        var fb = GetComponent<BossHitFeedback>();
        if (fb != null) fb.Muted = true;
        yield return new WaitForSecondsRealtime(0.15f);

        var renderers = new List<Renderer>();
        foreach (var r in GetComponentsInChildren<Renderer>())
            if (r is MeshRenderer || r is SkinnedMeshRenderer) renderers.Add(r);

        var mats = new List<Material>();
        var baseColors = new List<Color>();
        foreach (var r in renderers)
        {
            foreach (var m in r.materials)
            {
                mats.Add(m);
                baseColors.Add(m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor") : Color.white);
                if (m.HasProperty("_EmissionColor")) m.EnableKeyword("_EMISSION");
            }
        }

        Bounds bounds = renderers.Count > 0 ? renderers[0].bounds : new Bounds(transform.position, Vector3.one);
        foreach (var r in renderers) bounds.Encapsulate(r.bounds);
        Vector3 center = bounds.center;

        var lightGo = new GameObject("DeathLight");
        lightGo.transform.position = center + Vector3.up * lightHeight;
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = lightColor;
        light.intensity = 0f;
        light.range = 1f;
        light.shadows = LightShadows.None;

        var audioGo = new GameObject("DeathAudio");
        audioGo.transform.position = center;
        var audio = audioGo.AddComponent<AudioSource>();
        audio.spatialBlend = 0f;
        audio.loop = true;
        audio.volume = 0f;
        audio.pitch = humMinPitch;
        if (humClip != null) { audio.clip = humClip; audio.Play(); }

        Image overlay = CreateOverlay(out GameObject overlayGo);
        Transform sphere = CreateFlashSphere(center);
        Material sphereMat = sphere.GetComponent<MeshRenderer>().material;
        sphere.localScale = Vector3.zero;

        Vector3 basePos = transform.position;
        float camTimer = 0f;

        float t = 0f;
        while (t < chargeTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / chargeTime);
            float e = k * k;

            Vector3 rnd = Random.insideUnitSphere;
            rnd.y *= 0.3f;
            transform.position = basePos + rnd * (shakeMax * e);

            float strobe = 1f - 0.2f * (0.5f + 0.5f * Mathf.Sin(Time.time * Mathf.Lerp(6f, strobeSpeed, k)));
            float glow = glowMaxIntensity * e * strobe;
            for (int i = 0; i < mats.Count; i++)
            {
                var m = mats[i];
                if (m == null) continue;
                if (m.HasProperty("_EmissionColor"))
                    m.SetColor("_EmissionColor", glowColor * glow);
                else if (m.HasProperty("_BaseColor"))
                    m.SetColor("_BaseColor", Color.Lerp(baseColors[i], glowColor, e));
            }

            light.intensity = lightMaxIntensity * e * strobe;
            light.range = Mathf.Lerp(5f, lightMaxRange, e);

            audio.volume = humMaxVolume * Mathf.Clamp01(k * 1.5f);
            audio.pitch = Mathf.Lerp(humMinPitch, humMaxPitch, e);

            camTimer -= Time.deltaTime;
            if (camTimer <= 0f && HitFeedbackFX.Instance != null)
            {
                camTimer = 0.1f;
                HitFeedbackFX.Instance.Shake(cameraShakeMax * e, 0.15f);
            }

            yield return null;
        }

        t = 0f;
        while (t < flashRiseTime)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / flashRiseTime);

            transform.position = basePos + Random.insideUnitSphere * shakeMax;
            light.intensity = Mathf.Lerp(lightMaxIntensity, flashLightIntensity, k);
            light.range = Mathf.Lerp(lightMaxRange, lightMaxRange * 1.5f, k);
            sphere.localScale = Vector3.one * (flashSphereRadius * 2f * k);
            SetAlpha(sphereMat, 1f);
            SetOverlay(overlay, k);

            audio.volume = humMaxVolume;
            audio.pitch = humMaxPitch + k;
            yield return null;
        }

        transform.position = basePos;
        audio.Stop();
        if (explosionClip != null) audio.PlayOneShot(explosionClip);

        foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;

        if (brokenPrefab != null)
            Instantiate(brokenPrefab, transform.position, transform.rotation);

        if (explosionFx != null)
        {
            var fx = Instantiate(explosionFx, center, Quaternion.identity);
            fx.transform.localScale *= explosionFxScale;
            fx.Play();
            Destroy(fx.gameObject, 5f);
        }

        if (HitFeedbackFX.Instance != null)
            HitFeedbackFX.Instance.Shake(explosionCameraShake, 0.6f);

        SetOverlay(overlay, 1f);
        yield return new WaitForSeconds(flashHoldTime);

        t = 0f;
        while (t < flashFadeTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / flashFadeTime);
            float inv = 1f - Mathf.SmoothStep(0f, 1f, k);

            SetOverlay(overlay, inv);
            SetAlpha(sphereMat, inv);
            sphere.localScale = Vector3.one * (flashSphereRadius * 2f * (1f + k * 0.3f));
            light.intensity = flashLightIntensity * inv * inv;
            yield return null;
        }

        Destroy(overlayGo);
        Destroy(sphere.gameObject);
        Destroy(lightGo);
        Destroy(audioGo, 6f);
        foreach (var m in mats) if (m != null) Destroy(m);

        VictoryManager.Instance.ShowVictoryScreen();

        gameObject.SetActive(false);
    }

    private Image CreateOverlay(out GameObject go)
    {
        go = new GameObject("DeathFlashOverlay");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        var imgGo = new GameObject("Img");
        imgGo.transform.SetParent(go.transform, false);
        var img = imgGo.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0f);
        img.raycastTarget = false;

        var rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return img;
    }

    private void SetOverlay(Image img, float a)
    {
        if (img == null) return;
        img.color = new Color(1f, 1f, 1f, Mathf.Clamp01(a));
    }

    private Transform CreateFlashSphere(Vector3 center)
    {
        var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        s.name = "DeathFlashSphere";
        Destroy(s.GetComponent<Collider>());
        s.transform.position = center;

        var r = s.GetComponent<MeshRenderer>();
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
        r.material = flashSphereMaterial != null
            ? new Material(flashSphereMaterial)
            : new Material(Shader.Find("Sprites/Default"));
        SetAlpha(r.material, 1f);
        return s.transform;
    }

    private void SetAlpha(Material m, float a)
    {
        Color c = Color.white;
        c.a = Mathf.Clamp01(a);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Color")) m.SetColor("_Color", c);
    }
}