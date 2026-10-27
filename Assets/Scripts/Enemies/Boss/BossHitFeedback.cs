using System.Collections;
using UnityEngine;

public class BossHitFeedback : MonoBehaviour
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
    [SerializeField] private float sparkScale = 4f;     // общий множитель размера искр
    [SerializeField] private float sparkOffset = 1.5f;  // насколько вынести искры наружу от точки удара

    private Renderer[] renderers;
    private Material[][] originals;
    private float flashUntil;
    private bool flashing;

    private void Awake()
    {
        var all = GetComponentsInChildren<Renderer>(true);
        var list = new System.Collections.Generic.List<Renderer>();
        foreach (var r in all)
            if (r is MeshRenderer || r is SkinnedMeshRenderer)
                if (r.GetComponentInParent<BossDisc>() == null) list.Add(r);
        renderers = list.ToArray();
    }

    public void Play(Vector3 point, Vector3 normal)
    {
        Flash();
        PlaySound();
        SpawnSparks(point, normal);
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

    private void SpawnSparks(Vector3 point, Vector3 normal)
    {
        if (sparksPrefab == null)
        {
            Debug.LogWarning("BossHitFeedback: не назначен Sparks Prefab");
            return;
        }

        // если направление удара неизвестно (точка внутри коллайдера), разворачиваем искры к камере
        if (normal.sqrMagnitude < 0.01f)
        {
            Camera cam = Camera.main;
            normal = cam != null ? (cam.transform.position - point) : Vector3.up;
        }
        normal.Normalize();

        Vector3 pos = point + normal * sparkOffset;
        var fx = Instantiate(sparksPrefab, pos, Quaternion.LookRotation(normal));
        fx.transform.localScale *= sparkScale;
        fx.Play();
        Destroy(fx.gameObject, 3f);
    }
}