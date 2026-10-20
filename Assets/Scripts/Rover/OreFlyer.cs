using UnityEngine;

public class OreFlyer : MonoBehaviour
{
    private Transform target;
    private Vector3 start;
    private Vector3 control;
    private Vector3 baseScale;
    private float delay;
    private float duration;
    private float t;

    public void Init(Transform target, float delay, float duration, float arcHeight)
    {
        this.target = target;
        this.delay = delay;
        this.duration = duration;

        start = transform.position;
        baseScale = transform.localScale;

        Vector3 end = target.position + Vector3.up * 0.8f;
        Vector3 side = Random.insideUnitSphere;
        side.y = 0f;
        control = (start + end) * 0.5f + Vector3.up * arcHeight + side * 1.5f;

        transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        if (delay > 0f)
        {
            delay -= Time.deltaTime;
            return;
        }

        t += Time.deltaTime / duration;
        float k = Mathf.Clamp01(t);

        Vector3 end = target.position + Vector3.up * 0.8f;
        Vector3 a = Vector3.Lerp(start, control, k);
        Vector3 b = Vector3.Lerp(control, end, k);
        transform.position = Vector3.Lerp(a, b, k);

        // Появляется в начале и сжимается перед попаданием в ровер
        float grow = Mathf.Clamp01(k / 0.15f);
        float shrink = Mathf.Clamp01((1f - k) / 0.2f);
        transform.localScale = baseScale * Mathf.Min(grow, shrink);

        if (k >= 1f)
            Destroy(gameObject);
    }
}