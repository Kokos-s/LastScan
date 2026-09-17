using UnityEngine;
using UnityEngine.Audio;

public class Audio_Manager : MonoBehaviour
{
    [Header("Audio_Source")]
    [SerializeField] AudioSource SFXSource;

    public void PlaySFX(AudioClip SFXClip)
    {
        SFXSource.PlayOneShot(SFXClip);
    }
}

