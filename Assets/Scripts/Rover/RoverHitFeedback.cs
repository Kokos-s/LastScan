using System.Collections;
using UnityEngine;

public class RoverHitFeedback : MonoBehaviour
{
    [Header("Flash")]
    [SerializeField] private Material flashMaterial;
    [SerializeField] private float flashTime = 0.08f;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] hitClips;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.9f, 1.1f);

    [Header("Sparks")]
    [SerializeField] private ParticleSystem sparksPrefab;
    [SerializeField] private float sparkScale = 4f;
    [SerializeField] private float sparkOffset = 1.5f;

    [Header("Screen")]
    [SerializeField] private float strongHitDamage = 2f;   // с этого урона включаются тряска и замирание
    [SerializeField] private float shakeAmplitude = 0.4f;
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float hitStopTime = 0.05f;

    private Renderer[] renderers;
    private Material[][] originals;
    private float flashUntil;
    private bool flashing;
    private Collider bodyCollider;

    private void Awake()
    {
        bodyCollider = GetComponent<Collider>();

        var list = new System.Collections.Generic.List<Renderer>();
        foreach (var r in GetComponentsInChildren<Renderer>(true))
            if (r is MeshRenderer || r is SkinnedMeshRenderer) list.Add(r);
        renderers = list.ToArray();
    }

    public void Play(float damage)
    {
        Flash();
        PlaySound();
        SpawnSparks();

        if (damage >= strongHitDamage && HitFeedbackFX.Instance != null)
        {
            HitFeedbackFX.Instance.Shake(shakeAmplitude, shakeDuration);
            HitFeedbackFX.Instance.HitStop(hitStopTime);
        }
    }

    private void Flash()
    {
        if (flashMaterial == null) return;
        flashUntil = Time.unscaledTime + flashTime;
        if (!flashing) StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        flashing = true;

        originals = new Material[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
        {
            originals[i] = renderers[i].sharedMaterials;
            var white = new Material[originals[i].Length];
            for (int m = 0; m < white.Length; m++) white[m] = flashMaterial;
            renderers[i].sharedMaterials = white;
        }

        while (Time.unscaledTime < flashUntil) yield return null;

        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i] != null) renderers[i].sharedMaterials = originals[i];

        flashing = false;
    }

    private void PlaySound()
    {
        if (audioSource == null || hitClips == null || hitClips.Length == 0) return;
        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(hitClips[Random.Range(0, hitClips.Length)]);
    }

    private void SpawnSparks()
    {
        if (sparksPrefab == null) return;

        Vector3 center = bodyCollider != null ? bodyCollider.bounds.center : transform.position;
        Camera cam = Camera.main;
        Vector3 normal = cam != null ? (cam.transform.position - center).normalized : Vector3.up;

        var fx = Instantiate(sparksPrefab, center + normal * sparkOffset, Quaternion.LookRotation(normal));
        fx.transform.localScale *= sparkScale;
        fx.Play();
        Destroy(fx.gameObject, 3f);
    }
}