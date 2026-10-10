using UnityEngine;

public class BatteryFeedback : MonoBehaviour
{
    public static BatteryFeedback Instance { get; private set; }

    [SerializeField] private ParticleSystem collectParticlesPrefab;
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private float pitchMin = 0.95f;
    [SerializeField] private float pitchMax = 1.05f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlayCollectEffect(Vector3 position)
    {
        if (collectParticlesPrefab != null)
        {
            ParticleSystem instance = Instantiate(collectParticlesPrefab, position, Quaternion.identity);
            instance.Play();
            Destroy(instance.gameObject, instance.main.duration + instance.main.startLifetime.constantMax);
        }

        if (collectSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(pitchMin, pitchMax);
            audioSource.PlayOneShot(collectSound);
        }
    }
}