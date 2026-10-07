using UnityEngine;
using UnityEngine.InputSystem;

public class RoverAudio : MonoBehaviour
{
    [SerializeField] private RoverHealth health;
    [SerializeField] private Rigidbody rb;

    [SerializeField] private AudioClip aliveClip;
    [SerializeField] private AudioClip movingClip;
    [SerializeField] private AudioClip drillingClip;

    [SerializeField, Range(0f, 1f)] private float drillingVolume = 0.9f;
    [SerializeField] private float drillingFadeInSpeed = 4f;
    [SerializeField] private float drillingFadeOutSpeed = 1f;

    [SerializeField] private AudioClip scanClip;
    [SerializeField, Range(0f, 1f)] private float scanVolume = 1f;
    [SerializeField] private GameObject scanActiveLabel;

    [SerializeField, Range(0f, 1f)] private float aliveVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float movingVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float minMovingVolume = 0.2f;
    [SerializeField] private float fadeSpeed = 2f;

    [SerializeField] private float moveSpeedThreshold = 0.3f;

    [SerializeField] private float minPitch = 0.9f;
    [SerializeField] private float maxPitch = 1.3f;
    [SerializeField] private float maxSpeed = 17f;

    [SerializeField, Range(0f, 1f)] private float spatialBlend = 0f;

    private AudioSource aliveSource;
    private AudioSource movingSource;
    private AudioSource drillingSource;
    private AudioSource scanSource;
    private bool wasScanLabelActive;

    private void Awake()
    {
        if (health == null) health = GetComponentInParent<RoverHealth>();
        if (rb == null) rb = GetComponentInParent<Rigidbody>();

        aliveSource = CreateSource(aliveClip);
        movingSource = CreateSource(movingClip);
        drillingSource = CreateSource(drillingClip);

        scanSource = gameObject.AddComponent<AudioSource>();
        scanSource.loop = false;
        scanSource.playOnAwake = false;
        scanSource.spatialBlend = spatialBlend;
    }

    private AudioSource CreateSource(AudioClip clip)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.playOnAwake = false;
        source.volume = 0f;
        source.spatialBlend = spatialBlend;
        return source;
    }

    private void Update()
    {
        bool alive = health != null && !health.IsDead;

        float speed = rb != null ? rb.linearVelocity.magnitude : 0f;
        bool moving = alive && speed > moveSpeedThreshold;

        float t = Mathf.InverseLerp(moveSpeedThreshold, maxSpeed, speed);
        float targetMovingVolume = moving ? Mathf.Lerp(minMovingVolume, movingVolume, t) : 0f;

        UpdateSource(aliveSource, alive ? aliveVolume : 0f, fadeSpeed);
        UpdateSource(movingSource, targetMovingVolume, fadeSpeed);

        HandleScanSound(alive);

        bool drilling = alive && Keyboard.current != null && Keyboard.current.eKey.isPressed;
        UpdateSource(drillingSource, drilling ? drillingVolume : 0f,
            drilling ? drillingFadeInSpeed : drillingFadeOutSpeed);

        if (movingSource.isPlaying)
            movingSource.pitch = Mathf.Lerp(minPitch, maxPitch, t);
    }

    private void HandleScanSound(bool alive)
    {
        if (scanClip == null) return;

        bool scanStarted;

        if (scanActiveLabel != null)
        {
            bool labelActive = scanActiveLabel.activeSelf;
            scanStarted = labelActive && !wasScanLabelActive;
            wasScanLabelActive = labelActive;
        }
        else
        {
            scanStarted = alive && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        }

        if (scanStarted)
            scanSource.PlayOneShot(scanClip, scanVolume);
    }

    private void UpdateSource(AudioSource source, float targetVolume, float speed)
    {
        if (source.clip == null) return;

        if (targetVolume > 0f && !source.isPlaying)
            source.Play();

        source.volume = Mathf.MoveTowards(source.volume, targetVolume, speed * Time.deltaTime);

        if (targetVolume <= 0f && source.volume <= 0f && source.isPlaying)
            source.Stop();
    }
}