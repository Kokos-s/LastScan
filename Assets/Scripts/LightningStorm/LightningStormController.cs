using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DigitalRuby.LightningBolt;

public class LightningStormController : MonoBehaviour
{
    public LightningBoltScript lightningScript;
    public Terrain targetTerrain;

    public float cloudHeight = 80f;

    public Image flashImage;
    public float flashDuration = 0.15f;

    public AudioSource thunderAudioSource;
    public AudioClip thunderClip;
    public float maxSoundDistance = 35f;
    public float minVolume = 0.05f;

    public RoverHealth roverHealth;

    public float damageRadius = 2f;
    public float damageAmount = 3f;
    public float energyAddAmount = 25f;

    public Vector3 roverStrikeOffset = new Vector3(0f, 1.5f, 0f);

    public float minInterval = 1f;
    public float maxInterval = 2f;
    public float strikeRadius = 8f;

    private int strikeCounter = 0;
    private Coroutine flashCoroutine;

    public float ScaledStrikeRadius => strikeRadius * transform.lossyScale.x;

    private void Start()
    {
        if (lightningScript == null) lightningScript = GetComponent<LightningBoltScript>();
        if (lightningScript != null) lightningScript.ManualMode = true;

        if (thunderAudioSource == null) thunderAudioSource = GetComponent<AudioSource>();
        if (targetTerrain == null) targetTerrain = Terrain.activeTerrain;

        if (flashImage != null)
        {
            Color c = flashImage.color;
            c.a = 0f;
            flashImage.color = c;
        }

        StartCoroutine(StormRoutine());
    }

    private IEnumerator StormRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);

            strikeCounter++;

            Vector3 endPos;
            float currentRadius = ScaledStrikeRadius;

            bool isRoverUnderCloud = false;
            if (roverHealth != null)
            {
                Vector2 cloudPosFlat = new Vector2(transform.position.x, transform.position.z);
                Vector2 roverPosFlat = new Vector2(roverHealth.transform.position.x, roverHealth.transform.position.z);

                if (Vector2.Distance(cloudPosFlat, roverPosFlat) <= currentRadius)
                {
                    isRoverUnderCloud = true;
                }
            }

            if (strikeCounter % 3 == 0 && isRoverUnderCloud && roverHealth != null)
            {
                endPos = roverHealth.transform.position + roverStrikeOffset;
            }
            else
            {
                Vector2 randomCircle = Random.insideUnitCircle * currentRadius;
                float endX = transform.position.x + randomCircle.x;
                float endZ = transform.position.z + randomCircle.y;

                float endY = 0f;
                if (targetTerrain != null)
                {
                    endY = targetTerrain.SampleHeight(new Vector3(endX, 0, endZ)) + targetTerrain.transform.position.y;
                }

                endPos = new Vector3(endX, endY, endZ);
            }

            Vector3 startPos = new Vector3(endPos.x, cloudHeight, endPos.z);

            lightningScript.StartPosition = startPos;
            lightningScript.EndPosition = endPos;
            lightningScript.Trigger();

            PlayThunderSound(endPos);

            if (roverHealth != null)
            {
                float distanceToRover = Vector3.Distance(endPos, roverHealth.transform.position + roverStrikeOffset);
                if (distanceToRover <= damageRadius)
                {
                    roverHealth.TakeDamage(damageAmount);

                    RoverEnergy roverEnergy = roverHealth.GetComponent<RoverEnergy>();
                    if (roverEnergy != null)
                    {
                        roverEnergy.AddEnergy(energyAddAmount);
                    }

                    TriggerFlash();
                }
            }
        }
    }

    private void TriggerFlash()
    {
        if (flashImage == null) return;
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        Color c = flashImage.color;
        c.a = 0.85f;
        flashImage.color = c;

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(0.85f, 0f, elapsed / flashDuration);
            flashImage.color = c;
            yield return null;
        }

        c.a = 0f;
        flashImage.color = c;
    }

    private void PlayThunderSound(Vector3 strikePosition)
    {
        if (thunderAudioSource == null) return;
        float volume = 1.0f;

        if (roverHealth != null)
        {
            float distance = Vector3.Distance(strikePosition, roverHealth.transform.position);
            float t = Mathf.Clamp01(distance / maxSoundDistance);
            volume = Mathf.Lerp(1.0f, minVolume, t);
        }

        if (thunderClip != null) thunderAudioSource.PlayOneShot(thunderClip, volume);
        else
        {
            thunderAudioSource.volume = volume;
            thunderAudioSource.Play();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 centerProjection = new Vector3(transform.position.x, 0f, transform.position.z);
        Gizmos.DrawWireSphere(centerProjection, ScaledStrikeRadius);

        if (roverHealth != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(roverHealth.transform.position + roverStrikeOffset, damageRadius);
        }
    }
}