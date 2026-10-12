using UnityEngine;

public class BossHealthBar : MonoBehaviour
{
    [SerializeField] private Health boss;
    [SerializeField] private RectTransform fill;    // красная полоса
    [SerializeField] private RectTransform trail;   // светлый «хвост» (можно оставить пустым)
    [SerializeField] private float trailDelay = 0.5f;
    [SerializeField] private float trailSpeed = 0.8f;
    [SerializeField] private float hideDelay = 1.5f;

    private float trailValue = 1f;
    private float lastDamageTime;

    private void Start()
    {
        boss.onDamaged.AddListener(OnDamaged);
        boss.onDeath.AddListener(OnDeath);
        trailValue = boss.Normalized;
        SetBar(fill, boss.Normalized);
        SetBar(trail, trailValue);
    }

    private void OnDestroy()
    {
        if (boss == null) return;
        boss.onDamaged.RemoveListener(OnDamaged);
        boss.onDeath.RemoveListener(OnDeath);
    }

    private void OnDamaged(float current)
    {
        SetBar(fill, boss.Normalized);
        lastDamageTime = Time.time;
    }

    private void OnDeath()
    {
        SetBar(fill, 0f);
        Invoke(nameof(Hide), hideDelay);
    }

    private void Hide() => gameObject.SetActive(false);

    private void Update()
    {
        if (trail == null) return;

        float target = boss.Normalized;
        if (trailValue > target && Time.time - lastDamageTime > trailDelay)
        {
            trailValue = Mathf.MoveTowards(trailValue, target, trailSpeed * Time.deltaTime);
            SetBar(trail, trailValue);
        }
    }

    private static void SetBar(RectTransform rt, float value)
    {
        if (rt == null) return;
        Vector2 a = rt.anchorMax;
        a.x = Mathf.Clamp01(value);
        rt.anchorMax = a;
    }

    [ContextMenu("Test Damage")]
    private void TestDamage() => boss.TakeDamage(50f);
}