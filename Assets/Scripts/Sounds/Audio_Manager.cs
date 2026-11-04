using UnityEngine;
using UnityEngine.Audio;

public class Audio_Manager : MonoBehaviour
{
    //[SerializeField] AudioSource SFXSource;
    [SerializeField] AudioSource musicSource;
    public AudioClip music;

    //public void PlaySFX(AudioClip SFXClip)
    //{
    //    SFXSource.PlayOneShot(SFXClip);
    //}

    public static Audio_Manager Instance;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        if (music != null)
            PlayMusic(music);
    }
    public void PlayMusic(AudioClip musicClip)
    {
        if (musicClip == null) return;

        musicSource.ignoreListenerPause = true;
        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.Play(); ;
    }
    public void StopMusic()
    {
        musicSource.Stop();
    }

}

