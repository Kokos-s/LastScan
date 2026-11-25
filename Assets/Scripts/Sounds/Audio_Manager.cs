using UnityEngine;

public class Audio_Manager : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip music;

    private static Audio_Manager instance;
    public static Audio_Manager Instance => instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        AudioListener.pause = false;

        if (music != null && !musicSource.isPlaying)
            PlayMusic(music);
    }

    public void PlayMusic(AudioClip musicClip)
    {
        if (musicClip == null) return;

        musicSource.ignoreListenerPause = true;
        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
}


