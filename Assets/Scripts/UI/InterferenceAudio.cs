using UnityEngine;

public class InterferenceAudio : MonoBehaviour
{
    [SerializeField] private MagnetRange[] anomalies;
    [SerializeField] private float maxVolume = 0.8f;
    [SerializeField] private float fadeSpeed = 0.5f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0f;
    }

    private void Update()
    {
        float strength = 0f;

        foreach (MagnetRange anomaly in anomalies)
        {
            if (anomaly == null)
                continue;

            strength = Mathf.Max(strength, anomaly.InterferenceStrength);
        }

        float targetVolume = strength * maxVolume;

        audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, fadeSpeed * Time.deltaTime);

        if (audioSource.volume > 0f && !audioSource.isPlaying)
            audioSource.Play();

        if (audioSource.volume <= 0f && audioSource.isPlaying)
            audioSource.Stop();
    }

    private void OnDisable()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.volume = 0f;
        }
    }
}