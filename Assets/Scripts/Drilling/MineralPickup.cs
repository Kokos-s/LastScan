using UnityEngine;
using System.Collections;

public class MineralPickup : MonoBehaviour
{
    public ParticleSystem spawnEffect;

    public float riseHeight = 0.3f;
    public float riseDuration = 0.8f;

    public float despawnDelay = 0.5f;

    private Vector3 startLocalPosition;
    private bool hasRisen = false;

    private void Awake()
    {
        startLocalPosition = transform.localPosition;
    }

    public void PlaySpawnEffect()
    {
        if (hasRisen) return;

        if (spawnEffect != null)
            spawnEffect.Play();

        StartCoroutine(RiseRoutine());
    }

    private IEnumerator RiseRoutine()
    {
        hasRisen = true;
        Vector3 startPos = startLocalPosition;
        Vector3 endPos = startPos + Vector3.up * riseHeight;

        float elapsed = 0f;
        while (elapsed < riseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / riseDuration);
            t = t * t * (3f - 2f * t);

            transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        transform.localPosition = endPos;
    }

    public void AttachTo(Transform point)
    {
        transform.SetParent(point);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void PlaceInContainerAndDespawn(Transform containerSlot)
    {
        AttachTo(containerSlot);
        StartCoroutine(DespawnRoutine());
    }

    private IEnumerator DespawnRoutine()
    {
        yield return new WaitForSeconds(despawnDelay);
        Destroy(gameObject);
    }

    public void ResetMineral(Transform originalParent)
    {
        transform.SetParent(originalParent);
        transform.localPosition = startLocalPosition;
        hasRisen = false;
    }
}