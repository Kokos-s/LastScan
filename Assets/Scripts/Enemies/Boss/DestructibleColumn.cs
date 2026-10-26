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
    [SerializeField] private LayerMask damageMask;          // Boss + Player
    [SerializeField] private float hitHeight = 6f;          // высота зоны урона над полом
    [SerializeField] private float hitWidthMultiplier = 1.5f;
    [SerializeField] private float shakeAmplitude = 0.8f;
    [SerializeField] private float shakeDuration = 0.4f;
    [SerializeField] private float hitStopTime = 0.1f;
    [SerializeField] private ParticleSystem impactFx;       // пыль, необязательно

    [Header("Rocks (обломки)")]
    [SerializeField] private GameObject rockLargePrefab;
    [SerializeField] private GameObject rockSmallPrefab;
    [SerializeField] private int largeCount = 2;
    [SerializeField] private int smallCount = 3;
    [SerializeField] private LayerMask groundMask;          // Ground
    [SerializeField] private LayerMask avoidMask;           // Boss + Player
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

        // направление падения: от ровера, фиксируется в момент последнего удара
        Vector3 dir = transform.forward;
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) dir = basePos - player.transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) dir = transform.forward;
        dir.Normalize();
        Vector3 axis = Vector3.Cross(Vector3.up, dir);

        // 1. Телеграф: колонна трещит и трясётся всё сильнее
        float t = 0f;
        while (t < telegraphTime)
        {
            t += Time.deltaTime;
            Vector2 r = Random.insideUnitCircle * (hitShakeAmount * (0.5f + 2f * t / telegraphTime));
            model.localPosition = modelRestPos + new Vector3(r.x, 0f, r.y);
            yield return null;
        }
        model.localPosition = modelRestPos;

        // колонна больше не блокирует путь и не толкает ровер
        if (modelObstacle != null) modelObstacle.enabled = false;
        if (modelCollider != null) modelCollider.enabled = false;

        // 2. Падение с ускорением
        Quaternion startRot = fallingPivot.rotation;
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fallTime;
            float a = 90f * t * t;
            fallingPivot.rotation = Quaternion.AngleAxis(a, axis) * startRot;
            yield return null;
        }
        fallingPivot.rotation = Quaternion.AngleAxis(90f, axis) * startRot;

        // 3. Удар об пол
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

        // 4. Колонна исчезает, остаётся пенёк и обломки
        fallingPivot.gameObject.SetActive(false);
        if (stump != null) stump.SetActive(true);
        SpawnRocks(basePos, dir);
    }

    private bool DealImpactDamage(Vector3 basePos, Vector3 dir)
    {
        Vector3 center = basePos + dir * (height * 0.5f) + Vector3.up * (hitHeight * 0.5f);
        Vector3 half = new Vector3(radius * hitWidthMultiplier, hitHeight * 0.5f, height * 0.5f);

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

    private Vector3 RandomSpot(Vector3 basePos, Vector3 dir, Vector3 side)
    {
        // вдоль линии падения, дальше от пенька
        return basePos + dir * Random.Range(height * 0.45f, height)
                       + side * Random.Range(-radius * 1.5f, radius * 1.5f);
    }

    // сдвигаем точку в сторону, если на ней стоит босс или ровер
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