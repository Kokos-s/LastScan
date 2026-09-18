using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DigitalRuby.LightningBolt;

public class LightningStormController : MonoBehaviour
{
    [Header("Ссылки на объекты")]
    public LightningBoltScript lightningScript;
    public Terrain targetTerrain;

    [Header("Высота молнии")]
    [Tooltip("Высота (по Y), с которой всегда будет начинаться молния")]
    public float cloudHeight = 80f;

    [Header("Эффект вспышки")]
    public Image flashImage;
    public float flashDuration = 0.15f;

    [Header("Звук грома")]
    public AudioSource thunderAudioSource;
    public AudioClip thunderClip;
    public float maxSoundDistance = 35f;
    public float minVolume = 0.05f;

    [Header("Настройки урона и энергии")]
    public RoverHealth roverHealth;

    [Tooltip("Радиус от точки удара молнии, в пределах которого ровер получит урон/заряд.")]
    public float damageRadius = 2f;
    public float damageAmount = 3f;
    [Tooltip("Количество энергии, восполняемое роверу при ударе молнии")]
    public float energyAddAmount = 25f;

    [Header("Смещение точки удара в ровер")]
    [Tooltip("Высота над центром ровера, куда ударяет молния")]
    public Vector3 roverStrikeOffset = new Vector3(0f, 1.5f, 0f);

    [Header("Настройки шторма")]
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

            // Определяем точку приземления (EndPosition)
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

            // Стартовая точка молнии: берутся X и Z из точки удара (или центра тучи), но Y фиксируется на высотой cloudHeight (80)
            Vector3 startPos = new Vector3(endPos.x, cloudHeight, endPos.z);

            // Передаем координаты в скрипт молнии
            lightningScript.StartPosition = startPos;
            lightningScript.EndPosition = endPos;
            lightningScript.Trigger();

            PlayThunderSound(endPos);

            // Проверяем урон роверу
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
                        Debug.Log($"⚡ Молния зарядила ровер на {energyAddAmount} ед. энергии!");
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