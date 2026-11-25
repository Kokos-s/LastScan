using UnityEngine;

public class MenuMusic : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnDisable()
    {
        if (audioSource != null)
            audioSource.Stop();
    }
}
