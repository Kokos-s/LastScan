using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DestructibleColumn : MonoBehaviour, IDamageable
{
    [Header("References")]
    [SerializeField] private Transform fallingPivot;
    [SerializeField] private Transform model;
    [SerializeField] private GameObject stump;

    [Header("Health")]
    [SerializeField] private float maxHealth = 12f;
    [SerializeField] private float hitShakeAmount = 0.15f;

    [Header("Fall")]
    [SerializeField] private float telegraphTime = 1f;
    [SerializeField] private float fallTime = 1.2f;
    [SerializeField] private float fallDamage = 15f;
    [SerializeField] private LayerMask damageMask;
    [SerializeField] private float hitHeight = 6f;
    [SerializeField] private float hitWidthMultiplier = 1.8f;   // ширина зоны урона (было 1.5)
    [SerializeField] private float zoneLengthMultiplier = 1.5f; // длина зоны урона относительно высоты колонны
    [SerializeField] private float shakeAmplitude = 0.8f;
    [SerializeField] private float shakeDuration = 0.4f;
    [SerializeField] private float hitStopTime = 0.1f;
    [SerializeField] private ParticleSystem impactFx;

    [Header("Fall Zone (обозначение зоны поражения)")]
    [SerializeField] private Material zoneMaterial;
    [SerializeField] private Color zoneColor = new Color(1f, 0.1f, 0.1f, 1f);
    [SerializeField] private float zoneBaseAlpha = 0.25f;
    [SerializeField] private float zoneFillAlpha = 0.55f;
    [SerializeField] private float zoneHeightAboveFloor = 0.15f;
    [SerializeField, Range(0f, 0.5f)] private float zoneCornerRadius = 0.25f;
    [SerializeField, Range(0.01f, 0.5f)] private float zoneEdgeSoftness = 0.15f;

    [Header("Rocks (обломки)")]
    [SerializeField] private GameObject rockLargePrefab;
    [SerializeField] private GameObject rockSmallPrefab;
    [SerializeField] private int largeCount = 2;
    [SerializeField] private int smallCount = 3;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask avoidMask;
    [SerializeField] private float largeClearRadius = 6f;
    [SerializeField] private float smallClearRadius = 3f;
    [SerializeField] private float largeLift = 0f;
    [SerializeField] private float smallDropHeight = 1.5f;

    private float health;
    private bool falling;
    private float height;
    private float radius;
    private Vector3 modelRestPos;
    private Collider modelCollider;
    private NavMeshObstacle modelObstacle;

    private Transform zoneBase;
    private Transform zoneFill;
    private Material zoneMatBase;
    private Material zoneMatFill;
    private Texture2D zoneTexture;
    private float zoneWidth;
    private float zoneLength;
    private Vector3 zoneDir;
    private Vector3 zoneFloor;

    private float ZoneLen => height * zoneLengthMultiplier;

    private void Awake()
    {
        health = maxHealth;
        modelRestPos = model.localPosition;
        modelCollider = model.GetComponentInChildren<Collider>();
        modelObstacle = model.GetComponentInChildren<NavMeshObstacle>();

        if (modelCollider != null)
        {
            Bounds b = modelCollider.bounds;
            height = b.size.y;
            radius = Mathf.Max(b.size.x, b.size.z) * 0.5f;
        }
        if (stump != null) stump.SetActive(false);
    }

    public void TakeDamage(float amount)
    {
        if (falling) return;

        health -= amount;
        StopAllCoroutines();

        if (health <= 0f)
        {
            falling = true;
            StartCoroutine(FallRoutine());
        }
        else
        {
            StartCoroutine(ShakeModel(0.2f, hitShakeAmount));
        }
    }

    private IEnumerator ShakeModel(float time, float amount)
    {
        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            Vector2 r = Random.insideUnitCircle * amount;
            model.localPosition = modelRestPos + new Vector3(r.x, 0f, r.y);
            yield return null;
        }
        model.localPosition = modelRestPos;
    }

    private IEnumerator FallRoutine()
    {
        Vector3 basePos = fallingPivot.position;

        Vector3 dir = transform.forward;
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) dir = basePos - player.transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) dir = transform.forward;
        dir.Normalize();
        Vector3 axis = Vector3.Cross(Vector3.up, dir);

        CreateZone(basePos, dir);

        // 1. Телеграф
        float t = 0f;
        while (t < telegraphTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / telegraphTime);

            Vector2 r = Random.insideUnitCircle * (hitShakeAmount * (0.5f + 2f * k));
            model.localPosition = modelRestPos + new Vector3(r.x, 0f, r.y);

            UpdateZone(k);
            yield return null;
        }
        model.localPosition = modelRestPos;

        if (modelObstacle != null) modelObstacle.enabled = false;
        if (modelCollider != null) modelCollider.enabled = false;

        // 2. Падение
        Quaternion startRot = fallingPivot.rotation;
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fallTime;
            float a = 90f * t * t;
            fallingPivot.rotation = Quaternion.AngleAxis(a, axis) * startRot;
            UpdateZone(1f);
            yield return null;
        }
        fallingPivot.rotation = Quaternion.AngleAxis(90f, axis) * startRot;

        // 3. Удар об пол
        DestroyZone();
        bool hitSomething = DealImpactDamage(basePos, dir);

        if (HitFeedbackFX.Instance != null)
        {
            HitFeedbackFX.Instance.Shake(shakeAmplitude, shakeDuration);
            if (hitSomething) HitFeedbackFX.Instance.HitStop(hitStopTime);
        }

        if (impactFx != null)
        {
            var fx = Instantiate(impactFx, basePos + dir * height, Quaternion.identity);
            fx.Play();
            Destroy(fx.gameObject, 4f);
        }

        // 4. Пенёк и обломки
        fallingPivot.gameObject.SetActive(false);
        if (stump != null) stump.SetActive(true);
        SpawnRocks(basePos, dir);
    }

    // ---------------- ЗОНА ПОРАЖЕНИЯ ----------------
    private void CreateZone(Vector3 basePos, Vector3 dir)
    {
        zoneWidth = radius * hitWidthMultiplier * 2f;   // та же ширина, что у OverlapBox
        zoneLength = ZoneLen;                           // та же длина
        zoneDir = dir;
        zoneFloor = OnGround(basePos, basePos.y) + Vector3.up * zoneHeightAboveFloor;

        zoneTexture = BuildRoundedTexture(zoneWidth, zoneLength);

        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);

        Material baseMat = zoneMaterial != null ? zoneMaterial : new Material(Shader.Find("Sprites/Default"));
        zoneMatBase = new Material(baseMat);
        zoneMatFill = new Material(baseMat);
        SetTexture(zoneMatBase, zoneTexture);
        SetTexture(zoneMatFill, zoneTexture);

        zoneBase = MakeQuad("FallZone_Base", zoneMatBase).transform;
        zoneBase.SetPositionAndRotation(zoneFloor + dir * (zoneLength * 0.5f), rot);
        zoneBase.localScale = new Vector3(zoneWidth, zoneLength, 1f);

        zoneFill = MakeQuad("FallZone_Fill", zoneMatFill).transform;
        zoneFill.SetPositionAndRotation(zoneFloor + Vector3.up * 0.02f, rot);
        zoneFill.localScale = new Vector3(zoneWidth, 0.01f, 1f);

        SetAlpha(zoneMatBase, zoneBaseAlpha);
        SetAlpha(zoneMatFill, zoneFillAlpha);
    }

    private Texture2D BuildRoundedTexture(float width, float length)
    {
        float ppu = 256f / Mathf.Max(width, length);
        int w = Mathf.Clamp(Mathf.CeilToInt(width * ppu), 8, 256);
        int h = Mathf.Clamp(Mathf.CeilToInt(length * ppu), 8, 256);

        float halfW = width * 0.5f;
        float halfH = length * 0.5f;
        float r = Mathf.Min(width * zoneCornerRadius, Mathf.Min(halfW, halfH));
        float soft = Mathf.Max(0.0001f, width * zoneEdgeSoftness);

        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        var pixels = new Color32[w * h];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float px = ((x + 0.5f) / w - 0.5f) * width;
                float py = ((y + 0.5f) / h - 0.5f) * length;

                float qx = Mathf.Abs(px) - (halfW - r);
                float qy = Mathf.Abs(py) - (halfH - r);
                float d = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude
                          + Mathf.Min(Mathf.Max(qx, qy), 0f) - r;

                float a = Mathf.Clamp01(-d / soft);
                a = a * a * (3f - 2f * a);
                pixels[y * w + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
        }
        tex.SetPixels32(pixels);
        tex.Apply(false, true);
        return tex;
    }

    private void SetTexture(Material m, Texture2D tex)
    {
        if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", tex);
        if (m.HasProperty("_MainTex")) m.SetTexture("_MainTex", tex);
    }

    private void SetTextureScale(Material m, Vector2 scale)
    {
        if (m.HasProperty("_BaseMap")) m.SetTextureScale("_BaseMap", scale);
        if (m.HasProperty("_MainTex")) m.SetTextureScale("_MainTex", scale);
    }

    private GameObject MakeQuad(string name, Material mat)
    {
        var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
        q.name = name;
        Destroy(q.GetComponent<Collider>());
        var r = q.GetComponent<MeshRenderer>();
        r.sharedMaterial = mat;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
        q.layer = 0;
        return q;
    }

    private void UpdateZone(float k)
    {
        if (zoneBase == null) return;

        float fillLen = Mathf.Max(0.01f, zoneLength * k);
        zoneFill.localScale = new Vector3(zoneWidth, fillLen, 1f);
        SetTextureScale(zoneMatFill, new Vector2(1f, Mathf.Max(0.001f, k)));
        zoneFill.position = zoneFloor + Vector3.up * 0.02f + zoneDir * (fillLen * 0.5f);

        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * Mathf.Lerp(6f, 25f, k));
        SetAlpha(zoneMatBase, zoneBaseAlpha * Mathf.Lerp(0.6f, 1.4f, pulse));
        SetAlpha(zoneMatFill, zoneFillAlpha);
    }

    private void SetAlpha(Material m, float a)
    {
        Color c = zoneColor;
        c.a = Mathf.Clamp01(a);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Color")) m.SetColor("_Color", c);
    }

    private void DestroyZone()
    {
        if (zoneBase != null) Destroy(zoneBase.gameObject);
        if (zoneFill != null) Destroy(zoneFill.gameObject);
        if (zoneMatBase != null) Destroy(zoneMatBase);
        if (zoneMatFill != null) Destroy(zoneMatFill);
        if (zoneTexture != null) Destroy(zoneTexture);
        zoneBase = null;
        zoneFill = null;
    }

    private void OnDestroy() => DestroyZone();

    // ---------------- УРОН И ОБЛОМКИ ----------------
    private bool DealImpactDamage(Vector3 basePos, Vector3 dir)
    {
        float len = ZoneLen;
        Vector3 center = basePos + dir * (len * 0.5f) + Vector3.up * (hitHeight * 0.5f);
        Vector3 half = new Vector3(radius * hitWidthMultiplier, hitHeight * 0.5f, len * 0.5f);

        var hits = Physics.OverlapBox(center, half, Quaternion.LookRotation(dir), damageMask,
                                      QueryTriggerInteraction.Ignore);
        var done = new HashSet<IDamageable>();
        bool any = false;

        foreach (var h in hits)
        {
            var target = h.GetComponentInParent<IDamageable>();
            if (target == null || ReferenceEquals(target, this)) continue;
            if (!done.Add(target)) continue;

            target.TakeImpactDamage(fallDamage);
            any = true;

            var fb = h.GetComponentInParent<BossHitFeedback>();
            if (fb != null)
                fb.Play(h.bounds.ClosestPoint(center), Vector3.up);
        }
        return any;
    }

    private void SpawnRocks(Vector3 basePos, Vector3 dir)
    {
        Vector3 side = Vector3.Cross(Vector3.up, dir);

        for (int i = 0; i < largeCount; i++)
        {
            if (rockLargePrefab == null) break;
            Vector3 pos = RandomSpot(basePos, dir, side);
            pos = FindFreeSpot(pos, side, largeClearRadius);
            pos = OnGround(pos, basePos.y);
            Instantiate(rockLargePrefab, pos + Vector3.up * largeLift,
                        Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
        }

        for (int i = 0; i < smallCount; i++)
        {
            if (rockSmallPrefab == null) break;
            Vector3 pos = RandomSpot(basePos, dir, side);
            pos = FindFreeSpot(pos, side, smallClearRadius);
            pos = OnGround(pos, basePos.y);
            Instantiate(rockSmallPrefab, pos + Vector3.up * smallDropHeight, Random.rotation);
        }
    }

    // обломки остаются там, где колонна реально упала (в пределах высоты), а не по всей длинной зоне
    private Vector3 RandomSpot(Vector3 basePos, Vector3 dir, Vector3 side)
    {
        return basePos + dir * Random.Range(height * 0.45f, height)
                       + side * Random.Range(-radius * 1.5f, radius * 1.5f);
    }

    private Vector3 FindFreeSpot(Vector3 pos, Vector3 side, float clearRadius)
    {
        for (int i = 0; i < 8; i++)
        {
            if (!Physics.CheckSphere(pos, clearRadius, avoidMask, QueryTriggerInteraction.Ignore))
                return pos;
            float sign = (i % 2 == 0) ? 1f : -1f;
            pos += side * sign * clearRadius * (1 + i / 2);
        }
        return pos;
    }

    private Vector3 OnGround(Vector3 pos, float fallbackY)
    {
        if (Physics.Raycast(pos + Vector3.up * 30f, Vector3.down, out RaycastHit hit, 100f,
                            groundMask, QueryTriggerInteraction.Ignore))
            pos.y = hit.point.y;
        else
            pos.y = fallbackY;
        return pos;
    }
}