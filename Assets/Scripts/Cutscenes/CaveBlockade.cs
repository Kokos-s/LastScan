using System.Collections;
using UnityEngine;

public class CaveBlockade : MonoBehaviour
{
    [SerializeField] private GameObject[] rocks;
    [SerializeField] private Collider entranceTrigger;
    [SerializeField] private ParticleSystem dustEffect;
    [SerializeField] private Transform impactFlash;

    [Header("Тайминг")]
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float flyDuration = 1.8f;

    [Header("Вспышка")]
    [SerializeField] private float flashMaxScale = 12f;
    [SerializeField] private float flashDuration = 0.5f;

    [Header("Разлёт")]
    [SerializeField] private float flySpeed = 9f;
    [SerializeField] private float upwardSpeed = 6f;
    [SerializeField] private float spinSpeed = 360f;
    [SerializeField] private float gravity = 9.8f;
    [SerializeField] private float shakeAmount = 0.12f;

    private bool cleared = false;

    public void Clear()
    {
        if (cleared) return;
        cleared = true;
        StartCoroutine(ClearRoutine());
    }

    [ContextMenu("Test Clear")]
    public void TestClear()
    {
        Clear();
    }

    private IEnumerator ClearRoutine()
    {
        // Вспышка в точке удара
        if (impactFlash != null)
            StartCoroutine(FlashRoutine());

        // Фаза 1: короткая дрожь
        Vector3[] startPositions = new Vector3[rocks.Length];
        for (int i = 0; i < rocks.Length; i++)
        {
            if (rocks[i] != null)
                startPositions[i] = rocks[i].transform.position;
        }

        float t = 0f;
        while (t < shakeDuration)
        {
            t += Time.unscaledDeltaTime;
            float strength = shakeAmount * (t / shakeDuration);

            for (int i = 0; i < rocks.Length; i++)
            {
                if (rocks[i] != null)
                    rocks[i].transform.position = startPositions[i] + Random.insideUnitSphere * strength;
            }
            yield return null;
        }

        // Фаза 2: взрыв
        if (dustEffect != null)
        {
            var main = dustEffect.main;
            main.useUnscaledTime = true;
            dustEffect.Play();
        }

        for (int i = 0; i < rocks.Length; i++)
        {
            if (rocks[i] == null) continue;

            rocks[i].transform.position = startPositions[i];

            if (rocks[i].GetComponentInChildren<Renderer>() == null)
            {
                rocks[i].SetActive(false);
                continue;
            }

            StartCoroutine(FlyAway(rocks[i], startPositions[i]));
        }

        yield return new WaitForSecondsRealtime(flyDuration);

        if (entranceTrigger != null)
            entranceTrigger.enabled = true;
    }

    private IEnumerator FlashRoutine()
    {
        impactFlash.gameObject.SetActive(true);
        Vector3 start = Vector3.one * 0.5f;
        Vector3 end = Vector3.one * flashMaxScale;

        float t = 0f;
        while (t < flashDuration)
        {
            t += Time.unscaledDeltaTime;
            impactFlash.localScale = Vector3.Lerp(start, end, t / flashDuration);
            yield return null;
        }

        impactFlash.gameObject.SetActive(false);
    }

    private IEnumerator FlyAway(GameObject rock, Vector3 startPosition)
    {
        foreach (Collider c in rock.GetComponentsInChildren<Collider>())
            c.enabled = false;

        Transform tr = rock.transform;
        Vector3 startScale = tr.localScale;

        Vector3 outward = startPosition - transform.position;
        outward.y = 0f;
        if (outward.sqrMagnitude < 0.01f)
            outward = Random.insideUnitSphere;
        outward.y = 0f;
        outward.Normalize();

        Vector3 sideways = Random.insideUnitSphere;
        sideways.y = 0f;

        Vector3 velocity = (outward + sideways * 0.4f).normalized * flySpeed * Random.Range(0.7f, 1.3f);
        velocity.y = upwardSpeed * Random.Range(0.6f, 1.4f);

        Vector3 spinAxis = Random.onUnitSphere;
        float spin = spinSpeed * Random.Range(0.5f, 1.5f);

        float t = 0f;
        while (t < flyDuration)
        {
            float dt = Time.unscaledDeltaTime;
            t += dt;
            float progress = t / flyDuration;

            velocity.y -= gravity * dt;
            tr.position += velocity * dt;
            tr.Rotate(spinAxis, spin * dt, Space.World);

            float shrink = Mathf.Clamp01((progress - 0.5f) / 0.5f);
            tr.localScale = Vector3.Lerp(startScale, Vector3.zero, shrink);

            yield return null;
        }

        rock.SetActive(false);
    }
}