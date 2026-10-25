using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class HitFeedbackFX : MonoBehaviour
{
    public static HitFeedbackFX Instance { get; private set; }

    private Vector3 appliedOffset;
    private float shakeTimer;
    private float shakeDuration = 0.01f;
    private float shakeAmplitude;
    private bool frozen;
    private float savedTimeScale = 1f;

    private void Awake() => Instance = this;

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void OnDisable()
    {
        if (frozen)
        {
            Time.timeScale = savedTimeScale;
            frozen = false;
        }
    }

    // Update идёт раньше всех LateUpdate: убираем прошлый сдвиг до того, как Camera Follow посчитает позицию
    private void Update()
    {
        transform.position -= appliedOffset;
        appliedOffset = Vector3.zero;
    }

    private void LateUpdate()
    {
        if (shakeTimer <= 0f) return;

        shakeTimer -= Time.unscaledDeltaTime;
        float k = Mathf.Clamp01(shakeTimer / shakeDuration);

        Vector2 r = Random.insideUnitCircle * (shakeAmplitude * k);
        appliedOffset = transform.right * r.x + transform.up * r.y;
        transform.position += appliedOffset;
    }

    public void Shake(float amplitude, float duration)
    {
        // более сильная тряска не заменяется слабой
        if (shakeTimer > 0f && amplitude < shakeAmplitude * (shakeTimer / shakeDuration)) return;

        shakeAmplitude = amplitude;
        shakeDuration = Mathf.Max(0.01f, duration);
        shakeTimer = shakeDuration;
    }

    public void HitStop(float duration)
    {
        if (frozen || Time.timeScale < 0.01f) return; // не трогаем паузу меню
        StartCoroutine(HitStopRoutine(duration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        frozen = true;
        savedTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = savedTimeScale;
        frozen = false;
    }

    [ContextMenu("Test Shake")]
    private void TestShake() => Shake(0.4f, 0.2f);

    [ContextMenu("Test HitStop")]
    private void TestHitStop() => HitStop(0.08f);
}